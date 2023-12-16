using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.WindowsStore;
using Pathfinding.Serialization;
#if UNITY_WINRT && !UNITY_EDITOR
//using MarkerMetro.Unity.WinLegacy.IO;
//using MarkerMetro.Unity.WinLegacy.Reflection;
#endif

namespace Pathfinding {
	[System.Serializable]
	/// <summary>
	/// A* Pathfinding System�� �׺���̼� �׷����� �����մϴ�.
	/// \ingroup relevant
	///
	/// �� Ŭ������ �ν��Ͻ��� AstarPath.data�� �Ҵ�Ǹ�, ���⼭ <see cref="graphs"/> ������ ���� �ε�� ��� �׷����� �׼����� �� �ֽ��ϴ�.
	/// ���� �� Ŭ������ ���� ������ ����ȭ�� ó���մϴ�.
	/// </summary>
	public class AstarData {
		/// <summary>AstarPath.active�� �ٷ� ����</summary>
		public static AstarPath active {
			get {
				return AstarPath.active;
			}
		}

		#region Fields
		/// <summary>
		/// ù ��° NavMeshGraph�� �ٷ� �����Դϴ�.
		/// ��ĵ �� ������Ʈ�˴ϴ�.
		/// </summary>
		public NavMeshGraph navmesh { get; private set; }

#if !ASTAR_NO_GRID_GRAPH
		/// <summary>
		/// ù ��° GridGraph�� �ٷ� �����Դϴ�.
		/// ��ĵ �� ������Ʈ�˴ϴ�.
		/// </summary>
		public GridGraph gridGraph { get; private set; }
#endif

#if !ASTAR_NO_POINT_GRAPH
		/// <summary>
		/// ù ��° PointGraph�� �ٷ� �����Դϴ�.
		/// ��ĵ �� ������Ʈ�˴ϴ�.
		/// </summary>
		public PointGraph pointGraph { get; private set; }
#endif


		/// <summary>
		/// ��� �����ϴ� �׷��� �����Դϴ�.
		/// �ݻ� �˻��� ���� ä�����ϴ�.
		/// </summary>
		public System.Type[] graphTypes { get; private set; }

#if ASTAR_FAST_NO_EXCEPTIONS || UNITY_WINRT || UNITY_WEBGL
		/// <summary>
    /// iPhone���� Fast But No Exceptions�� ������ �� ����� �׷��� �����Դϴ�.
    /// ����� ���� �׷��� ������ �߰��ϴ� ��� �� �ϵ� �ڵ��� ��Ͽ� �߰��ؾ� �մϴ�.
    /// </summary>
		public static readonly System.Type[] DefaultGraphTypes = new System.Type[] {
#if !ASTAR_NO_GRID_GRAPH
			typeof(GridGraph),
#endif
#if !ASTAR_NO_POINT_GRAPH
			typeof(PointGraph),
#endif
			typeof(NavMeshGraph),
		};
#endif

		/// <summary>
		/// �� �ν��Ͻ��� ������ ��� �׷����Դϴ�.
		/// ����ȭ�� �Ϸ�� �Ŀ��� ä�����ϴ�.
		/// �׷����� ���ŵ� ��� null �׸��� ������ �� �ֽ��ϴ�.
		/// </summary>
		[System.NonSerialized]
		public NavGraph[] graphs = new NavGraph[0];

		// ����ȭ ����

		/// <summary>
		/// ��� �׷��� �� ������ ���� ����ȭ�� �������Դϴ�.
		/// Unity�� Undo �ý����� ���δ� ����Ʈ �����͸� �ջ��Ű�� ������ base64 ���ڵ� �� ���ڿ��� ����˴ϴ�.
		///
		/// �� �����ʹ� <see cref="data"/> �Ӽ����� ����Ʈ �迭�� �׼����� �� �ֽ��ϴ�.
		///
		/// \since 3.6.1
		/// </summary>
		[SerializeField]
		string dataString;

		/// <summary>
		/// 3.6.1 ���� ������ �������Դϴ�.
		/// ���׷��̵� ó���� ���˴ϴ�.
		/// \since 3.6.1
		/// </summary>
		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("data")]
		private byte[] upgradeData;

