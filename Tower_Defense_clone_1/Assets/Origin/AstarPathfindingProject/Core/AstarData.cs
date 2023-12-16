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
	/// A* Pathfinding System의 네비게이션 그래프를 저장합니다.
	/// \ingroup relevant
	///
	/// 이 클래스의 인스턴스는 AstarPath.data에 할당되며, 여기서 <see cref="graphs"/> 변수를 통해 로드된 모든 그래프에 액세스할 수 있습니다.
	/// 또한 이 클래스는 높은 수준의 직렬화를 처리합니다.
	/// </summary>
	public class AstarData {
		/// <summary>AstarPath.active의 바로 가기</summary>
		public static AstarPath active {
			get {
				return AstarPath.active;
			}
		}

		#region Fields
		/// <summary>
		/// 첫 번째 NavMeshGraph의 바로 가기입니다.
		/// 스캔 시 업데이트됩니다.
		/// </summary>
		public NavMeshGraph navmesh { get; private set; }

#if !ASTAR_NO_GRID_GRAPH
		/// <summary>
		/// 첫 번째 GridGraph의 바로 가기입니다.
		/// 스캔 시 업데이트됩니다.
		/// </summary>
		public GridGraph gridGraph { get; private set; }
#endif

#if !ASTAR_NO_POINT_GRAPH
		/// <summary>
		/// 첫 번째 PointGraph의 바로 가기입니다.
		/// 스캔 시 업데이트됩니다.
		/// </summary>
		public PointGraph pointGraph { get; private set; }
#endif


		/// <summary>
		/// 모든 지원하는 그래프 유형입니다.
		/// 반사 검색을 통해 채워집니다.
		/// </summary>
		public System.Type[] graphTypes { get; private set; }

#if ASTAR_FAST_NO_EXCEPTIONS || UNITY_WINRT || UNITY_WEBGL
		/// <summary>
    /// iPhone에서 Fast But No Exceptions로 빌드할 때 사용할 그래프 유형입니다.
    /// 사용자 정의 그래프 유형을 추가하는 경우 이 하드 코딩된 목록에 추가해야 합니다.
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
		/// 이 인스턴스가 보유한 모든 그래프입니다.
		/// 직렬화가 완료된 후에만 채워집니다.
		/// 그래프가 제거된 경우 null 항목을 포함할 수 있습니다.
		/// </summary>
		[System.NonSerialized]
		public NavGraph[] graphs = new NavGraph[0];

		// 직렬화 설정

		/// <summary>
		/// 모든 그래프 및 설정에 대한 직렬화된 데이터입니다.
		/// Unity의 Undo 시스템이 때로는 바이트 데이터를 손상시키기 때문에 base64 인코딩 된 문자열로 저장됩니다.
		///
		/// 이 데이터는 <see cref="data"/> 속성에서 바이트 배열로 액세스할 수 있습니다.
		///
		/// \since 3.6.1
		/// </summary>
		[SerializeField]
		string dataString;

		/// <summary>
		/// 3.6.1 이전 버전의 데이터입니다.
		/// 업그레이드 처리에 사용됩니다.
		/// \since 3.6.1
		/// </summary>
		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("data")]
		private byte[] upgradeData;

		/// <summary>모든 그래프 및 설정에 대한 직렬화된 데이터</summary>
		private byte[] data {
			get {
				// 3.6.1 이전 버전에서 업그레이드 처리
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
		/// 캐시된 시작 설정을위한 직렬화된 데이터입니다.
		/// 설정되면 시작할 때 그래프가이 파일에서 역직렬화됩니다.
		/// </summary>
		public TextAsset file_cachedStartup;

		/// <summary>
		/// 캐시된 시작 설정을위한 직렬화된 데이터입니다.
		///
		/// 사용되지 않음: 3.6 이후로 사용되지 않으며, AstarData.file_cachedStartup 대신 사용됩니다.
		/// </summary>
		public byte[] data_cachedStartup;

		/// <summary>
		/// 그래프 데이터를 캐시해야 하는지 여부.
		/// 시작 캐싱은 설정뿐만 아니라 전체 그래프 (설정뿐만 아니라)를 파일 (<see cref="file_cachedStartup"/>에서 로드 할 수 있도록 합니다.
		/// 이는 일반적으로 게임 시작 시 그래프 스캔보다 훨씬 빠릅니다. 이것은 에디터의 "저장 및로드" 탭에서 구성됩니다.
		///
		/// 참조: save-load-graphs (온라인 문서에서 작동 링크 보기)
		/// </summary>
		[SerializeField]
		public bool cacheStartup;

		// 직렬화 설정 끝

		List<bool> graphStructureLocked = new List<bool>();

		#endregion

		public byte[] GetData () {
			return data;
		}

		public void SetData (byte[] data) {
			this.data = data;
		}

		/// <summary>메모리에서 그래프를 로드합니다. 캐시 된 그래프가있으면 해당 그래프를로드합니다.</summary>
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
		/// 그래프 구조가 변경되지 않도록 잠그는 메서드입니다.
		/// 그래프 추가 또는 제거를 방지하고 그래프 직렬화 또는 역직렬화를 방지합니다.
		/// 예를 들어 비동기 스캔이 진행 중일 때 그래프가 파괴되지 않도록하기 위해 사용됩니다.
		///
		/// 이 메서드의 각 호출은 반드시 <see cref="UnlockGraphStructure"/> 메서드의 호출과 정확히 일치해야 합니다.
		/// 호출은 중첩될 수 있습니다.
		/// </summary>
		internal void LockGraphStructure (bool allowAddingGraphs = false) {
			graphStructureLocked.Add(allowAddingGraphs);
		}

		/// <summary>
		/// 그래프 구조가 다시 변경될 수 있도록 허용하는 메서드입니다.
		/// 참조: <see cref="LockGraphStructure"/>
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
					throw new System.InvalidOperationException("그래프 구조가 잠겨 있을 때는 그래프를 추가, 제거 또는 직렬화할 수 없습니다. 이 경우 그래프가 스캔 중이거나 그래프 업데이트 및 작업 항목을 실행할 때입니다.\n" +
						"그러나 특별한 경우로서 그래프를 작업 항목 내에서 추가 할 필요가 있을 수 있습니다.");
			}

			// 경로 찾기 스레드를 일시 중지합니다.
			var graphLock = active.PausePathfinding();
			if (!active.IsInsideWorkItem) {
				// 모든 그래프 업데이트 및 기타 콜백이 완료되었는지 확인합니다.
				// 이 코드가 작업 항목 자체에서 호출되지 않은 경우에만 수행됩니다.
				// 그렇지 않으면 재귀 대기가 발생하여 완료할 수 없는 재귀 대기가 발생할 수 있습니다.
				// 여기에는 이 코드가 호출될 때 그래프를 추가해야하는 경우가 있을 수 있습니다.
				active.FlushWorkItems();

				// 이미 계산되어 Seeker 구성 요소로 반환되기를 기다리는 경로는 대기열에서 즉시 처리해야 합니다.
				// 이는 일반적으로 현재 존재하는 그래프에 의존하는 결과가 있기 때문입니다.
				// 그래프를 파괴한 후 파괴된 노드를 포함하는 경로 결과를 받을 수 있습니다.
				active.pathReturnQueue.ReturnPaths(false);
			}
			return graphLock;
		}

		/// <summary>
		/// 모든 그래프의 모든 노드에 대해 콜백을 호출합니다.
		/// 모든 존재하는 노드를 반복하는 가장 간단한 방법입니다.
		///
		/// <code>
		/// AstarPath.active.data.GetNodes(node => {
		///     Debug.Log("위치 " + (Vector3)node.position + "에 노드를 찾았습니다.");
		/// });
		/// </code>
		///
		/// 참조: <see cref="Pathfinding.NavGraph.GetNodes"/>를 사용하여 특정 그래프의 노드를 모든 대신 얻는 방법을 찾을 수 있습니다.
		/// 참조: graph-updates (온라인 문서에서 작동 링크 보기)
		/// </summary>
		public void GetNodes (System.Action<GraphNode> callback) {
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] != null) graphs[i].GetNodes(callback);
			}
		}

		/// <summary>
		/// 다른 유형의 첫 번째 그래프에 대한 바로 가기를 업데이트합니다.
		/// 명시적으로 참조를 갖는 것은 좋지 않습니다. 동적이고 유연하게 유지하려고 합니다.
		/// 그러나 이러한 참조는 시스템 사용을 쉽게하기 때문에 유지하기로 결정했습니다.
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
				Debug.LogError("캐시가 비어 있으므로 캐시에서 로드할 수 없습니다.");
			}
			graphLock.Release();
		}

		#region Serialization

		/// <summary>
		/// 모든 그래프 설정을 바이트 배열로 직렬화합니다.
		/// 참조: DeserializeGraphs(byte[])
		/// </summary>
		public byte[] SerializeGraphs () {
			return SerializeGraphs(SerializeSettings.Settings);
		}

		/// <summary>
		/// 모든 그래프 설정 및 필요한 경우 노드 데이터를 바이트 배열로 직렬화합니다.
		/// 참조: DeserializeGraphs(byte[])
		/// 참조: Pathfinding.Serialization.SerializeSettings
		/// </summary>
		public byte[] SerializeGraphs (SerializeSettings settings) {
			uint checksum;

			return SerializeGraphs(settings, out checksum);
		}

		/// <summary>
		/// 주요 직렬화 함수입니다.
		/// 모든 그래프를 바이트 배열로 직렬화합니다.
		/// 추가 정보를 저장하기 위해 AstarPathEditor.cs 스크립트에 비슷한 함수가 있습니다.
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
			Debug.Log("데이터 양이 상당히 많습니다. " + bytes.Length + " 바이트");
