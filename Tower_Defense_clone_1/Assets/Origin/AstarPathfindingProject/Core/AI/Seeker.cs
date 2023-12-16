using UnityEngine;
using System.Collections.Generic;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	/// <summary>
	/// ���� ������ ��� ȣ���� ó���մϴ�.
	/// \ingroup relevant
	/// �� ������Ʈ�� ���� ���� (AI, �κ�, �÷��̾� ��)�� �����Ǿ� ��� Ž�� ȣ���� ó���ϰ� ��� �����⸦ ����Ͽ� ����� �� ó���� ����մϴ�.
	///
	/// [�̹����� ������ �¶��� ������ �������]
	///
	/// ����: calling-pathfinding (�¶��� �������� �۵��ϴ� ��ũ�� Ȯ��)
	/// ����: modifiers (�¶��� �������� �۵��ϴ� ��ũ�� Ȯ��)
	/// </summary>
	[AddComponentMenu("Pathfinding/Seeker")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_seeker.php")]
	public class Seeker : VersionedMonoBehaviour {
		/// <summary>
		/// ������ ���� ��θ� Gizmos�� ����Ͽ� �׸��ϴ�.
		/// ��δ� ������� ǥ�õ˴ϴ�.
		///
		/// ����: OnDrawGizmos
		/// </summary>
		public bool drawGizmos = true;

		/// <summary>
		/// �� ó������ ���� ��θ� Gizmos�� ����Ͽ� �׸��� ���θ� Ȱ��ȭ�մϴ�.
		/// ��δ� ��Ȳ������ ǥ�õ˴ϴ�.
		///
		/// <see cref="drawGizmos"/>�� true ���� �մϴ�.
		///
		/// �̰��� ��ο� �ε巯�� ó���� ���� �� ó���� ����Ǳ� ���� ��θ� ǥ���մϴ�.
		///
		/// ����: drawGizmos
		/// ����: OnDrawGizmos
		/// </summary>
		public bool detailedGizmos;

		/// <summary>
		/// ����� �������� ������ �����ϴ� ��� �������Դϴ�.
		/// </summary>
		[HideInInspector]
		public StartEndModifier startEndModifier = new StartEndModifier();

		/// <summary>
		/// Seeker�� Ž���� �� �ִ� �±��Դϴ�.
		///
		/// ����: �� �ʵ�� ��Ʈ����ũ(bitmask)�Դϴ�.
		/// ��Ʈ����ũ�� ���� �ڼ��� ������ �¶��� ������ �����Ͻʽÿ�.
		/// </summary>
		[HideInInspector]
		public int traversableTags = -1;

		/// <summary>
		/// �� �±׿� ���� �г�Ƽ ���Դϴ�.
		/// �⺻ �±��� �±� 0�� tagPenalties[0]�� �г�Ƽ�� �߰��˴ϴ�.
		/// �� ������ A* �˰����� ���� �г�Ƽ�� ó���� �� �����Ƿ� ��� ���� ����ؾ� �մϴ�.
		///
		/// ����: �� �迭�� ���̴� �׻� 32���� �ϸ�, �׷��� ������ �ý��ۿ��� ���õ˴ϴ�.
		///
		/// ����: Pathfinding.Path.tagPenalties
		/// </summary>
		[HideInInspector]
		public int[] tagPenalties = new int[32];

		/// <summary>
		/// �� Seeker�� ����� �� �ִ� �׷����Դϴ�.
		/// �� �ʵ�� ����� �������� ������ �˻��� �� � �׷����� ������� �����մϴ�.
		/// �̰��� �پ��� ��Ȳ���� �����ϸ�, ���� ������ �׷����� ū ������ �׷����� ����� ���� �� ����� �� �ֽ��ϴ�.
		///
		/// �̰��� ��Ʈ����ũ�̹Ƿ�, ���� ��� ������Ʈ�� �׷��� �ε��� 3�� ����Ϸ��� ������ ���� ������ �� �ֽ��ϴ�:
		/// <code> seeker.graphMask = 1 << 3; </code>
		///
		/// ����: bitmasks (�¶��� �������� �۵��ϴ� ��ũ�� Ȯ��)
		///
		/// �� �ʵ�� ���� �׷��� �ε����� �����մϴ�. �׷����� ������ ����Ǹ� �� ����ũ�� �� �̻� �ùٸ��� ���� �� �ֽ��ϴ�.
		///
		/// �׷����� �̸��� �˰� �ִٸ� <see cref="Pathfinding.GraphMask.FromGraphName"/> �޼��带 ����� �� �ֽ��ϴ�:
		/// <code>
		/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
		/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
		///
		/// NNConstraint nn = NNConstraint.Default;
		///
		/// nn.graphMask = mask1 | mask2;
		///
		/// // 'My Grid Graph' �Ǵ� 'My Other Grid Graph'�� ���ϴ� somePoint ���� ����� ��带 ã���ϴ�.
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		///
		/// �Ϻ� <see cref="StartPath"/> �޼����� �����ε�� graphMask �Ű������� ����մϴ�.
		/// �̷��� �����ε尡 ���Ǹ� �ش� ��� ��û�� �׷��� ����ũ�� �������մϴ�.
		///
		/// [�̹����� ������ �¶��� ������ �������]
		///
		/// ����: multiple-agent-types (�¶��� �������� �۵��ϴ� ��ũ�� Ȯ��)
		/// </summary>
		[HideInInspector]
		public GraphMask graphMask = GraphMask.everything;

		/// <summary>���� ������ ����ȭ ȣȯ���� ���� ���˴ϴ�</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("graphMask")]
		int graphMaskCompatibility = -1;

		/// <summary>
		/// ��ΰ� �Ϸ�� �� ȣ��Ǵ� �ݹ��Դϴ�.
		/// �� ��������Ʈ�� ������ ��ũ��Ʈ�� ����ؾ� �մϴ�.
		/// StartPath�� ȣ���� �� �ӽ� �ݹ鵵 ������ �� ������, �ش� ��ο� ���ؼ��� ȣ��˴ϴ�.
		/// </summary>
		public OnPathDelegate pathCallback;

		/// <summary>��� Ž���� ���۵Ǳ� ���� ȣ��˴ϴ�</summary>
		public OnPathDelegate preProcessPath;

		/// <summary>��ΰ� ���� ��, �����Ⱑ ����Ǳ� ������ ȣ��˴ϴ�.</summary>
		public OnPathDelegate postProcessPath;

		/// <summary>Gizmos�� �׸��� ���� ���˴ϴ�</summary>
		[System.NonSerialized]
		List<Vector3> lastCompletedVectorPath;

		/// <summary>Gizmos�� �׸��� ���� ���˴ϴ�</summary>
		[System.NonSerialized]
		List<GraphNode> lastCompletedNodePath;

		/// <summary>���� ����Դϴ�</summary>
		[System.NonSerialized]
		protected Path path;

		/// <summary>���� ���. Gizmos�� �׸��� ���� ���˴ϴ�</summary>
		[System.NonSerialized]
		private Path prevPath;

		/// <summary>��ΰ� ���۵� �� �Ź� �Ҵ��� ���ϱ� ���� ĳ�õ� ��������Ʈ</summary>
		private readonly OnPathDelegate onPathDelegate;

		/// <summary>���� ��ο��� ȣ��Ǵ� �ӽ� �ݹ��Դϴ�. �� ���� StartPath �Լ��� ���� �����˴ϴ�</summary>
		private OnPathDelegate tmpPathCallback;

		/// <summary>�ֱ� ��� ��ȸ�� ��� ID</summary>
		protected uint lastPathID;

		/// <summary>��� �������� ���� ���</summary>
		readonly List<IPathModifier> modifiers = new List<IPathModifier>();

		public enum ModifierPass {
			PreProcess,
			// ������ ���Ǵ� �׸��� �ε��� 1�� �����߽��ϴ�.
			PostProcess = 2,
		}

		public Seeker () {
			onPathDelegate = OnPathComplete;
		}

		/// <summary>�� ���� ������ �ʱ�ȭ�մϴ�.</summary>
		protected override void Awake () {
			base.Awake();
			startEndModifier.Awake(this);
		}

		/// <summary>
		/// ���� ��� ���� ��� �Ǵ� ���������� ����� ����Դϴ�.
		/// �� �޼��带 ����ϴ� ���� �幰��, ��� ��� �ݹ��� ȣ��� �� ��θ� �����;� �մϴ�.
		///
		/// ����: pathCallback
		/// </summary>
		public Path GetCurrentPath()
		{
			return path;
		}

		/// <summary>
		/// ���� ��� ��û�� ��� �����մϴ�.
		/// �� Seeker�� ���� ��θ� ��� ���� ��� ��� ����� ��ҵ˴ϴ�.
		/// �ݹ�(�Ϲ������� OnPathComplete��� �޼���)�� ����� 'error' �ʵ尡 true�� ������ ��ο� �Բ� �� ȣ��˴ϴ�.
		///
		/// �� �۾��� ĳ������ �̵��� �������� �ʰ�, ��� ��길 �ߴ��մϴ�.
		/// </summary>
		/// <param name="pool">���̸� ��ΰ� �ý��ۿ��� ��� �Ϸ�� �� ��ΰ� Ǯ�� ���ϴ�.</param>
		public void CancelCurrentPathRequest(bool pool = true)
		{
			if (!IsDone()) {
				path.FailWithError("��ũ��Ʈ�� ���� ��ҵ� (Seeker.CancelCurrentPathRequest)");
				if (pool) {
					// ��ΰ� ���� ī��Ʈ�� �����ϰ� ���ҵǾ����� Ȯ���մϴ�.
					// �̷��� ���� ������ �ý����� Ǯ���� ���� ������ �ʴ� ������ �����ϰ� ��θ� Ǯ�� �������� �ʽ��ϴ�.
					// �� �Ű� ����(�� ��� 'path')�� ����ϴ� Ư�� ��ü�� ���� �߿����� �ʽ��ϴ�.
					// ���� *�* ��ü���� �մϴ�.
					path.Claim(path);
					path.Release(path);
				}
			}
		}

		/// <summary>
		/// �� ���� ������ �����մϴ�.
		/// �ʿ��� ��� ��û�� ��θ� �����մϴ�.
		/// <see cref="startEndModifier"/>���� OnDestroy�� ȣ���մϴ�.
		///
		/// ����: ReleaseClaimedPath
		/// ����: startEndModifier
		/// </summary>
		public void OnDestroy () {
			ReleaseClaimedPath();
			startEndModifier.OnDestroy(this);
		}

		/// <summary>
		/// ��� ������ ��θ� �����մϴ� (�ִ� ���).
		/// Seeker�� �ֽ� ��θ� �����Ͽ� Gizmos�� �׸� �� �ֽ��ϴ�.
		/// ��쿡 ���� �̰��� ������ ���� �� ������, �����Ϸ��� �� �޼��带 ȣ���� �� �ֽ��ϴ� (��� Gizmos�� �׷����� ������ ����).
		///
		/// ������ �ƹ��͵� �������� ���ߴٸ� �Ƹ��� �� �޼��带 ����� �ʿ䰡 ���� ���Դϴ�.
		///
		/// ����: pooling (�۵� ��ũ�� �ִ� �¶��� �������� Ȯ��)
		/// </summary>
		void ReleaseClaimedPath () {
			if (prevPath != null) {
				prevPath.Release(this, true);
				prevPath = null;
			}
		}

		/// <summary>�����Ⱑ �ڽ��� ����� �� ȣ��˴ϴ�.</summary>
		public void RegisterModifier (IPathModifier modifier) {
			modifiers.Add(modifier);

			// ��õ� ������ ������� �����⸦ �����մϴ�.
			modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
		}

		/// <summary>�����Ⱑ ��Ȱ��ȭ�ǰų� �ı��� �� �����Ⱑ ȣ���մϴ�.</summary>
		public void DeregisterModifier (IPathModifier modifier) {
			modifiers.Remove(modifier);
		}

		/// <summary>
		/// ��θ� ��ó���մϴ�.
		/// �̴� �� GameObject�� ����� ��� �����⸦ ��ο� �����մϴ�.
		/// �̰��� RunModifiers(ModifierPass.PostProcess, path)�� ȣ���ϴ� �Ͱ� �����մϴ�.
		/// ����: RunModifiers
		/// \since 3.2�� �߰���
		/// </summary>
		public void PostProcess (Path path) {
			RunModifiers(ModifierPass.PostProcess, path);
		}

		/// <summary>��ο� �����⸦ �����մϴ�.</summary>
		public void RunModifiers (ModifierPass pass, Path path) {
			if (pass == ModifierPass.PreProcess) {
				if (preProcessPath != null) preProcessPath(path);

				for (int i = 0; i < modifiers.Count; i++) modifiers[i].PreProcess(path);
			} else if (pass == ModifierPass.PostProcess) {
				Profiler.BeginSample("Running Path Modifiers");
				// Call delegates if they exist
				if (postProcessPath != null) postProcessPath(path);

				// Loop through all modifiers and apply post processing
				for (int i = 0; i < modifiers.Count; i++) modifiers[i].Apply(path);
				Profiler.EndSample();
			}
		}

		/// <summary>
		/// ���� ��ΰ� ��� �Ϸ�Ǿ����� Ȯ���մϴ�.
		/// ���� ��ΰ� ��ȯ�Ǿ��ų� ��ΰ� null�� ��� true�� ��ȯ�մϴ�.
		///
		/// ����: �̰��� Pathfinding.Path.IsDone�� ȥ������ ���ʽÿ�.
		/// ���� ���� ���� ���� ��ȯ������ �׻� �׷� ���� �ƴϱ� �����Դϴ�.
		/// ��ΰ� ������ ���Ǿ����� ���� Seeker�� ���� ó������ �ʾ��� �� �ֽ��ϴ�.
		///
		/// \since 3.0.8�� �߰���
		/// ����: 3.2���� ������ ����Ǿ����ϴ�
		/// </summary>
		public bool IsDone () {
			return path == null || path.PipelineState >= PathState.Returned;
		}

		/// <summary>
		/// ��ΰ� �Ϸ�Ǿ��� �� ȣ��˴ϴ�.
		/// �� �޼���� ������ �Ű����� ������ �����Ǿ�� ������ ��������Ʈ�� �� �۵����� �ʾƺ������ϴ� (���� �⺻���� �ƴϾ����ϴ�).
		/// ����: OnPathComplete(Path,bool,bool)
		/// </summary>
		void OnPathComplete (Path path) {
			OnPathComplete(path, true, true);
		}

		/// <summary>
		/// ��ΰ� �Ϸ�Ǿ��� �� ȣ��˴ϴ�.
		/// �� �޼���� <see cref="tmpPathCallback"/> �� <see cref="pathCallback"/>�� ȣ���Ͽ� ��ó���ϰ� ��ȯ�մϴ�.
		/// </summary>
		void OnPathComplete (Path p, bool runModifiers, bool sendCallbacks) {
			if (p != null && p != path && sendCallbacks) {
				return;
			}

			if (this == null || p == null || p != path)
				return;

			if (!path.error && runModifiers) {
				// �̰��� �� Seeker�� ����� �����⿡ ��θ� ��ó���ϱ� ���� �����մϴ�
				RunModifiers(ModifierPass.PostProcess, path);
			}

			if (sendCallbacks) {
				p.Claim(this);

				lastCompletedNodePath = p.path;
				lastCompletedVectorPath = p.vectorPath;

				// �̰��� StartPath�� ȣ���� �� ������ ��� ��ο� ���� �ݹ鿡 ��θ� �����ϴ�.
				if (tmpPathCallback != null) {
					tmpPathCallback(p);
				}

				// �̰��� �ݹ鿡 ��ϵ� ��ũ��Ʈ�� ��θ� �����ϴ�.
				if (pathCallback != null) {
					pathCallback(p);
				}

				// ����: gizmos�� �׸����� #prevPath�� �����Ǿ�� �մϴ� (��, Ǯ������ ����)
				// ���� ��θ� �������� �޼��忡�� ��ȯ�� �� �ֱ� ������ #path�� �����Ǿ�� �մϴ�.
				// #path�� #prevPath�� ����� ���̹Ƿ� #prevPath�� ��ü�� ������ #prevPath�� �����ϴ� �͸����� ����մϴ�.

				// ���� ��θ� ��Ȱ���Ͽ� GC�� ���� ���ϸ� ���Դϴ�
				if (prevPath != null) {
					prevPath.Release(this, true);
				}

				prevPath = p;
			}
		}

		/// <summary>
		/// �� �Լ��� ȣ���Ͽ� ��� ����� �����մϴ�.
		/// �� �޼���� �ݹ� �Ű������� ������� �����Ƿ� �� �޼��带 ȣ���ϱ� ���� <see cref="pathCallback"/> �ʵ带 �����ؾ� �մϴ�.
		/// </summary>
		/// <param name="start">����� ������</param>
		/// <param name="end">����� ����</param>
		public Path StartPath (Vector3 start, Vector3 end) {
			return StartPath(start, end, null);
		}

		/// <summary>
		/// �� �Լ��� ȣ���Ͽ� ��� ����� �����մϴ�.
		///
		/// ��ΰ� ���Ǹ� �ݹ��� ȣ��˴ϴ�.
		/// ��ΰ� ��ҵ��� ������ (���� ��ΰ� �Ϸ�Ǳ� ���� �� ��ΰ� ��û�Ǵ� ��� ��) �ݹ��� ȣ����� �ʽ��ϴ�.
		/// </summary>
		/// <param name="start">����� ������</param>
		/// <param name="end">����� ����</param>
		/// <param name="callback">��ΰ� ���� �� ȣ���� �Լ�</param>
		public Path StartPath (Vector3 start, Vector3 end, OnPathDelegate callback) {
			return StartPath(ABPath.Construct(start, end, null), callback);
		}

		/// <summary>
		/// �� �Լ��� ȣ���Ͽ� ��� ����� �����մϴ�.
		///
		/// ��ΰ� ���Ǹ� �ݹ��� ȣ��˴ϴ�.
		/// ��ΰ� ��ҵ��� ������ (���� ��ΰ� �Ϸ�Ǳ� ���� �� ��ΰ� ��û�Ǵ� ��� ��) �ݹ��� ȣ����� �ʽ��ϴ�.
		/// </summary>
		/// <param name="start">����� ������</param>
		/// <param name="end">����� ����</param>
		/// <param name="callback">��ΰ� ���� �� ȣ���� �Լ�</param>
		/// <param name="graphMask">����� ��带 �˻��� �� �˻��� �׷����� �����ϴ� �� ���Ǵ� ����ũ�Դϴ�. #Pathfinding.NNConstraint.graphMask�� �����Ͻʽÿ�. �̰����� ��� ��û�� #graphMask�� �������մϴ�.</param>
		public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback, GraphMask graphMask)
		{
			return StartPath(ABPath.Construct(start, end, null), callback, graphMask);
		}

		/// <summary>
		/// ��� ����� �����Ϸ����� �Լ��� ȣ���Ͻʽÿ�.
		///
		/// ��ΰ� ���Ǹ� �ݹ��� ȣ��˴ϴ� (�̷� ���� �������� �� ����).
		/// �� ��� ��û�� ���Ǳ� ������ ��� ��û�� ������ ������ �ݹ��� ȣ����� �ʽ��ϴ�.
		///
		/// ����: 3.8.3 ���ķ��� �޼���� MultiTargetPath�� ���� ��� ����� �۵��մϴ�.
		/// ���� StartMultiTargetPath(MultiTargetPath) �޼���� �����ϰ� �۵��մϴ�.
		///
		/// ����: 4.1.x ���ķ��� �޼���� ��� ���� graphMask�� �Ű������� �������� �ʴ� �� ��� ���� graphMask�� ����� �ʽ��ϴ�. (�� �޼����� �ٸ� �����ε� ����). </summary>
		/// <param name="p">����� ������ ���</param>
		/// <param name="callback">��ΰ� ���� �� ȣ���� �Լ�</param>
		public Path StartPath(Path p, OnPathDelegate callback = null)
		{
			// ����ڰ� �⺻������ �׷��� ����ũ�� �������� �ʾҴٸ� �׷��� ����ũ�� �����մϴ�.
			// �̴� ����ڰ� ��Ȯ�� -1�� �����Ϸ��� ���� �� �ֱ� ������ �Ϻ������� ������, ���� �� �� �ִ� �ֻ��� �����Դϴ�.
			// �⺻���� �ƴ� Ȯ���� ���� �ڵ带 ��Ʈ���� �ʱ� ���� ȣȯ�� ������ �ַ� ���˴ϴ�.
			// �׷��� ����ũ�� �����Ϸ����� �޼��� ��� �׷��� ����ũ �ʵ尡 �ִ��� �޼����� �ٸ� �����ε带 ����ؾ� �մϴ�.
			if (p.nnConstraint.graphMask == -1) p.nnConstraint.graphMask = graphMask;
			StartPathInternal(p, callback);
			return p;
		}

		/// <summary>
		/// ��� ����� �����Ϸ����� �Լ��� ȣ���Ͻʽÿ�.
		///
		/// ��ΰ� ���Ǹ� �ݹ��� ȣ��˴ϴ� (�̷� ���� �������� �� ����).
		/// �� ��� ��û�� ���Ǳ� ������ ��� ��û�� ������ ������ �ݹ��� ȣ����� �ʽ��ϴ�.
		///
		/// ����: 3.8.3 ���ķ��� �޼���� MultiTargetPath�� ���� ��� ����� �۵��մϴ�.
		/// ���� StartMultiTargetPath(MultiTargetPath) �޼���� �����ϰ� �۵��մϴ�.
		/// </summary>
		/// <param name="p">����� ������ ���</param>
		/// <param name="callback">��ΰ� ���� �� ȣ���� �Լ�</param>
		/// <param name="graphMask">����� ��带 �˻��� �� �˻��� �׷����� �����ϴ� �� ���Ǵ� ����ũ�Դϴ�. #Pathfinding.GraphMask�� �����Ͻʽÿ�. �̰����� ��� ��û�� #graphMask�� �������մϴ�.</param>
		public Path StartPath(Path p, OnPathDelegate callback, GraphMask graphMask)
		{
			p.nnConstraint.graphMask = graphMask;
			StartPathInternal(p, callback);
			return p;
		}

		/// <summary>���� ��θ� �����ϰ� ���� Ȱ�� ��η� ǥ���ϴ� ���� �޼����Դϴ�.</summary>
		void StartPathInternal(Path p, OnPathDelegate callback)
		{
			p.callback += onPathDelegate;

			p.enabledTags = traversableTags;
			p.tagPenalties = tagPenalties;

			// ��ΰ� ó�� ���̸� �Ϸ� ���°� ������ �ƴ� ��� ���� ��ΰ� ���� ó������ �ʾұ� ������
			// ��θ� ����ϰ� �� �ٸ� ������ ��Ȱ����� �ʾҴ��� Ȯ���մϴ�.
			if (path != null && path.PipelineState <= PathState.Processing && path.CompleteState != PathCompleteState.Error && lastPathID == path.pathID)
			{
				path.FailWithError("�� ��ΰ� ��û�� �� ��ΰ� ��ҵǾ����ϴ�.\n" +
					"�̰��� �̹� ��� ���� ��ΰ� �̹� ��û�� ���¿��� �� ��ΰ� ���Ǳ⸦ ��ٸ��� �ʰ� ���� ��û�ϱ� ���� " +
					"Seeker���� �� ��θ� ��û�� �� �߻��մϴ�. �Ƹ��� ����� ���ϴ� ���� ���Դϴ�.\n" +
					"�̸� ���� �޴� ��� ��� ��û�� ��� �����ϴ��� ����� �� �ʿ䰡 �ֽ��ϴ�.");
				// ��ҵ� ��ο� ���� �ݹ��� �������� �ʽ��ϴ�.
			}

			// ���� ��θ� Ȱ�� ��η� �����մϴ�.
			path = p;
			tmpPathCallback = callback;

			// �� ��ΰ� ��Ȱ����� �ʵ��� ��� ID�� �����մϴ�.
			lastPathID = path.pathID;

			// ��θ� ���� ó���մϴ�.
			RunModifiers(ModifierPass.PreProcess, path);

			// ��û�� �н����δ��� �����մϴ�.
			AstarPath.StartPath(path);
		}


		/// <summary>Seeker�� gizmo�� �׸��ϴ�.</summary>
		public void OnDrawGizmos () {
			if (lastCompletedNodePath == null || !drawGizmos) {
				return;
			}

			if (detailedGizmos) {
				Gizmos.color = new Color(0.7F, 0.5F, 0.1F, 0.5F);

				if (lastCompletedNodePath != null) {
					for (int i = 0; i < lastCompletedNodePath.Count-1; i++) {
						Gizmos.DrawLine((Vector3)lastCompletedNodePath[i].position, (Vector3)lastCompletedNodePath[i+1].position);
					}
				}
			}

			Gizmos.color = new Color(0, 1F, 0, 1F);

			if (lastCompletedVectorPath != null) {
				for (int i = 0; i < lastCompletedVectorPath.Count-1; i++) {
					Gizmos.DrawLine(lastCompletedVectorPath[i], lastCompletedVectorPath[i+1]);
				}
			}
		}

		protected override int OnUpgradeSerializedData (int version, bool unityThread) {
			if (graphMaskCompatibility != -1) {
				Debug.Log("Loaded " + graphMaskCompatibility + " " + graphMask.value);
				graphMask = graphMaskCompatibility;
				graphMaskCompatibility = -1;
			}
			return base.OnUpgradeSerializedData(version, unityThread);
		}
	}
}