		/// <summary>��� �׷��� �� ������ ���� ����ȭ�� ������</summary>
		private byte[] data {
			get {
				// 3.6.1 ���� �������� ���׷��̵� ó��
				if (upgradeData != null && upgradeData.Length > 0) {
					data = upgradeData;
					upgradeData = null;
				}
				return dataString != null? System.Convert.FromBase64String(dataString) : null;
			}
			set {
				dataString = value != null? System.Convert.ToBase64String(value) : null;
			}
		}

		/// <summary>
		/// ĳ�õ� ���� ���������� ����ȭ�� �������Դϴ�.
		/// �����Ǹ� ������ �� �׷������� ���Ͽ��� ������ȭ�˴ϴ�.
		/// </summary>
		public TextAsset file_cachedStartup;

		/// <summary>
		/// ĳ�õ� ���� ���������� ����ȭ�� �������Դϴ�.
		///
		/// ������ ����: 3.6 ���ķ� ������ ������, AstarData.file_cachedStartup ��� ���˴ϴ�.
		/// </summary>
		public byte[] data_cachedStartup;

		/// <summary>
		/// �׷��� �����͸� ĳ���ؾ� �ϴ��� ����.
		/// ���� ĳ���� �����Ӹ� �ƴ϶� ��ü �׷��� (�����Ӹ� �ƴ϶�)�� ���� (<see cref="file_cachedStartup"/>���� �ε� �� �� �ֵ��� �մϴ�.
		/// �̴� �Ϲ������� ���� ���� �� �׷��� ��ĵ���� �ξ� �����ϴ�. �̰��� �������� "���� �׷ε�" �ǿ��� �����˴ϴ�.
		///
		/// ����: save-load-graphs (�¶��� �������� �۵� ��ũ ����)
		/// </summary>
		[SerializeField]
		public bool cacheStartup;

		// ����ȭ ���� ��

		List<bool> graphStructureLocked = new List<bool>();

		#endregion

		public byte[] GetData () {
			return data;
		}

		public void SetData (byte[] data) {
			this.data = data;
		}

		/// <summary>�޸𸮿��� �׷����� �ε��մϴ�. ĳ�� �� �׷����������� �ش� �׷������ε��մϴ�.</summary>
		public void Awake()
		{
			graphs = new NavGraph[0];

			if (cacheStartup && file_cachedStartup != null) {
				LoadFromCache();
			} else {
				DeserializeGraphs();
			}
		}

		/// <summary>
		/// �׷��� ������ ������� �ʵ��� ��״� �޼����Դϴ�.
		/// �׷��� �߰� �Ǵ� ���Ÿ� �����ϰ� �׷��� ����ȭ �Ǵ� ������ȭ�� �����մϴ�.
		/// ���� ��� �񵿱� ��ĵ�� ���� ���� �� �׷����� �ı����� �ʵ����ϱ� ���� ���˴ϴ�.
		///
		/// �� �޼����� �� ȣ���� �ݵ�� <see cref="UnlockGraphStructure"/> �޼����� ȣ��� ��Ȯ�� ��ġ�ؾ� �մϴ�.
		/// ȣ���� ��ø�� �� �ֽ��ϴ�.
		/// </summary>
		internal void LockGraphStructure (bool allowAddingGraphs = false) {
			graphStructureLocked.Add(allowAddingGraphs);
		}

		/// <summary>
		/// �׷��� ������ �ٽ� ����� �� �ֵ��� ����ϴ� �޼����Դϴ�.
		/// ����: <see cref="LockGraphStructure"/>
		/// </summary>
		internal void UnlockGraphStructure () {
			if (graphStructureLocked.Count == 0) throw new System.InvalidOperationException();
			graphStructureLocked.RemoveAt(graphStructureLocked.Count - 1);
		}