#endif
			graphLock.Release();
			return bytes;
		}

		/// <summary><see cref="data"/>에서 그래프를 역직렬화합니다.</summary>
		public void DeserializeGraphs () {
			if (data != null) {
				DeserializeGraphs(data);
			}
		}

		/// <summary>모든 그래프를 파괴하고 그래프를 null로 설정합니다.</summary>
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
		/// 지정된 바이트 배열에서 그래프를 역직렬화합니다.
		/// 역직렬화에 실패하면 오류가 로그에 기록됩니다.
		/// </summary>
		public void DeserializeGraphs (byte[] bytes) {
			var graphLock = AssertSafe();

			ClearGraphs();
			DeserializeGraphsAdditive(bytes);
			graphLock.Release();
		}

		/// <summary>
		/// 지정된 바이트 배열에서 그래프를 추가로 역직렬화합니다.
		/// 역직렬화에 실패하면 오류가 로그에 기록됩니다.
		/// 이 함수는 로드된 그래프를 현재 그래프에 추가합니다.
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
						Debug.Log("잘못된 데이터 파일 (zip을 읽을 수 없음).\n데이터가 손상되었거나 3.0.x 또는 이전 버전의 시스템을 사용하여 저장되었습니다.");
					}
				} else {
					throw new System.ArgumentNullException("bytes");
				}
				active.VerifyIntegrity();
			}
			catch (System.Exception e)
			{
				Debug.LogError("데이터를 역직렬화하는 동안 예외가 발생했습니다.\n" + e);
				graphs = new NavGraph[0];
			}

			UpdateShortcuts();
			graphLock.Release();
		}

		/// <summary>그래프 역직렬화를 지원하는 도우미 함수</summary>
		void DeserializeGraphsPartAdditive (AstarSerializer sr) {
			if (graphs == null) graphs = new NavGraph[0];

			var gr = new List<NavGraph>(graphs);

			// 그래프 인덱스가 올바르게 로드되도록 오프셋 설정
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
						Debug.LogWarning("그래프 추가로 가져올 때 Guid 충돌이 발생했습니다. 가져온 그래프는 새로운 Guid를 받습니다.\n이 메시지는 (비교적) 무해합니다.");
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
		/// 빌드된 환경에서 지원되는 모든 그래프 유형을 찾습니다.
		/// 리플렉션을 사용하여 어셈블리에서 NavGraph에서 파생된 유형을 찾습니다.
		/// </summary>
		public void FindGraphTypes () {
#if !ASTAR_FAST_NO_EXCEPTIONS && !UNITY_WINRT && !UNITY_WEBGL
			var graphList = new List<System.Type>();
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				System.Type[] types = null;
				try {
					types = assembly.GetTypes();
				} catch {
					// 타입 로드 예외 등을 무시합니다.
					// 어떤 이유로든 모든 어셈블리를 읽을 수 없을 수 있지만, 중요한 유형은 읽을 수 있는 어셈블리에 존재하는 것으로 기대합니다.
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
			Debug.Log("찾은 그래프 유형 수: " + graphTypes.Length);
#endif
#else
			graphTypes = DefaultGraphTypes;
#endif
		}

		#region GraphCreation
		/// <summary>
		/// 지정된 형식 문자열과 일치하는 System.Type을 반환합니다. 일치하는 그래프 유형을 찾지 못한 경우 null이 반환됩니다.
		///
		/// Deprecated: 사용하지 않는 기능입니다. 대신 System.Type.GetType을 사용하세요.
		/// </summary>
		[System.Obsolete("실제로 필요한 경우 System.Type.GetType을 사용하세요.")]
		public System.Type GetGraphType (string type) {
			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					return graphTypes[i];
				}
			}
			return null;
		}

		/// <summary>
		/// 지정된 형식의 그래프 인스턴스를 만듭니다. 일치하는 그래프 유형을 찾지 못한 경우 오류가 기록되고 null이 반환됩니다.
		/// 반환: 생성된 그래프
		/// 참조: <see cref="CreateGraph(System.Type)"/>
		///
		/// Deprecated: 대신 CreateGraph(System.Type)을 사용하세요.
		/// </summary>
		[System.Obsolete("CreateGraph(System.Type)을 대신 사용하세요.")]
		public NavGraph CreateGraph (string type) {
			Debug.Log("형식 '" + type + "'의 그래프를 생성 중");

			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					return CreateGraph(graphTypes[i]);
				}
			}
			Debug.LogError("그래프 형식 (" + type + ")을 찾을 수 없습니다");
			return null;
		}

		/// <summary>
		/// 형식 type의 새로운 그래프 인스턴스를 생성합니다.
		/// 참조: <see cref="CreateGraph(string)"/>
		/// </summary>
		internal NavGraph CreateGraph (System.Type type) {
			var graph = System.Activator.CreateInstance(type) as NavGraph;

			graph.active = active;
			return graph;
		}

		/// <summary>
		/// 형식 type의 그래프를 <see cref="graphs"/> 배열에 추가합니다.
		///
		/// Deprecated: 대신 AddGraph(System.Type)을 사용하세요.
		/// </summary>
		[System.Obsolete("AddGraph(System.Type)을 대신 사용하세요.")]
		public NavGraph AddGraph (string type) {
			NavGraph graph = null;

			for (int i = 0; i < graphTypes.Length; i++) {
				if (graphTypes[i].Name == type) {
					graph = CreateGraph(graphTypes[i]);
				}
			}

			if (graph == null) {
				Debug.LogError("형식 '" + type + "'의 NavGraph를 찾을 수 없습니다");
				return null;
			}

			AddGraph(graph);

			return graph;
		}

		/// <summary>
		/// 형식 type의 그래프를 <see cref="graphs"/> 배열에 추가합니다.
		/// 참조: 런타임 그래프 (온라인 문서에서 작동 링크 확인)
		/// </summary>
		public NavGraph AddGraph (System.Type type) {
			NavGraph graph = null;

			for (int i = 0; i < graphTypes.Length; i++) {
				if (System.Type.Equals(graphTypes[i], type)) {
					graph = CreateGraph(graphTypes[i]);
				}
			}

			if (graph == null) {
				Debug.LogError("형식 '" + type + "'의 NavGraph를 찾을 수 없습니다. " + graphTypes.Length + " 개의 그래프 유형이 있습니다");
				return null;
			}

			AddGraph(graph);

			return graph;
		}

		/// <summary>지정된 그래프를 <see cref="graphs"/> 배열에 추가합니다.</summary>
		void AddGraph (NavGraph graph) {
			// 경로 찾기에 영향을 미치지 않도록 합니다.
			var graphLock = AssertSafe(true);

			// 빈 위치를 채우려고 시도합니다.
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
					throw new System.Exception("그래프 개수 제한에 도달했습니다. " + GraphNode.MaxGraphIndex + "개 이상의 그래프를 가질 수 없습니다.");
				}

				// 목록에 새 항목 추가
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
		/// 지정된 그래프를 <see cref="graphs"/> 배열에서 제거하고 안전하게 파괴합니다.
		/// 기타 그래프의 그래프 인덱스를 변경하지 않으려면 그래프를 배열에서 실제로 제거하는 대신 배열에서 null로 설정합니다.
		/// 빈 위치는 새 그래프를 추가할 때 재사용됩니다.
		///
		/// 반환: 그래프가 성공적으로 제거되었는지 여부 (즉, <see cref="graphs"/> 배열에 존재했는지 여부). 그렇지 않으면 false입니다.
		///
		/// 버전: 3.2.5에서 변경되어 SafeOnDestroy를 호출하기 전에 그래프를 제거하고 배열에서 실제로 제거하는 대신 배열에서 null로 설정합니다.
		/// </summary>
		public bool RemoveGraph (NavGraph graph) {
			// 경로 찾기 스레드가 정지될 때까지 기다립니다.
			// 현재 이 그래프에서 실행 중인 경로 찾기를 기다리지 않으면 NullReferenceExceptions가 발생할 수 있습니다.
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
		/// 지정된 노드를 포함하는 그래프를 반환합니다. 그래프는 <see cref="graphs"/> 배열에 있어야 합니다.
		///
		/// 반환: 노드를 포함하는 그래프. 그래프를 찾지 못한 경우 null을 반환합니다.
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

		/// <summary>주어진 조건을 만족하는 첫 번째 그래프를 반환합니다. 그래프를 찾지 못한 경우 null을 반환합니다.</summary>
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

		/// <summary>지정된 형식의 첫 번째 그래프를 <see cref="graphs"/> 배열에서 찾아 반환합니다. 그래프를 찾지 못한 경우 null을 반환합니다.</summary>
		public NavGraph FindGraphOfType (System.Type type) {
			return FindGraph(graph => System.Type.Equals(graph.GetType(), type));
		}

		/// <summary>지정된 형식에서 상속되는 첫 번째 그래프를 반환합니다. 그래프를 찾지 못한 경우 null을 반환합니다.</summary>
		public NavGraph FindGraphWhichInheritsFrom (System.Type type) {
			return FindGraph(graph => WindowsStoreCompatibility.GetTypeInfo(type).IsAssignableFrom(WindowsStoreCompatibility.GetTypeInfo(graph.GetType())));
		}

		/// <summary>
		/// 이 함수를 반복하여 'type'의 모든 그래프를 가져옵니다.
		/// <code>
		/// foreach (GridGraph graph in AstarPath.data.FindGraphsOfType (typeof(GridGraph))) {
		///     // 그래프로 작업 수행
		/// }
		/// </code>
		/// 참조: AstarPath.RegisterSafeNodeUpdate
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
		/// UpdateableGraph 인터페이스를 구현한 모든 그래프
		/// <code> foreach (IUpdatableGraph graph in AstarPath.data.GetUpdateableGraphs ()) {
		///  // 그래프로 작업 수행
		/// } </code>
		/// 참조: AstarPath.AddWorkItem
		/// 참조: Pathfinding.IUpdatableGraph
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
		/// UpdateableGraph 인터페이스를 구현한 모든 그래프
		/// <code> foreach (IRaycastableGraph graph in AstarPath.data.GetRaycastableGraphs ()) {
		///  // 그래프로 작업 수행
		/// } </code>
		/// 참조: Pathfinding.IRaycastableGraph
		/// Deprecated: 내부적으로 사용되지 않으며 사용 사례가 적습니다. 대신 <see cref="graphs"/> 배열을 반복하세요.
		/// </summary>
		[System.Obsolete("내부적으로 사용되지 않으며 사용 사례가 적으므로 사용되지 않습니다. 대신 graphs 배열을 반복하세요.")]
		public IEnumerable GetRaycastableGraphs () {
			if (graphs == null) yield break;
			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is IRaycastableGraph) {
					yield return graphs[i];
				}
			}
		}

		/// <summary><see cref="graphs"/> 배열에서 NavGraph의 인덱스를 가져옵니다.</summary>
		public int GetGraphIndex (NavGraph graph) {
			if (graph == null) throw new System.ArgumentNullException("graph");

			var index = -1;
			if (graphs != null) {
				index = System.Array.IndexOf(graphs, graph);
				if (index == -1) Debug.LogError("그래프가 존재하지 않습니다");
			}
			return index;
		}

		#endregion
	}
}
