#define DECREASE_KEY
using System.Collections.Generic;

namespace Pathfinding {
	/// <summary>
	/// ���� ��� ã�� ��û�� ���� �ӽ� ��� �����͸� �����մϴ�.
	/// �� ��忡�� �ϳ��� PathNode�� ������, ������ �����忡�� ���˴ϴ�.
	/// �̴� ��� ��꿡 �ʿ������� �׷��� ������ �Ϻΰ� �ƴ� G ����, H ���� �� ��Ÿ �ӽ� ������ �����մϴ�.
	///
	/// ����: Pathfinding.PathHandler
	/// ����: https://en.wikipedia.org/wiki/A*_search_algorithm
	/// </summary>
	public class PathNode
	{
		/// <summary>���� �׷��� ��忡 ���� ����</summary>
		public GraphNode node;

		/// <summary>Ž�� Ʈ������ �θ� ���</summary>
		public PathNode parent;

		/// <summary>�� ��带 ���������� ����� ��� ��û(�� �����忡�� ���Ǵ� ���)�� ��� ID</summary>
		public ushort pathID;


#if DECREASE_KEY
		/// <summary>
		/// ���� ������ ����� �ε����Դϴ�.
		/// A* �˰����� ���� ���(open list)�� ���� ������ �����˴ϴ�.
		/// ���� 'Ű ����(decrease key)' �۾��� �����ϱ� ���� ����� �ε����� ���⿡ ����˴ϴ�.
		/// </summary>
		public ushort heapIndex = BinaryHeap.NotInHeap;
#endif

		/// <summary>���� �ʵ带 �����ϴ� ��Ʈ ��ŷ ����</summary>
		private uint flags;

		/// <summary>����� ù 28 ��Ʈ�� ����մϴ�</summary>
		private const uint CostMask = (1U << 28) - 1U;

		/// <summary>�÷��� 1�� 28�� ��Ʈ�� �ֽ��ϴ�</summary>
		private const int Flag1Offset = 28;
		private const uint Flag1Mask = (uint)(1 << Flag1Offset);

		/// <summary>�÷��� 2�� 29�� ��Ʈ�� �ֽ��ϴ�</summary>
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
		/// ��� ã�� ������ ��Ÿ���� �ӽ� �÷���.
		/// ��� ã�� �߿��� Pathfinders�� ��带 ǥ���ϴ� �� ����� �� �ֽ��ϴ�. �Ϸ�Ǹ� �� �÷��״� �⺻ ����(����)�� �ǵ������� �ϸ�
		/// �ٸ� ��� ã�� ��û�� ��ġ�� �ʵ��� �ؾ� �մϴ�.
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
		/// ��� ã�� ������ ��Ÿ���� �ӽ� �÷���.
		/// ��� ã�� �߿��� Pathfinders�� ��带 ǥ���ϴ� �� ����� �� �ֽ��ϴ�. �Ϸ�Ǹ� �� �÷��״� �⺻ ����(����)�� �ǵ������� �ϸ�
		/// �ٸ� ��� ã�� ��û�� ��ġ�� �ʵ��� �ؾ� �մϴ�.
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

		/// <summary>G ������ ��� �ʵ�</summary>
		private uint g;

		/// <summary>H ������ ��� �ʵ�</summary>
		private uint h;

		/// <summary>G ����, �� ��忡 �����ϱ� ���� ���</summary>
		public uint G { get { return g; } set { g = value; } }

		/// <summary>H ����, ��ǥ������ �����ϴ� ���� ���</summary>
		public uint H { get { return h; } set { h = value; } }

		/// <summary>F ����. H ���� + G ����</summary>
		public uint F { get { return g + h; } }


		public void UpdateG (Path path) {
#if ASTAR_NO_TRAVERSAL_COST
			g = parent.g + cost;
#else
			g = parent.g + cost + path.GetTraversalCost(node);
#endif
		}
	}