		PathProcessor.GraphUpdateLock AssertSafe (bool onlyAddingGraph = false) {
			if (graphStructureLocked.Count > 0) {
				bool allowAdding = true;
				for (int i = 0; i < graphStructureLocked.Count; i++) allowAdding &= graphStructureLocked[i];
				if (!(onlyAddingGraph && allowAdding))
					throw new System.InvalidOperationException("�׷��� ������ ��� ���� ���� �׷����� �߰�, ���� �Ǵ� ����ȭ�� �� �����ϴ�. �� ��� �׷����� ��ĵ ���̰ų� �׷��� ������Ʈ �� �۾� �׸��� ������ ���Դϴ�.\n" +
						"�׷��� Ư���� ���μ� �׷����� �۾� �׸� ������ �߰� �� �ʿ䰡 ���� �� �ֽ��ϴ�.");
			}

			// ��� ã�� �����带 �Ͻ� �����մϴ�.
			var graphLock = active.PausePathfinding();
			if (!active.IsInsideWorkItem) {
				// ��� �׷��� ������Ʈ �� ��Ÿ �ݹ��� �Ϸ�Ǿ����� Ȯ���մϴ�.
				// �� �ڵ尡 �۾� �׸� ��ü���� ȣ����� ���� ��쿡�� ����˴ϴ�.
				// �׷��� ������ ��� ��Ⱑ �߻��Ͽ� �Ϸ��� �� ���� ��� ��Ⱑ �߻��� �� �ֽ��ϴ�.
				// ���⿡�� �� �ڵ尡 ȣ��� �� �׷����� �߰��ؾ��ϴ� ��찡 ���� �� �ֽ��ϴ�.
				active.FlushWorkItems();

				// �̹� ���Ǿ� Seeker ���� ��ҷ� ��ȯ�Ǳ⸦ ��ٸ��� ��δ� ��⿭���� ��� ó���ؾ� �մϴ�.
				// �̴� �Ϲ������� ���� �����ϴ� �׷����� �����ϴ� ����� �ֱ� �����Դϴ�.
				// �׷����� �ı��� �� �ı��� ��带 �����ϴ� ��� ����� ���� �� �ֽ��ϴ�.
				active.pathReturnQueue.ReturnPaths(false);
			}
			return graphLock;
		}

