using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Serialization;
using System.Linq;
using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	/// <summary>
	/// 특정 경로 찾기 쿼리를 빠르게 수행하기 위한 계층적 그래프를 보유합니다.
	///
	/// 매우 빨라야 하는 일반적인 쿼리 유형 중 하나는 '이 노드가 다른 노드에서 도달 가능한가'입니다.
	/// 이는 예를 들어 경로의 끝 노드를 선택할 때 사용됩니다. 끝 노드는 시작 노드에서 도달할 수 있는 가장 가까운 노드로 결정됩니다.
	///
	/// 이 데이터 구조의 주요 목적은 각 노드가 포함된 연결된 구성 요소를 추적하여 이러한 쿼리를 빠르게 처리하는 것입니다.
	///
	/// 참조: https://en.wikipedia.org/wiki/Connected_component_(graph_theory)
	///
	/// 연결된 구성 요소는 해당 집합 내의 모든 노드 간에 유효한 경로가 있는 노드 집합입니다.
	/// 따라서 위의 쿼리는 단순히 두 노드가 동일한 연결된 구성 요소에 있는지 확인하여 답할 수 있습니다.
	/// 노드의 경우 연결된 구성 요소는 <see cref="Pathfinding.GraphNode.Area"/> 속성으로 노출되며 이 클래스에서는 <see cref="GetArea"/> 메서드를 사용하여 액세스합니다.
	///
	/// 아래 이미지에서 (200x200 그리드 그래프를 표시) 각 연결된 구성 요소는 별도의 색상으로 표시됩니다.
	/// 실제 색상은 특별한 의미가 있는 것은 아니며 서로 다름만 나타냅니다.
	/// [온라인 문서에서 이미지 보기]
	///
	/// 버전 4.2 이전에는 연결된 구성 요소가 각 노드에 저장된 숫자만 있었으며 그래프가 업데이트될 때마다 연결된 구성 요소가 완전히 다시 계산되었습니다.
	/// 이를 수행하는 데 상당히 비싼 큰 그래프에서 비용이 많이 들 수 있지만, 그래도 많은 수의 노드를 통과해야 합니다.
	///
	/// 이 클래스는 실제 그래프와 동일한 연결성을 유지하면서 더 작은 그래프를 구축합니다.
	/// 이 계층적 그래프의 각 노드는 하나의 연결된 구성 요소를 나타내며 아래 이미지에서 예제를 확인하세요.
	/// 이미지에서 각 색상은 별도의 계층적 노드이며 검은색 연결은 각 계층적 노드의 중심 사이를 이동합니다.
	///
	/// [온라인 문서에서 이미지 보기]
	///
	/// 계층적 그래프를 사용하면 연결된 구성 요소를 실제 그래프 대신 계층적 그래프에서 침수로 계산하여 연결된 구성 요소를 확인할 수 있습니다.
	/// 그런 다음 노드가 속한 계층적 노드의 연결된 구성 요소를 확인하려면 해당 계층적 노드의 연결된 구성 요소를 찾습니다.
	///
	/// 이러한 이점이 즉시 명확하지 않을 수 있습니다. 위에서 설명한 것은 동일한 것을 수행하는 더 복잡한 방법일 뿐입니다.
	/// 그러나 실제 이점은 그래프를 업데이트할 때 나타납니다.
	/// 그래프가 업데이트되면 업데이트에 영향을 받는 모든 노드를 포함하는 계층적 노드가 완전히 제거되고 모든 것이 제거된 후에 그 자리에 새로운 계층적 노드가 다시 계산됩니다.
	/// 이 작업이 완료되면 전체 그래프의 연결된 구성 요소를 계층적 그래프만 침수하여 업데이트할 수 있습니다.
	/// 계층적 그래프는 실제 그래프보다 훨씬 작으므로 이것이 훨씬 빠릅니다.
	///
	/// 따라서 모든 이러한 최적화를 사용하면 그래프가 업데이트될 때 그래프의 연결된 구성 요소를 매우 빠르게 다시 계산할 수 있습니다.
	/// 이 효과는 그래프가 클수록, 그래프 업데이트가 작을수록 커지며, 전체 그래프를 스캔하거나 전체 그래프를 업데이트하는 경우 속도 향상이 없습니다.
	/// 실제로 추가 복잡성 때문에 약간 느리게 작동하지만 프로파일링 후 추가 시간은 그래프를 스캔하는 나머지 비용과 비교해 대부분 무시할 정도로 미미한 것으로 나타납니다.
	///
	/// [온라인 문서에서 비디오 보기]
	///
	/// 참조: <see cref="Pathfinding.PathUtilities.IsPathPossible"/>
	/// 참조: <see cref="Pathfinding.NNConstraint"/>
	/// 참조: <see cref="Pathfinding.GraphNode.Area"/>
	/// </summary>
	public class HierarchicalGraph {
		const int Tiling = 16;
		const int MaxChildrenPerNode = Tiling * Tiling;
		const int MinChildrenPerNode = MaxChildrenPerNode/2;

		List<GraphNode>[] children = new List<GraphNode>[0];
		List<int>[] connections = new List<int>[0];
		int[] areas = new int[0];
		byte[] dirty = new byte[0];

		public int version { get; private set; }
		public System.Action onConnectedComponentsChanged;

		System.Action<GraphNode> connectionCallback;

		Queue<GraphNode> temporaryQueue = new Queue<GraphNode>();
		List<GraphNode> currentChildren = null;
		List<int> currentConnections = null;
		int currentHierarchicalNodeIndex;
		Stack<int> temporaryStack = new Stack<int>();

		int numDirtyNodes = 0;
		GraphNode[] dirtyNodes = new GraphNode[128];

		Stack<int> freeNodeIndices = new Stack<int>();

		int gizmoVersion = 0;

		public HierarchicalGraph () {
			// 이 콜백을 캐시하여 FindHierarchicalNodeChildren 메서드가 호출될 때마다 새로 할당하는 것을 피합니다.
			// 그 메서드에서 상태 정보에 대한 멤버 변수를 사용해야 하는 것은 큰 불편하지만 더 나은 방법이 없습니다.
			connectionCallback = (GraphNode neighbour) => {
				var hIndex = neighbour.HierarchicalNodeIndex;
				if (hIndex == 0) {
					if (currentChildren.Count < MaxChildrenPerNode && neighbour.Walkable /* && (((GridNode)currentChildren[0]).XCoordinateInGrid/Tiling == ((GridNode)neighbour).XCoordinateInGrid/Tiling) && (((GridNode)currentChildren[0]).ZCoordinateInGrid/Tiling == ((GridNode)neighbour).ZCoordinateInGrid/Tiling)*/)
					{
						neighbour.HierarchicalNodeIndex = currentHierarchicalNodeIndex;
						temporaryQueue.Enqueue(neighbour);
						currentChildren.Add(neighbour);
					}
				} else if (hIndex != currentHierarchicalNodeIndex && !currentConnections.Contains(hIndex)) {
					// 이론적으로 Contains 호출은 매우 느릴 수 있으므로
					// 계층적 노드가 임의 개수의 노드에 인접할 수 있습니다.
					// 그러나 실제로는 노드가 구성될 방식 때문에 약간 작은 (≈4-6) 수의 다른 노드에만 인접할 것입니다.
					// 따라서 Contains 호출은 Set 조회보다 훨씬 빠릅니다.
					currentConnections.Add(hIndex);
				}
			};

			Grow();
		}

		void Grow () {
			var newChildren = new List<GraphNode>[System.Math.Max(64, children.Length*2)];
			var newConnections = new List<int>[newChildren.Length];
			var newAreas = new int[newChildren.Length];
			var newDirty = new byte[newChildren.Length];

			children.CopyTo(newChildren, 0);
			connections.CopyTo(newConnections, 0);
			areas.CopyTo(newAreas, 0);
			dirty.CopyTo(newDirty, 0);

			for (int i = children.Length; i < newChildren.Length; i++) {
				newChildren[i] = ListPool<GraphNode>.Claim(MaxChildrenPerNode);
				newConnections[i] = new List<int>();
				if (i > 0) freeNodeIndices.Push(i);
			}

			children = newChildren;
			connections = newConnections;
			areas = newAreas;
			dirty = newDirty;
		}

		int GetHierarchicalNodeIndex () {
			if (freeNodeIndices.Count == 0) Grow();
			return freeNodeIndices.Pop();
		}

		internal void OnCreatedNode (GraphNode node) {
			if (node.NodeIndex >= dirtyNodes.Length) {
				var newDirty = new GraphNode[System.Math.Max(node.NodeIndex + 1, dirtyNodes.Length*2)];
				dirtyNodes.CopyTo(newDirty, 0);
				dirtyNodes = newDirty;
			}
			AddDirtyNode(node);
		}

		internal void AddDirtyNode (GraphNode node) {
			if (!node.IsHierarchicalNodeDirty) {
				node.IsHierarchicalNodeDirty = true;
				// dirtyNodes 배열은 그래프의 모든 노드를 저장하는 데 충분히 크다고 보장되지만
				// 배열은 때때로 많은 파괴된 노드를 포함할 수도 있습니다. 드물게 배열이 범위를 벗어나는 원인이 될 수 있습니다.
				// 그런 경우에는 배열을 통과하여 파괴된 노드를 걸러내고 해당 노드의 hierarchical 노드를 표시하는 것이 필요합니다.
				if (numDirtyNodes < dirtyNodes.Length)
				{
					dirtyNodes[numDirtyNodes] = node;
					numDirtyNodes++;
				} else {
					int maxIndex = 0;
					for (int i = numDirtyNodes - 1; i >= 0; i--) {
						if (dirtyNodes[i].Destroyed) {
							numDirtyNodes--;
							dirty[dirtyNodes[i].HierarchicalNodeIndex] = 1;
							dirtyNodes[i] = dirtyNodes[numDirtyNodes];
							dirtyNodes[numDirtyNodes] = null;
						} else {
							maxIndex = System.Math.Max(maxIndex, dirtyNodes[i].NodeIndex);
						}
					}
					if (numDirtyNodes >= dirtyNodes.Length) throw new System.Exception("Failed to compactify dirty nodes array. This should not happen. " + maxIndex + " " + numDirtyNodes + " " + dirtyNodes.Length);
					AddDirtyNode(node);
				}
			}
		}

		public int NumConnectedComponents { get; private set; }

		/// <summary>계층적 노드의 연결된 구성 요소 인덱스를 가져옵니다.</summary>
		public uint GetConnectedComponent(int hierarchicalNodeIndex)
		{
			return (uint)areas[hierarchicalNodeIndex];
		}

		void RemoveHierarchicalNode (int hierarchicalNode, bool removeAdjacentSmallNodes) {
			freeNodeIndices.Push(hierarchicalNode);
			var conns = connections[hierarchicalNode];

			for (int i = 0; i < conns.Count; i++) {
				var adjacentHierarchicalNode = conns[i];
				// Dirty한 경우 이 노드는 나중에 제거될 것이므로 아무런 조치도 취하지 않습니다.
				if (dirty[adjacentHierarchicalNode] != 0) continue;

				if (removeAdjacentSmallNodes && children[adjacentHierarchicalNode].Count < MinChildrenPerNode) {
					dirty[adjacentHierarchicalNode] = 2;
					RemoveHierarchicalNode(adjacentHierarchicalNode, false);
				} else {
					// 이 노드를 제거하므로 다른 노드에서 이 노드로의 연결을 제거합니다.
					connections[adjacentHierarchicalNode].Remove(hierarchicalNode);
				}
			}
			conns.Clear();

			var nodeChildren = children[hierarchicalNode];

			for (int i = 0; i < nodeChildren.Count; i++) {
				AddDirtyNode(nodeChildren[i]);
			}

			nodeChildren.ClearFast();
		}

		/// <summary>노드가 표시된 경우 계층적 그래프와 연결된 구성 요소를 필요한 경우 다시 계산합니다.</summary>
		public void RecalculateIfNecessary()
		{
			if (numDirtyNodes > 0) {
				Profiler.BeginSample("Recalculate Connected Components");
				for (int i = 0; i < numDirtyNodes; i++) {
					dirty[dirtyNodes[i].HierarchicalNodeIndex] = 1;
				}

				// 모든 계층적 노드를 제거한 다음 새로운 계층적 노드를 그 자리에 빌드합니다
				// 새로운 그래프 데이터를 고려합니다.
				for (int i = 1; i < dirty.Length; i++) {
					if (dirty[i] == 1) RemoveHierarchicalNode(i, true);
				}
				for (int i = 1; i < dirty.Length; i++) dirty[i] = 0;

				for (int i = 0; i < numDirtyNodes; i++) {
					dirtyNodes[i].HierarchicalNodeIndex = 0;
				}

				for (int i = 0; i < numDirtyNodes; i++) {
					var node = dirtyNodes[i];
					// GC에 대해 좋습니다
					dirtyNodes[i] = null;
					node.IsHierarchicalNodeDirty = false;

					if (node.HierarchicalNodeIndex == 0 && node.Walkable && !node.Destroyed) {
						FindHierarchicalNodeChildren(GetHierarchicalNodeIndex(), node);
					}
				}

				numDirtyNodes = 0;
				// 계층적 노드의 연결된 구성 요소 다시 계산
				FloodFill();
				Profiler.EndSample();
				gizmoVersion++;
			}
		}

		/// <summary>
		/// 모든 것을 처음부터 다시 계산합니다.
		/// 주로 호환성을 위해 레거시 코드에 사용해야 하며 새 코드에는 사용하지 않아야 합니다.
		///
		/// 참조: <see cref="RecalculateIfNecessary"/>
		/// </summary>
		public void RecalculateAll () {
			AstarPath.active.data.GetNodes(node => AddDirtyNode(node));
			RecalculateIfNecessary();
		}

		/// <summary>계층적 노드 그래프를 침수하고 동일한 연결된 구성 요소에 속하는 계층적 노드에 동일한 영역 ID를 할당합니다.</summary>
		void FloodFill()
		{
			for (int i = 0; i < areas.Length; i++) areas[i] = 0;

			Stack<int> stack = temporaryStack;
			int currentArea = 0;
			for (int i = 1; i < areas.Length; i++) {
				// Already taken care of
				if (areas[i] != 0) continue;

				currentArea++;
				areas[i] = currentArea;
				stack.Push(i);
				while (stack.Count > 0) {
					int node = stack.Pop();
					var conns = connections[node];
					for (int j = conns.Count - 1; j >= 0; j--) {
						var otherNode = conns[j];
						// 참고: 연결이 != currentArea가 되도록 해야하며 != 0이 아니어야 합니다.
						// 그래프에 연결되었지만 연결이 강하지 않은 구성 요소가 (이것은 매우 드문 종류의 게임에서만 발생할 것입니다) 아니라면 발생합니다.
						if (areas[otherNode] != currentArea)
						{
							areas[otherNode] = currentArea;
							stack.Push(otherNode);
						}
					}
				}
			}

			NumConnectedComponents = System.Math.Max(1, currentArea + 1);
			version++;
		}

		/// <summary>시작 노드에서 BFS를 실행하고 다른 계층적 노드에 이미 할당되지 않은 MaxChildrenPerNode 노드를 해당 계층적 노드에 할당합니다.</summary>
		void FindHierarchicalNodeChildren(int hierarchicalNode, GraphNode startNode)
		{
			// connectionCallback 델리게이트가 사용할 상태를 설정합니다.
			currentChildren = children[hierarchicalNode];
			currentConnections = connections[hierarchicalNode];
			currentHierarchicalNodeIndex = hierarchicalNode;

			var que = temporaryQueue;
			que.Enqueue(startNode);

			startNode.HierarchicalNodeIndex = hierarchicalNode;
			currentChildren.Add(startNode);

			while (que.Count > 0) {
				que.Dequeue().GetConnections(connectionCallback);
			}

			for (int i = 0; i < currentConnections.Count; i++) {
				connections[currentConnections[i]].Add(hierarchicalNode);
			}

			que.Clear();
		}

		public void OnDrawGizmos (Pathfinding.Util.RetainedGizmos gizmos) {
			var hasher = new Pathfinding.Util.RetainedGizmos.Hasher(AstarPath.active);

			hasher.AddHash(gizmoVersion);

			if (!gizmos.Draw(hasher)) {
				var builder = ObjectPool<RetainedGizmos.Builder>.Claim();
				var centers = ArrayPool<UnityEngine.Vector3>.Claim(areas.Length);
				for (int i = 0; i < areas.Length; i++) {
					Int3 center = Int3.zero;
					var childs = children[i];
					if (childs.Count > 0) {
						for (int j = 0; j < childs.Count; j++) center += childs[j].position;
						center /= childs.Count;
						centers[i] = (UnityEngine.Vector3)center;
					}
				}

				for (int i = 0; i < areas.Length; i++) {
					if (children[i].Count > 0) {
						for (int j = 0; j < connections[i].Count; j++) {
							if (connections[i][j] > i) {
								builder.DrawLine(centers[i], centers[connections[i][j]], UnityEngine.Color.black);
							}
						}
					}
				}

				builder.Submit(gizmos, hasher);
			}
		}
	}
}