	/// <summary>�����庰 ��� �����͸� ó���մϴ�.</summary>
	public class PathHandler
	{
		/// <summary>
		/// ���� PathID.
		/// ����: <see cref="PathID"/>
		/// </summary>
		private ushort pathID;

		public readonly int threadID;
		public readonly int totalThreadCount;

		/// <summary>
		/// "���� ���(Open list)"�� ��带 �����ϴ� ���� ��(Binary Heap).
		/// ����: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		public readonly BinaryHeap heap = new BinaryHeap(128);

		/// <summary>���� ��� ���� ��� �Ǵ� ���������� ���� ����� ID</summary>
		public ushort PathID { get { return pathID; } }

		/// <summary>��� PathNode�� �迭</summary>
		public PathNode[] nodes = new PathNode[0];

		/// <summary>
		/// ����� ���ڿ��� �ۼ��ϴ� �� ����� �� �ִ� StringBuilder.
		/// �� ��ΰ� ��ü StringBuilder�� �����ϴ� ��� ���� StringBuilder�� ����ϸ� ���� �� �޸� ����� �� ȿ�����Դϴ�.
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

		/// <summary>��� ������ ������ ���� ���� �޼���</summary>
		public void DestroyNode(GraphNode node)
		{
			PathNode pn = GetPathNode(node);

			// GC�� �����ϱ� ���� ���� ����
			pn.node = null;
			pn.parent = null;
			// �̰��� ��� ã�⿡ �ʿ����� ������ NULL�� ������ ������ 'Show Search Tree'�� Ȱ��ȭ�Ǿ����� ��
			// (Ư�� 'Show Search Tree'�� Ȱ��ȭ�Ǿ����� ��) ��� �� �������� �Ͻ������� ȥ�������� �� �� �ֽ��ϴ�.
			// �׷��� ����뿡 ������ �ݴϴ�
			pn.pathID = 0;
			pn.G = 0;
			pn.H = 0;
		}

		/// <summary>��� ������ �ʱ�ȭ�� ���� ���� �޼���</summary>
		public void InitializeNode(GraphNode node)
		{
			// ����� �ε��� ��������
			int ind = node.NodeIndex;

			if (ind >= nodes.Length)
			{
				// ũ�⸦ 2��� Ȯ��
				PathNode[] newNodes = new PathNode[System.Math.Max(128, nodes.Length * 2)];
				nodes.CopyTo(newNodes, 0);
				// ��� PathNode �ν��Ͻ��� �Ѳ����� �ʱ�ȭ�մϴ�.
				// �̸� ���⿡�� �����ϰ� �������� �ʱ�ȭ���� �ʴ� ���� �߿��մϴ�. ���� ��� NULL�� �ΰ� �������� �ʱ�ȭ���� �ʴ� ���
				// ��� ��带 ���� ����� ��ġ�� �Ҵ��ϵ��� �� �� �ֽ��ϴ�(��κ��� �ý����� � ������ bumb-allocator�� ����մϴ�).
				// �̷��� �ϸ� ĳ�� ���μ��� �����ϰ� false sharing(���� ������ ���� �����ϴ� ��)�� ���̴� �� ������ �˴ϴ�
				// (���� �����忡 ���� ��� ��带 �����̿� �Ҵ��ϴ� ��� false sharing�� �߻��մϴ�). 
				// �̷� ���� ��ü ��� ã�� ���ɿ� �� 4% ������ ���̰� �ֽ��ϴ�.
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
		/// ������ ��忡 �ش��ϴ� PathNode�� ��ȯ�մϴ�.
		/// PathNode�� ���� PathHandler���� ���ǹǷ� ��Ƽ�������� Ȱ��ȭ�� ����Դϴ�.
		/// </summary>
		public PathNode GetPathNode(GraphNode node)
		{
			return nodes[node.NodeIndex];
		}

		/// <summary>
		/// ��� ����� pathID�� 0���� �����մϴ�.
		/// ����: Pathfinding.PathNode.pathID
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