		/// <summary>
		/// ��� �׷����� ��� ��忡 ���� �ݹ��� ȣ���մϴ�.
		/// ��� �����ϴ� ��带 �ݺ��ϴ� ���� ������ ����Դϴ�.
		///
		/// <code>
		/// AstarPath.active.data.GetNodes(node => {
		///     Debug.Log("��ġ " + (Vector3)node.position + "�� ��带 ã�ҽ��ϴ�.");
		/// });
		/// </code>
		///
		/// ����: <see cref="Pathfinding.NavGraph.GetNodes"/>�� ����Ͽ� Ư�� �׷����� ��带 ��� ��� ��� ����� ã�� �� �ֽ��ϴ�.
		/// ����: graph-updates (�¶��� �������� �۵� ��ũ ����)
		/// </summary>
		public void GetNodes (System.Action<GraphNode> callback) {
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null) graphs[i].GetNodes(callback);
			}
		}

		/// <summary>
		/// �ٸ� ������ ù ��° �׷����� ���� �ٷ� ���⸦ ������Ʈ�մϴ�.
		/// ��������� ������ ���� ���� ���� �ʽ��ϴ�. �����̰� �����ϰ� �����Ϸ��� �մϴ�.
		/// �׷��� �̷��� ������ �ý��� ����� �����ϱ� ������ �����ϱ�� �����߽��ϴ�.
		/// </summary>
		public void UpdateShortcuts () {
			navmesh = (NavMeshGraph)FindGraphOfType(typeof(NavMeshGraph));

#if !ASTAR_NO_GRID_GRAPH
			gridGraph = (GridGraph)FindGraphOfType(typeof(GridGraph));
#endif

#if !ASTAR_NO_POINT_GRAPH
			pointGraph = (PointGraph)FindGraphOfType(typeof(PointGraph));
#endif
		}

		/// <summary>Load from data from <see cref="file_cachedStartup"/></summary>
		public void LoadFromCache () {
			var graphLock = AssertSafe();

			if (file_cachedStartup != null) {
				var bytes = file_cachedStartup.bytes;
				DeserializeGraphs(bytes);

				GraphModifier.TriggerEvent(GraphModifier.EventType.PostCacheLoad);
			} else {
				Debug.LogError("ĳ�ð� ��� �����Ƿ� ĳ�ÿ��� �ε��� �� �����ϴ�.");
			}
			graphLock.Release();
		}

		#region Serialization

		/// <summary>
		/// ��� �׷��� ������ ����Ʈ �迭�� ����ȭ�մϴ�.
		/// ����: DeserializeGraphs(byte[])
		/// </summary>
		public byte[] SerializeGraphs () {
			return SerializeGraphs(SerializeSettings.Settings);
		}

		/// <summary>
		/// ��� �׷��� ���� �� �ʿ��� ��� ��� �����͸� ����Ʈ �迭�� ����ȭ�մϴ�.
		/// ����: DeserializeGraphs(byte[])
		/// ����: Pathfinding.Serialization.SerializeSettings
		/// </summary>
		public byte[] SerializeGraphs (SerializeSettings settings) {
			uint checksum;

			return SerializeGraphs(settings, out checksum);
		}

		/// <summary>
		/// �ֿ� ����ȭ �Լ��Դϴ�.
		/// ��� �׷����� ����Ʈ �迭�� ����ȭ�մϴ�.
		/// �߰� ������ �����ϱ� ���� AstarPathEditor.cs ��ũ��Ʈ�� ����� �Լ��� �ֽ��ϴ�.
		/// </summary>
		public byte[] SerializeGraphs (SerializeSettings settings, out uint checksum) {
			var graphLock = AssertSafe();
			var sr = new AstarSerializer(this, settings, active.gameObject);

			sr.OpenSerialize();
			sr.SerializeGraphs(graphs);
			sr.SerializeExtraInfo();
			byte[] bytes = sr.CloseSerialize();
			checksum = sr.GetChecksum();
#if ASTARDEBUG
			Debug.Log("������ ���� ����� �����ϴ�. " + bytes.Length + " ����Ʈ");
#endif
			graphLock.Release();
			return bytes;
		}

		/// <summary><see cref="data"/>���� �׷����� ������ȭ�մϴ�.</summary>
		public void DeserializeGraphs () {
			if (data != null) {
				DeserializeGraphs(data);
			}
		}

		/// <summary>��� �׷����� �ı��ϰ� �׷����� null�� �����մϴ�.</summary>
		void ClearGraphs () {
			if (graphs == null) return;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null) {
					((IGraphInternals)graphs[i]).OnDestroy();
					graphs[i].active = null;
				}
			}
			graphs = new NavGraph[0];
			UpdateShortcuts();
		}

		public void OnDestroy () {
			ClearGraphs();
		}

		/// <summary>
		/// ������ ����Ʈ �迭���� �׷����� ������ȭ�մϴ�.
		/// ������ȭ�� �����ϸ� ������ �α׿� ��ϵ˴ϴ�.
		/// </summary>
		public void DeserializeGraphs (byte[] bytes) {
			var graphLock = AssertSafe();

			ClearGraphs();
			DeserializeGraphsAdditive(bytes);
			graphLock.Release();
		}

		/// <summary>
		/// ������ ����Ʈ �迭���� �׷����� �߰��� ������ȭ�մϴ�.
		/// ������ȭ�� �����ϸ� ������ �α׿� ��ϵ˴ϴ�.
		/// �� �Լ��� �ε�� �׷����� ���� �׷����� �߰��մϴ�.
		/// </summary>
		public void DeserializeGraphsAdditive (byte[] bytes) {
			var graphLock = AssertSafe();

			try {
				if (bytes != null) {
					var sr = new AstarSerializer(this, active.gameObject);

					if (sr.OpenDeserialize(bytes)) {
						DeserializeGraphsPartAdditive(sr);
						sr.CloseDeserialize();
					}
					else
					{
						Debug.Log("�߸��� ������ ���� (zip�� ���� �� ����).\n�����Ͱ� �ջ�Ǿ��ų� 3.0.x �Ǵ� ���� ������ �ý����� ����Ͽ� ����Ǿ����ϴ�.");
					}
				} else {
					throw new System.ArgumentNullException("bytes");
				}
				active.VerifyIntegrity();
			}
			catch (System.Exception e)
			{
				Debug.LogError("�����͸� ������ȭ�ϴ� ���� ���ܰ� �߻��߽��ϴ�.\n" + e);
				graphs = new NavGraph[0];
			}

			UpdateShortcuts();
			graphLock.Release();
		}

		/// <summary>�׷��� ������ȭ�� �����ϴ� ����� �Լ�</summary>
		void DeserializeGraphsPartAdditive (AstarSerializer sr) {
			if (graphs == null) graphs = new NavGraph[0];

			var gr = new List<NavGraph>(graphs);

			// �׷��� �ε����� �ùٸ��� �ε�ǵ��� ������ ����
			sr.SetGraphIndexOffset(gr.Count);

			if (graphTypes == null) FindGraphTypes();
			gr.AddRange(sr.DeserializeGraphs(graphTypes));
			graphs = gr.ToArray();

			sr.DeserializeEditorSettingsCompatibility();
			sr.DeserializeExtraInfo();

			//Assign correct graph indices.
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] == null) continue;
				graphs[i].GetNodes(node => node.GraphIndex = (uint)i);
			}

			for (int i = 0; i < graphs.Length; i++) {
				for (int j = i+1; j < graphs.Length; j++) {
					if (graphs[i] != null && graphs[j] != null && graphs[i].guid == graphs[j].guid) {
						Debug.LogWarning("�׷��� �߰��� ������ �� Guid �浹�� �߻��߽��ϴ�. ������ �׷����� ���ο� Guid�� �޽��ϴ�.\n�� �޽����� (����) �����մϴ�.");
						graphs[i].guid = Pathfinding.Util.Guid.NewGuid();
						break;
					}
				}
			}

			sr.PostDeserialization();
			active.hierarchicalGraph.RecalculateIfNecessary();
		}

		#endregion

		/// <summary>
		/// ����� ȯ�濡�� �����Ǵ� ��� �׷��� ������ ã���ϴ�.
		/// ���÷����� ����Ͽ� ��������� NavGraph���� �Ļ��� ������ ã���ϴ�.
		/// </summary>
		public void FindGraphTypes () {
#if !ASTAR_FAST_NO_EXCEPTIONS && !UNITY_WINRT && !UNITY_WEBGL
			var graphList = new List<System.Type>();
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				System.Type[] types = null;
				try {
					types = assembly.GetTypes();
				} catch {
					// Ÿ�� �ε� ���� ���� �����մϴ�.
					// � �����ε� ��� ������� ���� �� ���� �� ������, �߿��� ������ ���� �� �ִ� ������� �����ϴ� ������ ����մϴ�.
					continue;
				}

				foreach (var type in types) {
#if NETFX_CORE && !UNITY_EDITOR
					System.Type baseType = type.GetTypeInfo().BaseType;
#else
					var baseType = type.BaseType;
#endif
					while (baseType != null) {
						if (System.Type.Equals(baseType, typeof(NavGraph))) {
							graphList.Add(type);

							break;
						}

#if NETFX_CORE && !UNITY_EDITOR
						baseType = baseType.GetTypeInfo().BaseType;
#else
						baseType = baseType.BaseType;
#endif
					}
				}
			}

			graphTypes = graphList.ToArray();

