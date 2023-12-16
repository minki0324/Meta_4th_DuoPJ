#define DECREASE_KEY
using System.Collections.Generic;

namespace Pathfinding {
	/// <summary>
	/// 단일 경로 찾기 요청에 대한 임시 노드 데이터를 저장합니다.
	/// 각 노드에는 하나의 PathNode가 있으며, 각각의 스레드에서 사용됩니다.
	/// 이는 경로 계산에 필요하지만 그래프 구조의 일부가 아닌 G 점수, H 점수 및 기타 임시 변수를 저장합니다.
	///
	/// 참조: Pathfinding.PathHandler
	/// 참조: https://en.wikipedia.org/wiki/A*_search_algorithm
	/// </summary>
	public class PathNode
	{
		/// <summary>실제 그래프 노드에 대한 참조</summary>
		public GraphNode node;

		/// <summary>탐색 트리에서 부모 노드</summary>
		public PathNode parent;

		/// <summary>이 노드를 마지막으로 사용한 경로 요청(이 스레드에서 사용되는 경우)의 경로 ID</summary>
		public ushort pathID;


#if DECREASE_KEY
		/// <summary>
		/// 이진 힙에서 노드의 인덱스입니다.
		/// A* 알고리즘의 열린 목록(open list)은 이진 힙으로 구현됩니다.
		/// 빠른 '키 감소(decrease key)' 작업을 지원하기 위해 노드의 인덱스가 여기에 저장됩니다.
		/// </summary>
		public ushort heapIndex = BinaryHeap.NotInHeap;
#endif

		/// <summary>여러 필드를 저장하는 비트 팩킹 변수</summary>
		private uint flags;

		/// <summary>비용은 첫 28 비트를 사용합니다</summary>
		private const uint CostMask = (1U << 28) - 1U;

		/// <summary>플래그 1은 28번 비트에 있습니다</summary>
		private const int Flag1Offset = 28;
		private const uint Flag1Mask = (uint)(1 << Flag1Offset);

		/// <summary>플래그 2는 29번 비트에 있습니다</summary>
		private const int Flag2Offset = 29;
		private const uint Flag2Mask = (uint)(1 << Flag2Offset);

		public uint cost
		{
			get
			{
				return flags & CostMask;
			}
			set
			{
				flags = (flags & ~CostMask) | value;
			}
		}

		/// <summary>
		/// 경로 찾기 중임을 나타내는 임시 플래그.
		/// 경로 찾기 중에만 Pathfinders가 노드를 표시하는 데 사용할 수 있습니다. 완료되면 이 플래그는 기본 상태(거짓)로 되돌려져야 하며
		/// 다른 경로 찾기 요청을 망치지 않도록 해야 합니다.
		/// </summary>
		public bool flag1
		{
			get
			{
				return (flags & Flag1Mask) != 0;
			}
			set
			{
				flags = (flags & ~Flag1Mask) | (value ? Flag1Mask : 0U);
			}
		}

		/// <summary>
		/// 경로 찾기 중임을 나타내는 임시 플래그.
		/// 경로 찾기 중에만 Pathfinders가 노드를 표시하는 데 사용할 수 있습니다. 완료되면 이 플래그는 기본 상태(거짓)로 되돌려져야 하며
		/// 다른 경로 찾기 요청을 망치지 않도록 해야 합니다.
		/// </summary>
		public bool flag2
		{
			get
			{
				return (flags & Flag2Mask) != 0;
			}
			set
			{
				flags = (flags & ~Flag2Mask) | (value ? Flag2Mask : 0U);
			}
		}

		/// <summary>G 점수의 백업 필드</summary>
		private uint g;

		/// <summary>H 점수의 백업 필드</summary>
		private uint h;

		/// <summary>G 점수, 이 노드에 도달하기 위한 비용</summary>
		public uint G { get { return g; } set { g = value; } }

		/// <summary>H 점수, 목표지까지 도달하는 예상 비용</summary>
		public uint H { get { return h; } set { h = value; } }

		/// <summary>F 점수. H 점수 + G 점수</summary>
		public uint F { get { return g + h; } }


		public void UpdateG (Path path) {
#if ASTAR_NO_TRAVERSAL_COST
			g = parent.g + cost;
#else
			g = parent.g + cost + path.GetTraversalCost(node);
#endif
		}
	}

