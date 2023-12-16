using Pathfinding.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
	/// <summary>
	/// ��ο� ��带 �ٷ�� ������ �Լ��� �����ϴ� Ŭ�����Դϴ�.
	/// �� Ŭ������ �ַ� <see cref="Pathfinding.GraphNode"/> Ŭ������ �Բ� �۵��ϸ�, ��带 �������� ������ �Լ��� AstarPath.GetNearest�Դϴ�.
	/// ����: <see cref="AstarPath.GetNearest"/>
	/// ����: <see cref="Pathfinding.GraphUpdateUtilities"/>
	/// ����: <see cref="Pathfinding.GraphUtilities"/>
	/// \ingroup utils
	/// </summary>
	public static class PathUtilities {
		/// <summary>
		/// node1���� node2�� �� �� �ִ� ��ΰ� �ִ��� ��ȯ�մϴ�.
		/// �� �޼���� ������ ���� ������ ����ϱ� ������ �ſ� �����ϴ�.
		///
		/// <code>
		/// GraphNode node1 = AstarPath.active.GetNearest(point1, NNConstraint.Default).node;
		/// GraphNode node2 = AstarPath.active.GetNearest(point2, NNConstraint.Default).node;
		///
		/// if (PathUtilities.IsPathPossible(node1, node2)) {
		///     // �� ��� ���̿� ��ΰ� �ֽ��ϴ�.
		/// }
		/// </code>
		///
		/// ����: �׷��� ������Ʈ (�۵� ��ũ�� ���� �¶��� ���� ����)
		/// ����: <see cref="AstarPath.GetNearest"/>
		/// </summary>
		public static bool IsPathPossible (GraphNode node1, GraphNode node2) {
			return node1.Walkable && node2.Walkable && node1.Area == node2.Area;
		}

		/// <summary>
		/// ��� ��� ���̿� �� �� �ִ� ��ΰ� �ִ��� ���θ� ��ȯ�մϴ�.
		///
		/// �� ��Ͽ� ���ؼ��� true�� ��ȯ�մϴ�.
		///
		/// ����: �׷��� ������Ʈ (�۵� ��ũ�� ���� �¶��� ���� ����)
		///
		/// ����: <see cref="AstarPath.GetNearest"/>
		/// </summary>
		public static bool IsPathPossible (List<GraphNode> nodes) {
			if (nodes.Count == 0) return true;

			uint area = nodes[0].Area;
			for (int i = 0; i < nodes.Count; i++) if (!nodes[i].Walkable || nodes[i].Area != area) return false;
			return true;
		}

		/// <summary>
		/// ��� ��� ���̿� �� �� �ִ� ��ΰ� �ִ��� ���θ� ��ȯ�մϴ�.
		/// ����: �׷��� ������Ʈ (�۵� ��ũ�� ���� �¶��� ���� ����)
		///
		/// �� �޼���� ������ ù ��° ��尡 ��� �ٸ� ��忡 ������ �� �ִ����� Ȯ���մϴ�.
		/// ���� ��� ��쿡 �׷��� ������ ������Դϴ�.
		/// ��������� �ܹ��� ������ �����ϴ� ��찡 ���ٸ� ������ �ʿ� ���� �� �޼��带 ����� �� �ֽ��ϴ�.
		///
		/// �� ��Ͽ� ���ؼ��� true�� ��ȯ�մϴ�.
		///
		/// ���: �� �޼���� tagMask�� ������� �ʴ� IsPathPossible �޼��庸�� �ξ� �����ϴ�.
		///
		/// ����: <see cref="AstarPath.GetNearest"/>
		/// </summary>
		public static bool IsPathPossible (List<GraphNode> nodes, int tagMask) {
			if (nodes.Count == 0) return true;

			// ù ��° ��尡 ��ȿ�� �±׸� ������ �ִ��� Ȯ���մϴ�.
			if (((tagMask >> (int)nodes[0].Tag) & 1) == 0) return false;

			// ���� �˻� ���� ����
			if (!IsPathPossible(nodes)) return false;

			// ù ��° ��尡 ��� �ٸ� ��忡 ������ �� �ִ��� Ȯ���մϴ�.
			var reachable = GetReachableNodes(nodes[0], tagMask);
			bool result = true;

			// ù ��° ��尡 �ٸ� ��� ��忡 ������ �� �ִ��� Ȯ���մϴ�.
			for (int i = 1; i < nodes.Count; i++) {
				if (!reachable.Contains(nodes[i])) {
					result = false;
					break;
				}
			}

			// �ӽ� ����Ʈ�� ��ȯ�մϴ�.
			ListPool<GraphNode>.Release(ref reachable);

			return result;
		}

		/// <summary>
		/// �õ� ��忡�� ���� ������ ��� ��带 ��ȯ�մϴ�.
		/// �� �Լ��� �׷����� ���� �켱 Ž��(DFS) �Ǵ� ȫ�� ä��� ������� �˻��ϰ� �õ� ��忡�� ���� ������ ��� ��带 ��ȯ�մϴ�.
		/// ��κ��� ��� �̰��� �õ� ���� ���� ������ ���ϴ� ��� ��带 ��ȯ�ϴ� �Ͱ� �����մϴ�.
		/// �����Ϳ��� ������ ����� �ٸ� �������� ǥ�õ˴ϴ�.
		/// ������ ���ܴ� ������ � �κп��� �õ� ������ �Ϲ��� ��ΰ� ������ �õ� ��忡�� �׷����� �� �κ������� ��ΰ� ���� ���Դϴ�.
		///
		/// ��ȯ�� ����� Ư���� ������� ���ĵ��� �ʽ��ϴ�.
		///
		/// ���� ������ ��� ���� ���� �� �Լ��� ��꿡 ����� �ð��� �ɸ� �� �����Ƿ� �ʹ� ���� ������� ���ʽÿ�. �̷� ���� ������ ������ �ӵ��� ������ �� �� �ֽ��ϴ�.
		///
		/// �ڼ��� ������ ��Ʈ����ũ�� �����Ͻʽÿ� (�¶��� �������� �۵� ��ũ Ȯ��).
		///
		/// ��ȯ��: �õ� ��忡�� ���� ������ ��� ��带 �����ϴ� List<Node>.
		/// �޸� ������ ���� ��ȯ�� ����� Ǯ���Ǿ�� �մϴ�. Pathfinding.Util.ListPool ����.
		/// </summary>
		/// <param name="seed">�˻��� ������ ����Դϴ�.</param>
		/// <param name="tagMask">�±׿� ���� ������ ����ũ�Դϴ�. �̰��� ��Ʈ����ũ�Դϴ�.</param>
		/// <param name="filter">��� �˻��� ���� ������ �����Դϴ�. �̰��� �±� ����ũ = -1�� �Բ� ����Ͽ� ���Ͱ� ��� ���� �����ϵ��� �� �� �ֽ��ϴ�.
		///      ���� �Լ��� false�� ��ȯ�ϸ� ��尡 �� �� ���� ������ ó���˴ϴ�.</param>
		public static List<GraphNode> GetReachableNodes(GraphNode seed, int tagMask = -1, System.Func<GraphNode, bool> filter = null)
		{
			Stack<GraphNode> dfsStack = StackPool<GraphNode>.Claim();
			List<GraphNode> reachable = ListPool<GraphNode>.Claim();

			/// <summary>TODO: Pool</summary>
			var map = new HashSet<GraphNode>();

			System.Action<GraphNode> callback;
			// ���� ��θ� ����� �� �ִ��� Ȯ���մϴ�.
			if (tagMask == -1 && filter == null) {
				callback = (GraphNode node) => {
					if (node.Walkable && map.Add(node)) {
						reachable.Add(node);
						dfsStack.Push(node);
					}
				};
			} else {
				callback = (GraphNode node) => {
					if (node.Walkable && ((tagMask >> (int)node.Tag) & 0x1) != 0 && map.Add(node)) {
						if (filter != null && !filter(node)) return;

						reachable.Add(node);
						dfsStack.Push(node);
					}
				};
			}

			callback(seed);

			while (dfsStack.Count > 0) {
				dfsStack.Pop().GetConnections(callback);
			}

			StackPool<GraphNode>.Release(dfsStack);
			return reachable;
		}

		static Queue<GraphNode> BFSQueue;
		static Dictionary<GraphNode, int> BFSMap;

		/// <summary>
		/// �õ� ���κ��� �־��� ��� �Ÿ������� ��� ��带 ��ȯ�մϴ�.
		/// �� �Լ��� �׷����� �ʺ� �켱 Ž��(BFS) �Ǵ� ȫ�� ä��� ������� �˻��ϰ� �õ� ���κ��� ������ ��� �Ÿ� ������ ���� ������ ��� ��带 ��ȯ�մϴ�.
		/// ��κ��� ��� ���̰� ����� ū ��� �̰��� �õ� ���� ���� ������ ���ϴ� ��� ��带 ��ȯ�ϴ� �Ͱ� �����մϴ�.
		/// �����Ϳ��� ������ ����� �ٸ� �������� ǥ�õ˴ϴ�.
		/// ������ ���ܴ� ������ � �κп��� �õ� ������ �Ϲ��� ��ΰ� ������ �õ� ��忡�� �� �κ������� ��ΰ� ���� ���Դϴ�.
		///
		/// ��ȯ�� ����� �õ� ���κ����� ��� �Ÿ��� �������� ���ĵ˴ϴ�.
		/// ��, �Ÿ��� �õ忡�� �ش� �������� �ִ� ��ΰ� ����ϴ� ��� ���� �����˴ϴ�.
		/// �Ÿ� ������ �޸���ƽ, �г�Ƽ �Ǵ� �±� �г�Ƽ�� ������� �ʽ��ϴ�.
		///
		/// ��� ���� ���� �� �Լ��� ��꿡 ����� �ð��� �ɸ� �� �����Ƿ� �ʹ� ���� ������� ���ʽÿ�. �̷� ���� ������ ������ �ӵ��� ������ �� �� �ֽ��ϴ�.
		///
		/// ��ȯ��: �õ� ���κ��� ������ ��� �Ÿ� ������ ���� ������ ��� ��带 �����ϴ� List<GraphNode>.
		/// �޸� ������ ���� ��ȯ�� ����� Ǯ���Ǿ�� �մϴ�. Pathfinding.Util.ListPool ����
		///
		/// ���: �� �޼���� ������ �������� �ʽ��ϴ�. Unity ������(��, �Ϲ� ���� �ڵ�)������ ����Ͻʽÿ�.
		///
		/// �Ʒ� ������ ���� ���� �ٸ� ����� BFS ����� �����ݴϴ�. ���� GetPointsOnNodes�� ����Ͽ� ��忡�� ���ø��˴ϴ�.
		/// [�¶��� �������� ���� Ȯ��]
		/// </summary>
		/// <param name="seed">�˻��� ������ ����Դϴ�.</param>
		/// <param name="depth">�õ� ���κ����� �ִ� ��� �Ÿ��Դϴ�.</param>
		/// <param name="tagMask">�±׿� ���� ������ ����ũ�Դϴ�. �̰��� ��Ʈ����ũ�Դϴ�.</param>
		/// <param name="filter">��� �˻��� ���� ������ �����Դϴ�. �̰��� depth = int.MaxValue �� tagMask = -1�� �Բ� ����Ͽ� ���Ͱ� ��� ���� �����ϵ��� �� �� �ֽ��ϴ�.
		///      ���� �Լ��� false�� ��ȯ�ϸ� ��尡 �� �� ���� ������ ó���˴ϴ�.</param>
		public static List<GraphNode> BFS(GraphNode seed, int depth, int tagMask = -1, System.Func<GraphNode, bool> filter = null)
		{
#if ASTAR_PROFILE
    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
    watch.Start();
#endif

			BFSQueue = BFSQueue ?? new Queue<GraphNode>();
			var que = BFSQueue;

			BFSMap = BFSMap ?? new Dictionary<GraphNode, int>();
			var map = BFSMap;

			// �� �Լ��� ���� ȣ�⿡�� ���ܰ� �߻��ϰ� que�� map�� �������� �ʾ��� �� �����Ƿ� ���⿡�� ����� ���� �����ϴ�.
			que.Clear();
			map.Clear();

			List<GraphNode> result = ListPool<GraphNode>.Claim();

			int currentDist = -1;
			System.Action<GraphNode> callback;
			if (tagMask == -1) {
				callback = node => {
					if (node.Walkable && !map.ContainsKey(node)) {
						if (filter != null && !filter(node)) return;

						map.Add(node, currentDist+1);
						result.Add(node);
						que.Enqueue(node);
					}
				};
			} else {
				callback = node => {
					if (node.Walkable && ((tagMask >> (int)node.Tag) & 0x1) != 0 && !map.ContainsKey(node)) {
						if (filter != null && !filter(node)) return;

						map.Add(node, currentDist+1);
						result.Add(node);
						que.Enqueue(node);
					}
				};
			}

			callback(seed);

			while (que.Count > 0) {
				GraphNode n = que.Dequeue();
				currentDist = map[n];

				if (currentDist >= depth) break;

				n.GetConnections(callback);
			}

			que.Clear();
			map.Clear();

#if ASTAR_PROFILE
			watch.Stop();
			Debug.Log((1000*watch.Elapsed.TotalSeconds).ToString("0.0 ms"));
#endif
			return result;
		}

		/// <summary>
		/// ���� �߽����� ��ø�� ���� ������ ����Ʈ�� ��ȯ�մϴ�. �ٸ� ����Ʈ���� �ּ� ������ �����մϴ�.
		/// ����Ʈ�� ���� �κ���Ʈ(involutes)�� ���Դϴ�.
		/// �ڼ��� ������ ������ �����Ͻʽÿ�: http://en.wikipedia.org/wiki/Involute
		/// ���⿡ �Ϻ� ���� Ư���� �ֽ��ϴ�.
		/// ��� ����Ʈ�� ���� ������ �и��˴ϴ�.
		/// �� �޼���� O(n)�Դϴ�. �ڵ带 ������ ���� �˻��� ������ �ش� ���� �˻��� �ܰ� ���� ���Ѽ��� �����Ƿ� �α� ������ �������� �ʽ��ϴ�.
		/// 
		/// ����: ��� �� ����� ��Ȱ���Ͽ� �Ҵ��� ���̴� ���� ����Ͻʽÿ�.
		/// ����: Pathfinding.Util.ListPool
		/// </summary>
		public static List<Vector3> GetSpiralPoints (int count, float clearance) {
			List<Vector3> pts = ListPool<Vector3>.Claim(count);

			// ���� ���� ������ (���������� ���� �����ϴ� �� ����)
			// ȸ�� ������ �и� �Ÿ��κ��� ���˴ϴ�.
			float a = clearance/(2*Mathf.PI);
			float t = 0;


			pts.Add(InvoluteOfCircle(a, t));

			for (int i = 0; i < count; i++) {
				Vector3 prev = pts[pts.Count-1];

				// d = -t0/2 + sqrt( t0^2/4 + 2d/a )
				// ���ݺ��� ū ��ũ �Ÿ��� ������ �ּ� ���� (����)
				float d = -t / 2 + Mathf.Sqrt(t * t / 4 + 2 * clearance / a);

				// ���� ����Ʈ�� �� ����Ʈ�� �и��ϱ� ���� ���� �˻�
				float mn = t + d;
				float mx = t + 2*d;
				while (mx - mn > 0.01f) {
					float mid = (mn + mx)/2;
					Vector3 p = InvoluteOfCircle(a, mid);
					if ((p - prev).sqrMagnitude < clearance*clearance) {
						mn = mid;
					} else {
						mx = mid;
					}
				}

				pts.Add(InvoluteOfCircle(a, mx));
				t = mx;
			}

			return pts;
		}

		/// <summary>
		/// ���� �κ���Ʈ(involutes)�� XZ ��ǥ�� ��ȯ�մϴ�.
		/// �ڼ��� ������ ������ �����Ͻʽÿ�: http://en.wikipedia.org/wiki/Involute
		/// </summary>
		private static Vector3 InvoluteOfCircle(float a, float t)
		{
			return new Vector3(a * (Mathf.Cos(t) + t * Mathf.Sin(t)), 0, a * (Mathf.Sin(t) - t * Mathf.Cos(t)));
		}

		/// <summary>
		/// p �ֺ��� �׷��� ���� ������ ����ϸ�, ������ ���� clearance��ŭ �и��˴ϴ�.
		/// ��� ���� ã������ �װ��� �׷� �߽����� ó���˴ϴ�.
		/// </summary>
		/// <param name="p">�ֺ��� ���� ������ ����</param>
		/// <param name="g">���� �˻翡 ����� �׷���. �ϳ��� �׷����� ����ϴ� ��� AstarPath.active.graphs[0]�� IRaycastableGraph�� ���� �� �ֽ��ϴ�.
		/// ��� �׷����� ���� �˻� ������ ���� �ƴϸ�, recast, navmesh �� grid �׷����� ���� �˻� �����մϴ�. recast�� navmesh���� ���� �� �۵��մϴ�.</param>
		/// <param name="previousPoints">������ ����� ���� ���. �̷��� ������ ���� ������ �־�� �մϴ�.
		///      ���ο� ������ ��� ���� ���� ������ ����ϴ�. ����� ���� ������ �ֽ��ϴ�.</param>
		/// <param name="radius">���� ������ �� �Ÿ� �̳��� ��ġ�մϴ�.</param>
		/// <param name="clearanceRadius">������ ������ ��� ���� �� �Ÿ� �̻� ������ �־�� �մϴ�.</param>
		public static void GetPointsAroundPointWorld(Vector3 p, IRaycastableGraph g, List<Vector3> previousPoints, float radius, float clearanceRadius)
		{
			if (previousPoints.Count == 0) return;

			Vector3 avg = Vector3.zero;
			for (int i = 0; i < previousPoints.Count; i++) avg += previousPoints[i];
			avg /= previousPoints.Count;

			for (int i = 0; i < previousPoints.Count; i++) previousPoints[i] -= avg;

			GetPointsAroundPoint(p, g, previousPoints, radius, clearanceRadius);
		}

		/// <summary>
		/// �߽� �ֺ��� ��ġ�ϰ� ���� clearance��ŭ �и��Ǵ� ������ ����մϴ�.
		/// �߽ɿ��� � �������� �ִ� �Ÿ��� radius�Դϴ�.
		/// ���� previousPoints�� ���� ��ġ�ϰ�, �����ϸ� ������ ���� ���õ˴ϴ�.
		/// �׷� �̵��� ���� ��ǥ ���� �����Ϸ��� �̻����Դϴ�. �׷��� ��� ��ġ���� ���� ������Ʈ ���� ��� �����ϸ�
		/// �� �޼���� �׷� ������ ������Ʈ�� ���� �̵����� �ʵ��� ��ǥ ���� ��ȯ�մϴ�. �̴� �ð������� �ŷ����̸�, ������ ȸ�Ǹ� ����� �� ��鸲�� ���Դϴ�.
		/// 
		/// TODO: ���� �׽�Ʈ �ۼ�
		/// </summary>
		/// <param name="center">�ֺ��� ���� ������ ����</param>
		/// <param name="g">���� �˻翡 ����� �׷���. �ϳ��� �׷����� ����ϴ� ��� AstarPath.active.graphs[0]�� IRaycastableGraph�� ���� �� �ֽ��ϴ�.
		/// ��� �׷����� ���� �˻� ������ ���� �ƴϸ�, recast, navmesh �� grid �׷����� ���� �˻� �����մϴ�. recast�� navmesh���� ���� �� �۵��մϴ�.</param>
		/// <param name="previousPoints">������ ����� ���� ���. �̷��� ������ �߽��� �������� ��������� ó���Ǿ�� �մϴ�.
		///      ���ο� ������ ��� ���� ���� ������ ����ϴ�. ����� �߽��� �������� ������� �ƴ� ���� ������ �ֽ��ϴ�.</param>
		/// <param name="radius">���� ������ �� �Ÿ� �̳��� ��ġ�մϴ�.</param>
		/// <param name="clearanceRadius">������ ������ ��� ���� �� �Ÿ� �̻� ������ �־�� �մϴ�.</param>
		public static void GetPointsAroundPoint(Vector3 center, IRaycastableGraph g, List<Vector3> previousPoints, float radius, float clearanceRadius)
		{
			if (g == null) throw new System.ArgumentNullException("g");

			var graph = g as NavGraph;

			if (graph == null) throw new System.ArgumentException("g is not a NavGraph");

			NNInfoInternal nn = graph.GetNearestForce(center, NNConstraint.Default);
			center = nn.clampedPosition;

			if (nn.node == null) {
				// No valid point to start from
				return;
			}


			// ���� �е��� 0.5�� ������ �����ϴ� ���� �������� Ȯ���մϴ�.
			radius = Mathf.Max(radius, 1.4142f * clearanceRadius * Mathf.Sqrt(previousPoints.Count)); // Mathf.Sqrt(previousPoints.Count*clearanceRadius*2));
			clearanceRadius *= clearanceRadius;

			for (int i = 0; i < previousPoints.Count; i++) {
				Vector3 dir = previousPoints[i];
				float magn = dir.magnitude;

				if (magn > 0) dir /= magn;

				float newMagn = radius;//magn > radius ? radius : magn;
				dir *= newMagn;

				GraphHitInfo hit;

				int tests = 0;
				while (true) {
					Vector3 pt = center + dir;

					if (g.Linecast(center, pt, nn.node, out hit)) {
						if (hit.point == Vector3.zero) {
							// ����: ���� �˻簡 ������ ����
							// ���� �õ��� �� ��쿡�� ����մϴ�.
							tests++;
							if (tests > 8) {
								previousPoints[i] = pt;
								break;
							}
						} else {
							pt = hit.point;
						}
					}

					bool worked = false;

					for (float q = 0.1f; q <= 1.0f; q += 0.05f) {
						Vector3 qt = Vector3.Lerp(center, pt, q);
						worked = true;
						for (int j = 0; j < i; j++) {
							if ((previousPoints[j] - qt).sqrMagnitude < clearanceRadius) {
								worked = false;
								break;
							}
						}

						// 8ȸ �õ� ���ĳ� ��ȿ�� ���� ã�� ���
						if (worked || tests > 8) {
							worked = true;
							previousPoints[i] = qt;
							break;
						}
					}

					// ��ø�� ������ �������ɴϴ�.
					if (worked) {
						break;
					}

					// ��ȿ�� ���� ã�� �� ���� ���, ���� �� �õ��� ���� ��踦 �ణ ����Ͽ� Ȯ���� ���Դϴ�.
					clearanceRadius *= 0.9f;
					// �̰��� �� ���� Ȯ���� ���� �����ڸ��� ����� 2D ����Ʈ�� �����մϴ�.
					dir = Random.onUnitSphere * Mathf.Lerp(newMagn, radius, tests / 5);
					dir.y = 0;
					tests++;
				}
			}
		}

		/// <summary>
		/// ������ ��� ���� ������ ���õ� ���� ��ȯ�ϸ�, �� ���� ���� clearanceRadius��ŭ �и��˴ϴ�.
		/// ��� ���� ���� �����ϴ� ���� TriangleMeshNode (Recast �׷��� �� Navmesh �׷������� ���) �� GridNode (GridGraph���� ���)�� ���ؼ��� �۵��մϴ�.
		/// �ٸ� ��� ������ ���, ����� ��ġ�� ���˴ϴ�.
		///
		/// ��ȿ�� ���� ã�� �� ���� ��� clearanceRadius�� ���ҵ˴ϴ�.
		///
		/// ����: �� �޼���� ��� ���� ��� ��尡 Ư���� ��쿡 ���ؼ��� ������ ������ �����մϴ�.
		/// ��ü������ ù ��° ��尡 TriangleMeshNode �Ǵ� GridNode�� �ƴ� ���,
		/// ��� ���� ��� ��尡 ������ ǥ�� ������ ���� ������ �����Ͽ� ���� ��θ� ����մϴ�.
		/// (�Ϲ������� ǥ�� ������ 0�̰� ��尡 ��� PointNode�� ����Դϴ�).
		/// </summary>
		public static List<Vector3> GetPointsOnNodes (List<GraphNode> nodes, int count, float clearanceRadius = 0) {
			if (nodes == null) throw new System.ArgumentNullException("nodes");
			if (nodes.Count == 0) throw new System.ArgumentException("no nodes passed");

			List<Vector3> pts = ListPool<Vector3>.Claim(count);

			// �簢��
			clearanceRadius *= clearanceRadius;

			if (clearanceRadius > 0 || nodes[0] is TriangleMeshNode
#if !ASTAR_NO_GRID_GRAPH
				|| nodes[0] is GridNode
#endif
				) {
				// ��� ����� ���� ����
				List<float> accs = ListPool<float>.Claim(nodes.Count);

				// ���ݱ��� ��� ����� �� ����
				float tot = 0;

				for (int i = 0; i < nodes.Count; i++) {
					var surfaceArea = nodes[i].SurfaceArea();
					// �׻� ������ ��带 �����ϴ� ���� �ƴ϶� ǥ�� ��带 �����ϱ� ���� ��尡 0�� ��쿡�� ǥ�� ������ �����ϵ��� �մϴ�.
					surfaceArea += 0.001f;
					tot += surfaceArea;
					accs.Add(tot);
				}

				for (int i = 0; i < count; i++) {
					//Pick point
					int testCount = 0;
					int testLimit = 10;
					bool worked = false;

					while (!worked) {
						worked = true;


						// ��ȿ�� ���� ã�� �� ���� ���, ���� ã�� ������ clearanceRadius�� ���������� ����ϴ�.
						if (testCount >= testLimit)
						{
							// ����: clearanceRadius�� ���� �������Դϴ�.
							clearanceRadius *= 0.9f*0.9f;
							testLimit += 10;
							if (testLimit > 100) clearanceRadius = 0;
						}

						// ������ ���� ����ġ�� �ΰ� ��� ���� ��� �� ������ ��� ����
						float tg = Random.value*tot;
						int v = accs.BinarySearch(tg);
						if (v < 0) v = ~v;

						if (v >= nodes.Count) {
							// ���� ���̽� ó��
							worked = false;
							continue;
						}

						var node = nodes[v];
						var p = node.RandomPointOnSurface();

						// �ٸ� ����κ��� ���� �Ÿ� ������ �ִ��� �׽�Ʈ
						if (clearanceRadius > 0) {
							for (int j = 0; j < pts.Count; j++) {
								if ((pts[j]-p).sqrMagnitude < clearanceRadius) {
									worked = false;
									break;
								}
							}
						}

						if (worked) {
							pts.Add(p);
							break;
						}
						testCount++;
					}
				}

				ListPool<float>.Release(ref accs);
			} else {
				// ���� ���, ��� ��尡 ������ ������ ���� (���� 0)
				for (int i = 0; i < count; i++) {
					pts.Add((Vector3)nodes[Random.Range(0, nodes.Count)].RandomPointOnSurface());
				}
			}

			return pts;
		}
	}
}