#if ASTARDEBUG
			Debug.Log("ã�� �׷��� ���� ��: " + graphTypes.Length);
#endif
#else
			graphTypes = DefaultGraphTypes;
#endif
		}

		#region GraphCreation
		/// <summary>
		/// ������ ���� ���ڿ��� ��ġ�ϴ� System.Type�� ��ȯ�մϴ�. ��ġ�ϴ� �׷��� ������ ã�� ���� ��� null�� ��ȯ�˴ϴ�.
		///
		/// Deprecated: ������� �ʴ� ����Դϴ�. ��� System.Type.GetType�� ����ϼ���.
		/// </summary>
		[System.Obsolete("������ �ʿ��� ��� System.Type.GetType�� ����ϼ���.")]
		public System.Type GetGraphType (string type) {
			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					return graphTypes[i];
				}
			}
			return null;
		}

		/// <summary>
		/// ������ ������ �׷��� �ν��Ͻ��� ����ϴ�. ��ġ�ϴ� �׷��� ������ ã�� ���� ��� ������ ��ϵǰ� null�� ��ȯ�˴ϴ�.
		/// ��ȯ: ������ �׷���
		/// ����: <see cref="CreateGraph(System.Type)"/>
		///
		/// Deprecated: ��� CreateGraph(System.Type)�� ����ϼ���.
		/// </summary>
		[System.Obsolete("CreateGraph(System.Type)�� ��� ����ϼ���.")]
		public NavGraph CreateGraph (string type) {
			Debug.Log("���� '" + type + "'�� �׷����� ���� ��");

			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					return CreateGraph(graphTypes[i]);
				}
			}
			Debug.LogError("�׷��� ���� (" + type + ")�� ã�� �� �����ϴ�");
			return null;
		}

		/// <summary>
		/// ���� type�� ���ο� �׷��� �ν��Ͻ��� �����մϴ�.
		/// ����: <see cref="CreateGraph(string)"/>
		/// </summary>
		internal NavGraph CreateGraph (System.Type type) {
			var graph = System.Activator.CreateInstance(type) as NavGraph;

			graph.active = active;
			return graph;
		}

		/// <summary>
		/// ���� type�� �׷����� <see cref="graphs"/> �迭�� �߰��մϴ�.
		///
		/// Deprecated: ��� AddGraph(System.Type)�� ����ϼ���.
		/// </summary>
		[System.Obsolete("AddGraph(System.Type)�� ��� ����ϼ���.")]
		public NavGraph AddGraph (string type) {
			NavGraph graph = null;

			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					graph = CreateGraph(graphTypes[i]);
				}
			}

			if (graph == null) {
				Debug.LogError("���� '" + type + "'�� NavGraph�� ã�� �� �����ϴ�");
				return null;
			}

			AddGraph(graph);

			return graph;
		}

		/// <summary>
		/// ���� type�� �׷����� <see cref="graphs"/> �迭�� �߰��մϴ�.
		/// ����: ��Ÿ�� �׷��� (�¶��� �������� �۵� ��ũ Ȯ��)
		/// </summary>
		public NavGraph AddGraph (System.Type type) {
			NavGraph graph = null;

			for (int i = 0; i < graphTypes.Length; i++) {
				if (System.Type.Equals(graphTypes[i], type)) {
					graph = CreateGraph(graphTypes[i]);
				}
			}

			if (graph == null) {
				Debug.LogError("���� '" + type + "'�� NavGraph�� ã�� �� �����ϴ�. " + graphTypes.Length + " ���� �׷��� ������ �ֽ��ϴ�");
				return null;
			}

			AddGraph(graph);

			return graph;
		}

		/// <summary>������ �׷����� <see cref="graphs"/> �迭�� �߰��մϴ�.</summary>
		void AddGraph (NavGraph graph) {
			// ��� ã�⿡ ������ ��ġ�� �ʵ��� �մϴ�.
			var graphLock = AssertSafe(true);

			// �� ��ġ�� ä����� �õ��մϴ�.
			bool foundEmpty = false;

			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] == null) {
					graphs[i] = graph;
					graph.graphIndex = (uint)i;
					foundEmpty = true;
					break;
				}
			}

			if (!foundEmpty) {
				if (graphs != null && graphs.Length >= GraphNode.MaxGraphIndex) {
					throw new System.Exception("�׷��� ���� ���ѿ� �����߽��ϴ�. " + GraphNode.MaxGraphIndex + "�� �̻��� �׷����� ���� �� �����ϴ�.");
				}

				// ��Ͽ� �� �׸� �߰�
				var graphList = new List<NavGraph>(graphs ?? new NavGraph[0]);
				graphList.Add(graph);
				graphs = graphList.ToArray();
				graph.graphIndex = (uint)(graphs.Length-1);
			}

			UpdateShortcuts();
			graph.active = active;
			graphLock.Release();
		}

		/// <summary>
		/// ������ �׷����� <see cref="graphs"/> �迭���� �����ϰ� �����ϰ� �ı��մϴ�.
		/// ��Ÿ �׷����� �׷��� �ε����� �������� �������� �׷����� �迭���� ������ �����ϴ� ��� �迭���� null�� �����մϴ�.
		/// �� ��ġ�� �� �׷����� �߰��� �� ����˴ϴ�.
		///
		/// ��ȯ: �׷����� ���������� ���ŵǾ����� ���� (��, <see cref="graphs"/> �迭�� �����ߴ��� ����). �׷��� ������ false�Դϴ�.
		///
		/// ����: 3.2.5���� ����Ǿ� SafeOnDestroy�� ȣ���ϱ� ���� �׷����� �����ϰ� �迭���� ������ �����ϴ� ��� �迭���� null�� �����մϴ�.
		/// </summary>
		public bool RemoveGraph (NavGraph graph) {
			// ��� ã�� �����尡 ������ ������ ��ٸ��ϴ�.
			// ���� �� �׷������� ���� ���� ��� ã�⸦ ��ٸ��� ������ NullReferenceExceptions�� �߻��� �� �ֽ��ϴ�.
			var graphLock = AssertSafe();

			((IGraphInternals)graph).OnDestroy();
			graph.active = null;

			int i = System.Array.IndexOf(graphs, graph);
			if (i != -1) graphs[i] = null;

			UpdateShortcuts();
			graphLock.Release();
			return i != -1;
		}

		#endregion

		#region GraphUtility

		/// <summary>
		/// ������ ��带 �����ϴ� �׷����� ��ȯ�մϴ�. �׷����� <see cref="graphs"/> �迭�� �־�� �մϴ�.
		///
		/// ��ȯ: ��带 �����ϴ� �׷���. �׷����� ã�� ���� ��� null�� ��ȯ�մϴ�.
		/// </summary>
		public static NavGraph GetGraph (GraphNode node) {
			if (node == null) return null;

			AstarPath script = AstarPath.active;
			if (script == null) return null;

			AstarData data = script.data;
			if (data == null || data.graphs == null) return null;

			uint graphIndex = node.GraphIndex;

			if (graphIndex >= data.graphs.Length) {
				return null;
			}

			return data.graphs[(int)graphIndex];
		}

		/// <summary>�־��� ������ �����ϴ� ù ��° �׷����� ��ȯ�մϴ�. �׷����� ã�� ���� ��� null�� ��ȯ�մϴ�.</summary>
		public NavGraph FindGraph (System.Func<NavGraph, bool> predicate) {
			if (graphs != null) {
				for (int i = 0; i < graphs.Length; i++) {
					if (graphs[i] != null && predicate(graphs[i])) {
						return graphs[i];
					}
				}
			}
			return null;
		}

		/// <summary>������ ������ ù ��° �׷����� <see cref="graphs"/> �迭���� ã�� ��ȯ�մϴ�. �׷����� ã�� ���� ��� null�� ��ȯ�մϴ�.</summary>
		public NavGraph FindGraphOfType (System.Type type) {
			return FindGraph(graph => System.Type.Equals(graph.GetType(), type));
		}

		/// <summary>������ ���Ŀ��� ��ӵǴ� ù ��° �׷����� ��ȯ�մϴ�. �׷����� ã�� ���� ��� null�� ��ȯ�մϴ�.</summary>
		public NavGraph FindGraphWhichInheritsFrom (System.Type type) {
			return FindGraph(graph => WindowsStoreCompatibility.GetTypeInfo(type).IsAssignableFrom(WindowsStoreCompatibility.GetTypeInfo(graph.GetType())));
		}

		/// <summary>
		/// �� �Լ��� �ݺ��Ͽ� 'type'�� ��� �׷����� �����ɴϴ�.
		/// <code>
		/// foreach (GridGraph graph in AstarPath.data.FindGraphsOfType (typeof(GridGraph))) {
		///     // �׷����� �۾� ����
		/// }
		/// </code>
		/// ����: AstarPath.RegisterSafeNodeUpdate
		/// </summary>
		public IEnumerable FindGraphsOfType (System.Type type) {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null && System.Type.Equals(graphs[i].GetType(), type)) {
					yield return graphs[i];
				}
			}
		}

		/// <summary>
		/// UpdateableGraph �������̽��� ������ ��� �׷���
		/// <code> foreach (IUpdatableGraph graph in AstarPath.data.GetUpdateableGraphs ()) {
		///  // �׷����� �۾� ����
		/// } </code>
		/// ����: AstarPath.AddWorkItem
		/// ����: Pathfinding.IUpdatableGraph
		/// </summary>
		public IEnumerable GetUpdateableGraphs () {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is IUpdatableGraph) {
					yield return graphs[i];
				}
			}
		}

		/// <summary>
		/// UpdateableGraph �������̽��� ������ ��� �׷���
		/// <code> foreach (IRaycastableGraph graph in AstarPath.data.GetRaycastableGraphs ()) {
		///  // �׷����� �۾� ����
		/// } </code>
		/// ����: Pathfinding.IRaycastableGraph
		/// Deprecated: ���������� ������ ������ ��� ��ʰ� �����ϴ�. ��� <see cref="graphs"/> �迭�� �ݺ��ϼ���.
		/// </summary>
		[System.Obsolete("���������� ������ ������ ��� ��ʰ� �����Ƿ� ������ �ʽ��ϴ�. ��� graphs �迭�� �ݺ��ϼ���.")]
		public IEnumerable GetRaycastableGraphs () {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is IRaycastableGraph) {
					yield return graphs[i];
				}
			}
		}

		/// <summary><see cref="graphs"/> �迭���� NavGraph�� �ε����� �����ɴϴ�.</summary>
		public int GetGraphIndex (NavGraph graph) {
			if (graph == null) throw new System.ArgumentNullException("graph");

			var index = -1;
			if (graphs != null) {
				index = System.Array.IndexOf(graphs, graph);
				if (index == -1) Debug.LogError("�׷����� �������� �ʽ��ϴ�");
			}
			return index;
		}

		#endregion
	}
}