	/// <summary>스레드별 경로 데이터를 처리합니다.</summary>
	public class PathHandler
	{
		/// <summary>
		/// 현재 PathID.
		/// 참조: <see cref="PathID"/>
		/// </summary>
		private ushort pathID;

		public readonly int threadID;
		public readonly int totalThreadCount;

		/// <summary>
		/// "열린 목록(Open list)"의 노드를 추적하는 이진 힙(Binary Heap).
		/// 참조: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		public readonly BinaryHeap heap = new BinaryHeap(128);

		/// <summary>현재 계산 중인 경로 또는 마지막으로 계산된 경로의 ID</summary>
		public ushort PathID { get { return pathID; } }

		/// <summary>모든 PathNode의 배열</summary>
		public PathNode[] nodes = new PathNode[0];

		/// <summary>
		/// 디버그 문자열을 작성하는 데 사용할 수 있는 StringBuilder.
		/// 각 경로가 자체 StringBuilder를 생성하는 대신 단일 StringBuilder를 사용하면 성능 및 메모리 사용이 더 효율적입니다.
		/// </summary>
		public readonly System.Text.StringBuilder DebugStringBuilder = new System.Text.StringBuilder();

		public PathHandler(int threadID, int totalThreadCount)
		{
			this.threadID = threadID;
			this.totalThreadCount = totalThreadCount;
		}

		public void InitializeForPath(Path p)
		{
			pathID = p.pathID;
			heap.Clear();
		}

		/// <summary>노드 데이터 정리를 위한 내부 메서드</summary>
		public void DestroyNode(GraphNode node)
		{
			PathNode pn = GetPathNode(node);

			// GC를 지원하기 위해 참조 정리
			pn.node = null;
			pn.parent = null;
			// 이것은 경로 찾기에 필요하지 않지만 NULL로 남기지 않으면 'Show Search Tree'가 활성화되어있을 때
			// (특히 'Show Search Tree'가 활성화되어있을 때) 기시 모 렌더링을 일시적으로 혼란스럽게 할 수 있습니다.
			// 그래픽 디버깅에 영향을 줍니다
			pn.pathID = 0;
			pn.G = 0;
			pn.H = 0;
		}

		/// <summary>노드 데이터 초기화를 위한 내부 메서드</summary>
		public void InitializeNode(GraphNode node)
		{
			// 노드의 인덱스 가져오기
			int ind = node.NodeIndex;

			if (ind >= nodes.Length)
			{
				// 크기를 2배로 확장
				PathNode[] newNodes = new PathNode[System.Math.Max(128, nodes.Length * 2)];
				nodes.CopyTo(newNodes, 0);
				// 모든 PathNode 인스턴스를 한꺼번에 초기화합니다.
				// 이를 여기에서 수행하고 게으르게 초기화하지 않는 것이 중요합니다. 예를 들어 NULL로 두고 게으르게 초기화하지 않는 경우
				// 경로 노드를 서로 가까운 위치에 할당하도록 할 수 있습니다(대부분의 시스템은 어떤 종류의 bumb-allocator를 사용합니다).
				// 이렇게 하면 캐시 국부성을 개선하고 false sharing(여러 스레드 간에 경쟁하는 것)을 줄이는 데 도움이 됩니다
				// (여러 스레드에 대해 경로 노드를 가까이에 할당하는 경우 false sharing이 발생합니다). 
				// 이로 인해 전체 경로 찾기 성능에 약 4% 정도의 차이가 있습니다.
				for (int i = nodes.Length; i < newNodes.Length; i++) newNodes[i] = new PathNode();
				nodes = newNodes;
			}

			nodes[ind].node = node;
		}

		public PathNode GetPathNode(int nodeIndex)
		{
			return nodes[nodeIndex];
		}

		/// <summary>
		/// 지정된 노드에 해당하는 PathNode를 반환합니다.
		/// PathNode는 여러 PathHandler에서 사용되므로 멀티스레딩이 활성화된 경우입니다.
		/// </summary>
		public PathNode GetPathNode(GraphNode node)
		{
			return nodes[node.NodeIndex];
		}

		/// <summary>
		/// 모든 노드의 pathID를 0으로 설정합니다.
		/// 참조: Pathfinding.PathNode.pathID
		/// </summary>
		public void ClearPathIDs()
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i] != null) nodes[i].pathID = 0;
			}
		}
	}
}
