using Pathfinding.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
	/// <summary>
	/// 경로와 노드를 다루는 유용한 함수를 포함하는 클래스입니다.
	/// 이 클래스는 주로 <see cref="Pathfinding.GraphNode"/> 클래스와 함께 작동하며, 노드를 가져오는 유용한 함수는 AstarPath.GetNearest입니다.
	/// 참조: <see cref="AstarPath.GetNearest"/>
	/// 참조: <see cref="Pathfinding.GraphUpdateUtilities"/>
	/// 참조: <see cref="Pathfinding.GraphUtilities"/>
	/// \ingroup utils
	/// </summary>
	public static class PathUtilities {
		/// <summary>
		/// node1에서 node2로 갈 수 있는 경로가 있는지 반환합니다.
		/// 이 메서드는 사전에 계산된 정보만 사용하기 때문에 매우 빠릅니다.
		///
		/// <code>
		/// GraphNode node1 = AstarPath.active.GetNearest(point1, NNConstraint.Default).node;
		/// GraphNode node2 = AstarPath.active.GetNearest(point2, NNConstraint.Default).node;
		///
		/// if (PathUtilities.IsPathPossible(node1, node2)) {
		///     // 두 노드 사이에 경로가 있습니다.
		/// }
		/// </code>
		///
		/// 참조: 그래프 업데이트 (작동 링크를 위한 온라인 문서 참조)
		/// 참조: <see cref="AstarPath.GetNearest"/>
		/// </summary>
		public static bool IsPathPossible (GraphNode node1, GraphNode node2) {
			return node1.Walkable && node2.Walkable && node1.Area == node2.Area;
		}

		/// <summary>
		/// 모든 노드 사이에 갈 수 있는 경로가 있는지 여부를 반환합니다.
		///
		/// 빈 목록에 대해서는 true를 반환합니다.
		///
		/// 참조: 그래프 업데이트 (작동 링크를 위한 온라인 문서 참조)
		///
		/// 참조: <see cref="AstarPath.GetNearest"/>
		/// </summary>
		public static bool IsPathPossible (List<GraphNode> nodes) {
			if (nodes.Count == 0) return true;

			uint area = nodes[0].Area;
			for (int i = 0; i < nodes.Count; i++) if (!nodes[i].Walkable || nodes[i].Area != area) return false;
			return true;
		}

		/// <summary>
		/// 모든 노드 사이에 갈 수 있는 경로가 있는지 여부를 반환합니다.
		/// 참조: 그래프 업데이트 (작동 링크를 위한 온라인 문서 참조)
		///
		/// 이 메서드는 실제로 첫 번째 노드가 모든 다른 노드에 도달할 수 있는지만 확인합니다.
		/// 거의 모든 경우에 그래프 연결은 양방향입니다.
		/// 명시적으로 단방향 연결을 생성하는 경우가 없다면 걱정할 필요 없이 이 메서드를 사용할 수 있습니다.
		///
		/// 빈 목록에 대해서는 true를 반환합니다.
		///
		/// 경고: 이 메서드는 tagMask를 사용하지 않는 IsPathPossible 메서드보다 훨씬 느립니다.
		///
		/// 참조: <see cref="AstarPath.GetNearest"/>
		/// </summary>
		public static bool IsPathPossible (List<GraphNode> nodes, int tagMask) {
			if (nodes.Count == 0) return true;

			// 첫 번째 노드가 유효한 태그를 가지고 있는지 확인합니다.
			if (((tagMask >> (int)nodes[0].Tag) & 1) == 0) return false;

			// 빠른 검사 먼저 수행
			if (!IsPathPossible(nodes)) return false;

			// 첫 번째 노드가 모든 다른 노드에 도달할 수 있는지 확인합니다.
			var reachable = GetReachableNodes(nodes[0], tagMask);
			bool result = true;

			// 첫 번째 노드가 다른 모든 노드에 도달할 수 있는지 확인합니다.
			for (int i = 1; i < nodes.Count; i++) {
				if (!reachable.Contains(nodes[i])) {
					result = false;
					break;
				}
			}

			// 임시 리스트를 반환합니다.
			ListPool<GraphNode>.Release(ref reachable);

			return result;
		}

		/// <summary>
		/// 시드 노드에서 도달 가능한 모든 노드를 반환합니다.
		/// 이 함수는 그래프를 깊이 우선 탐색(DFS) 또는 홍수 채우기 방식으로 검색하고 시드 노드에서 도달 가능한 모든 노드를 반환합니다.
		/// 대부분의 경우 이것은 시드 노드와 같은 영역에 속하는 모든 노드를 반환하는 것과 동일합니다.
		/// 에디터에서 영역은 노드의 다른 색상으로 표시됩니다.
		/// 유일한 예외는 영역의 어떤 부분에서 시드 노드로의 일방향 경로가 있지만 시드 노드에서 그래프의 그 부분으로의 경로가 없을 때입니다.
		///
		/// 반환된 목록은 특정한 방식으로 정렬되지 않습니다.
		///
		/// 도달 가능한 노드 수에 따라 이 함수는 계산에 상당한 시간이 걸릴 수 있으므로 너무 자주 사용하지 마십시오. 이로 인해 게임의 프레임 속도에 영향을 줄 수 있습니다.
		///
		/// 자세한 내용은 비트마스크를 참조하십시오 (온라인 문서에서 작동 링크 확인).
		///
		/// 반환값: 시드 노드에서 도달 가능한 모든 노드를 포함하는 List<Node>.
		/// 메모리 관리를 위해 반환된 목록은 풀링되어야 합니다. Pathfinding.Util.ListPool 참조.
		/// </summary>
		/// <param name="seed">검색을 시작할 노드입니다.</param>
		/// <param name="tagMask">태그에 대한 선택적 마스크입니다. 이것은 비트마스크입니다.</param>
		/// <param name="filter">노드 검색을 위한 선택적 필터입니다. 이것을 태그 마스크 = -1과 함께 사용하여 필터가 모든 것을 결정하도록 할 수 있습니다.
		///      필터 함수가 false를 반환하면 노드가 갈 수 없는 것으로 처리됩니다.</param>
		public static List<GraphNode> GetReachableNodes(GraphNode seed, int tagMask = -1, System.Func<GraphNode, bool> filter = null)
		{
			Stack<GraphNode> dfsStack = StackPool<GraphNode>.Claim();
			List<GraphNode> reachable = ListPool<GraphNode>.Claim();

			/// <summary>TODO: Pool</summary>
			var map = new HashSet<GraphNode>();

			System.Action<GraphNode> callback;
			// 빠른 경로를 사용할 수 있는지 확인합니다.
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
		/// 시드 노드로부터 주어진 노드 거리까지의 모든 노드를 반환합니다.
		/// 이 함수는 그래프를 너비 우선 탐색(BFS) 또는 홍수 채우기 방식으로 검색하고 시드 노드로부터 지정된 노드 거리 내에서 도달 가능한 모든 노드를 반환합니다.
		/// 대부분의 경우 깊이가 충분히 큰 경우 이것은 시드 노드와 같은 영역에 속하는 모든 노드를 반환하는 것과 동일합니다.
		/// 에디터에서 영역은 노드의 다른 색상으로 표시됩니다.
		/// 유일한 예외는 영역의 어떤 부분에서 시드 노드로의 일방향 경로가 있지만 시드 노드에서 그 부분으로의 경로가 없을 때입니다.
		///
		/// 반환된 목록은 시드 노드로부터의 노드 거리를 기준으로 정렬됩니다.
		/// 즉, 거리는 시드에서 해당 노드까지의 최단 경로가 통과하는 노드 수로 측정됩니다.
		/// 거리 측정은 휴리스틱, 패널티 또는 태그 패널티를 고려하지 않습니다.
		///
		/// 노드 수에 따라 이 함수는 계산에 상당한 시간이 걸릴 수 있으므로 너무 자주 사용하지 마십시오. 이로 인해 게임의 프레임 속도에 영향을 줄 수 있습니다.
		///
		/// 반환값: 시드 노드로부터 지정된 노드 거리 내에서 도달 가능한 모든 노드를 포함하는 List<GraphNode>.
		/// 메모리 관리를 위해 반환된 목록은 풀링되어야 합니다. Pathfinding.Util.ListPool 참조
		///
		/// 경고: 이 메서드는 스레드 안전하지 않습니다. Unity 스레드(즉, 일반 게임 코드)에서만 사용하십시오.
		///
		/// 아래 비디오는 깊이 값이 다른 경우의 BFS 결과를 보여줍니다. 점은 GetPointsOnNodes를 사용하여 노드에서 샘플링됩니다.
		/// [온라인 문서에서 비디오 확인]
		/// </summary>
		/// <param name="seed">검색을 시작할 노드입니다.</param>
		/// <param name="depth">시드 노드로부터의 최대 노드 거리입니다.</param>
		/// <param name="tagMask">태그에 대한 선택적 마스크입니다. 이것은 비트마스크입니다.</param>
		/// <param name="filter">노드 검색을 위한 선택적 필터입니다. 이것을 depth = int.MaxValue 및 tagMask = -1과 함께 사용하여 필터가 모든 것을 결정하도록 할 수 있습니다.
		///      필터 함수가 false를 반환하면 노드가 갈 수 없는 것으로 처리됩니다.</param>
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

			// 이 함수의 이전 호출에서 예외가 발생하고 que와 map이 지워지지 않았을 수 있으므로 여기에서 지우는 것이 좋습니다.
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
		/// 원점 중심으로 중첩된 나선 형태의 포인트를 반환합니다. 다른 포인트와의 최소 간격을 유지합니다.
		/// 포인트는 원의 인볼루트(involutes)에 놓입니다.
		/// 자세한 내용은 다음을 참조하십시오: http://en.wikipedia.org/wiki/Involute
		/// 여기에 일부 좋은 특성이 있습니다.
		/// 모든 포인트는 간격 단위로 분리됩니다.
		/// 이 메서드는 O(n)입니다. 코드를 읽으면 이진 검색이 있지만 해당 이진 검색은 단계 수에 상한선이 있으므로 로그 요인을 생성하지 않습니다.
		/// 
		/// 참고: 사용 후 목록을 재활용하여 할당을 줄이는 것을 고려하십시오.
		/// 참조: Pathfinding.Util.ListPool
		/// </summary>
		public static List<Vector3> GetSpiralPoints (int count, float clearance) {
			List<Vector3> pts = ListPool<Vector3>.Claim(count);

			// 작은 원의 반지름 (나선형태의 원을 생성하는 데 사용됨)
			// 회전 사이의 분리 거리로부터 계산됩니다.
			float a = clearance/(2*Mathf.PI);
			float t = 0;


			pts.Add(InvoluteOfCircle(a, t));

			for (int i = 0; i < count; i++) {
				Vector3 prev = pts[pts.Count-1];

				// d = -t0/2 + sqrt( t0^2/4 + 2d/a )
				// 간격보다 큰 아크 거리를 생성할 최소 각도 (라디안)
				float d = -t / 2 + Mathf.Sqrt(t * t / 4 + 2 * clearance / a);

				// 이전 포인트와 이 포인트를 분리하기 위한 이진 검색
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
		/// 원의 인볼루트(involutes)의 XZ 좌표를 반환합니다.
		/// 자세한 내용은 다음을 참조하십시오: http://en.wikipedia.org/wiki/Involute
		/// </summary>
		private static Vector3 InvoluteOfCircle(float a, float t)
		{
			return new Vector3(a * (Mathf.Cos(t) + t * Mathf.Sin(t)), 0, a * (Mathf.Sin(t) - t * Mathf.Cos(t)));
		}

		/// <summary>
		/// p 주변의 그래프 위의 점들을 계산하며, 점들은 서로 clearance만큼 분리됩니다.
		/// 평균 점은 찾아지고 그것은 그룹 중심으로 처리됩니다.
		/// </summary>
		/// <param name="p">주변의 점을 생성할 지점</param>
		/// <param name="g">선형 검사에 사용할 그래프. 하나의 그래프만 사용하는 경우 AstarPath.active.graphs[0]를 IRaycastableGraph로 얻을 수 있습니다.
		/// 모든 그래프가 선형 검사 가능한 것은 아니며, recast, navmesh 및 grid 그래프가 선형 검사 가능합니다. recast와 navmesh에서 가장 잘 작동합니다.</param>
		/// <param name="previousPoints">참조로 사용할 점들 목록. 이러한 점들은 월드 공간에 있어야 합니다.
		///      새로운 점들은 목록 내의 기존 점들을 덮어씁니다. 결과는 월드 공간에 있습니다.</param>
		/// <param name="radius">최종 점들은 이 거리 이내에 위치합니다.</param>
		/// <param name="clearanceRadius">점들은 가능한 경우 서로 이 거리 이상 떨어져 있어야 합니다.</param>
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
		/// 중심 주변에 위치하고 서로 clearance만큼 분리되는 점들을 계산합니다.
		/// 중심에서 어떤 점까지의 최대 거리는 radius입니다.
		/// 먼저 previousPoints로 점을 배치하고, 실패하면 무작위 점이 선택됩니다.
		/// 그룹 이동을 위한 목표 점을 선택하려면 이상적입니다. 그룹의 평균 위치에서 현재 에이전트 점을 모두 전달하면
		/// 이 메서드는 그룹 내에서 에이전트가 거의 이동하지 않도록 목표 점을 반환합니다. 이는 시각적으로 매력적이며, 지역적 회피를 사용할 때 흔들림을 줄입니다.
		/// 
		/// TODO: 유닛 테스트 작성
		/// </summary>
		/// <param name="center">주변에 점을 생성할 지점</param>
		/// <param name="g">선형 검사에 사용할 그래프. 하나의 그래프만 사용하는 경우 AstarPath.active.graphs[0]를 IRaycastableGraph로 얻을 수 있습니다.
		/// 모든 그래프가 선형 검사 가능한 것은 아니며, recast, navmesh 및 grid 그래프가 선형 검사 가능합니다. recast와 navmesh에서 가장 잘 작동합니다.</param>
		/// <param name="previousPoints">참조로 사용할 점들 목록. 이러한 점들은 중심을 기준으로 상대적으로 처리되어야 합니다.
		///      새로운 점들은 목록 내의 기존 점들을 덮어씁니다. 결과는 중심을 기준으로 상대적이 아닌 월드 공간에 있습니다.</param>
		/// <param name="radius">최종 점들은 이 거리 이내에 위치합니다.</param>
		/// <param name="clearanceRadius">점들은 가능한 경우 서로 이 거리 이상 떨어져 있어야 합니다.</param>
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


			// 포장 밀도가 0.5인 원들을 포함하는 원의 반지름을 확보합니다.
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
							// 오류: 선형 검사가 완전히 실패
							// 많은 시도를 한 경우에만 계속합니다.
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

						// 8회 시도 이후나 유효한 점을 찾은 경우
						if (worked || tests > 8) {
							worked = true;
							previousPoints[i] = qt;
							break;
						}
					}

					// 중첩된 루프를 빠져나옵니다.
					if (worked) {
						break;
					}

					// 유효한 점을 찾을 수 없는 경우, 다음 번 시도를 위해 경계를 약간 축소하여 확률을 높입니다.
					clearanceRadius *= 0.9f;
					// 이것은 더 높은 확률로 원의 가장자리에 가까운 2D 포인트를 선택합니다.
					dir = Random.onUnitSphere * Mathf.Lerp(newMagn, radius, tests / 5);
					dir.y = 0;
					tests++;
				}
			}
		}

		/// <summary>
		/// 지정된 노드 위의 무작위 선택된 점을 반환하며, 각 점은 서로 clearanceRadius만큼 분리됩니다.
		/// 노드 위의 점을 선택하는 것은 TriangleMeshNode (Recast 그래프 및 Navmesh 그래프에서 사용) 및 GridNode (GridGraph에서 사용)에 대해서만 작동합니다.
		/// 다른 노드 유형의 경우, 노드의 위치만 사용됩니다.
		///
		/// 유효한 점을 찾을 수 없는 경우 clearanceRadius가 감소됩니다.
		///
		/// 참고: 이 메서드는 목록 내의 모든 노드가 특정한 경우에 대해서는 동일한 유형을 가정합니다.
		/// 구체적으로 첫 번째 노드가 TriangleMeshNode 또는 GridNode가 아닌 경우,
		/// 목록 내의 모든 노드가 동일한 표면 면적을 가진 것으로 가정하여 빠른 경로를 사용합니다.
		/// (일반적으로 표면 면적은 0이고 노드가 모두 PointNode인 경우입니다).
		/// </summary>
		public static List<Vector3> GetPointsOnNodes (List<GraphNode> nodes, int count, float clearanceRadius = 0) {
			if (nodes == null) throw new System.ArgumentNullException("nodes");
			if (nodes.Count == 0) throw new System.ArgumentException("no nodes passed");

			List<Vector3> pts = ListPool<Vector3>.Claim(count);

			// 사각형
			clearanceRadius *= clearanceRadius;

			if (clearanceRadius > 0 || nodes[0] is TriangleMeshNode
#if !ASTAR_NO_GRID_GRAPH
				|| nodes[0] is GridNode
#endif
				) {
				// 모든 노드의 누적 면적
				List<float> accs = ListPool<float>.Claim(nodes.Count);

				// 지금까지 모든 노드의 총 면적
				float tot = 0;

				for (int i = 0; i < nodes.Count; i++) {
					var surfaceArea = nodes[i].SurfaceArea();
					// 항상 랜덤한 노드를 선택하는 것이 아니라 표면 노드를 선택하기 위해 노드가 0인 경우에도 표면 면적이 존재하도록 합니다.
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


						// 유효한 점을 찾을 수 없는 경우, 점을 찾을 때까지 clearanceRadius를 점진적으로 낮춥니다.
						if (testCount >= testLimit)
						{
							// 주의: clearanceRadius는 제곱 반지름입니다.
							clearanceRadius *= 0.9f*0.9f;
							testLimit += 10;
							if (testLimit > 100) clearanceRadius = 0;
						}

						// 면적에 따라 가중치를 두고 목록 내의 노드 중 무작위 노드 선택
						float tg = Random.value*tot;
						int v = accs.BinarySearch(tg);
						if (v < 0) v = ~v;

						if (v >= nodes.Count) {
							// 예외 케이스 처리
							worked = false;
							continue;
						}

						var node = nodes[v];
						var p = node.RandomPointOnSurface();

						// 다른 점들로부터 일정 거리 떨어져 있는지 테스트
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
				// 빠른 경로, 모든 노드가 동일한 면적을 가짐 (보통 0)
				for (int i = 0; i < count; i++) {
					pts.Add((Vector3)nodes[Random.Range(0, nodes.Count)].RandomPointOnSurface());
				}
			}

			return pts;
		}
	}
}
