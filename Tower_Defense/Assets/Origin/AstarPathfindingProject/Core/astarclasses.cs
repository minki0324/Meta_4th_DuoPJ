using UnityEngine;
using System.Collections.Generic;


// RVO ���ӽ����̽��� Ŭ������ ���� ���� �������� ������ �����ϱ� ���� �� ���ӽ����̽� ����
namespace Pathfinding.RVO {}

namespace Pathfinding {
	using Pathfinding.Util;

#if UNITY_5_0
	/// <summary>Unity 5.0���� ���Ǹ�, HelpURLAttribute�� Unity 5.1���� ó�� �߰��Ǿ����ϴ�.</summary>
	public class HelpURLAttribute : Attribute {
	}
#endif

	[System.Serializable]
	/// <summary>������ ������ �����մϴ�.</summary>
	public class AstarColor {
		public Color _SolidColor; // �ָ��� �÷�
		public Color _UnwalkableNode; // �ȱ� �Ұ����� ��� �÷�
		public Color _BoundsHandles; // ��� �ڵ� �÷�

		public Color _ConnectionLowLerp; // ���� �ο� ���� �÷�
		public Color _ConnectionHighLerp; // ���� ���� ���� �÷�

		public Color _MeshEdgeColor; // �޽� ���� �÷�

		/// <summary>
		/// ����ڰ� ������ ���� ������ �����մϴ�.
		/// ���� ������ �������� GetAreaColor�� ����ϼ���.
		/// </summary>
		public Color[] _AreaColors;

		public static Color SolidColor = new Color(30 / 255f, 102 / 255f, 201 / 255f, 0.9F); // �ָ��� �÷� �⺻��
		public static Color UnwalkableNode = new Color(1, 0, 0, 0.5F); // �ȱ� �Ұ����� ��� �÷� �⺻��
		public static Color BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F); // ��� �ڵ� �÷� �⺻��

		public static Color ConnectionLowLerp = new Color(0, 1, 0, 0.5F); // ���� �ο� ���� �÷� �⺻��
		public static Color ConnectionHighLerp = new Color(1, 0, 0, 0.5F); // ���� ���� ���� �÷� �⺻��

		public static Color MeshEdgeColor = new Color(0, 0, 0, 0.5F); // �޽� ���� �÷� �⺻��


		private static Color[] AreaColors = new Color[1];

		public static int ColorHash () {
			var hash = SolidColor.GetHashCode() ^ UnwalkableNode.GetHashCode() ^ BoundsHandles.GetHashCode() ^ ConnectionLowLerp.GetHashCode() ^ ConnectionHighLerp.GetHashCode() ^ MeshEdgeColor.GetHashCode();

			for (int i = 0; i < AreaColors.Length; i++) hash = 7*hash ^ AreaColors[i].GetHashCode();
			return hash;
		}

		/// <summary>
		/// ������ ���� ������ ��ȯ�մϴ�. ����ڰ� ������ ����� ���� ������ ��� ����մϴ�.
		/// ����ڰ� ������ ���� ������ ������ ��� �ش� ������ ����ϰ�, �׷��� ���� ��� AstarMath.IntToColor�� ����Ͽ� ������ ����մϴ�.
		/// </summary>
		public static Color GetAreaColor (uint area) {
			if (area >= AreaColors.Length) return AstarMath.IntToColor((int)area, 1F);
			return AreaColors[(int)area];
		}

		/// <summary>
		/// �±׿� ���� ������ ��ȯ�մϴ�. ����ڰ� ������ ����� ���� ������ ��� ����մϴ�.
		/// ����ڰ� �±׿� ���� ������ ������ ��� �ش� ������ ����ϰ�, �׷��� ���� ��� AstarMath.IntToColor�� ����Ͽ� ������ ����մϴ�.
		/// </summary>
		public static Color GetTagColor (uint tag) {
			if (tag >= AreaColors.Length) return AstarMath.IntToColor((int)tag, 1F);
			return AreaColors[(int)tag];
		}

		/// <summary>
		/// ��� ���� ������ ���� ������ �о�ֽ��ϴ�.
		/// �̰��� Gizmo ������ �߿� ���� ���� �׼����ϱ� ������ ����˴ϴ�.
		/// ���� ���ɿ� �������� ������ ��Ĩ�ϴ� (Gizmo �������� �� �ڵ��Դϴ�).
		/// �ణ ������ ��������� �ⲯ�ؾ��ϴ� ���Դϴ�.
		/// </summary>
		public void PushToStatic (AstarPath astar) {
			_AreaColors  = _AreaColors ?? new Color[1];

			SolidColor = _SolidColor;
			UnwalkableNode = _UnwalkableNode;
			BoundsHandles = _BoundsHandles;
			ConnectionLowLerp = _ConnectionLowLerp;
			ConnectionHighLerp = _ConnectionHighLerp;
			MeshEdgeColor = _MeshEdgeColor;
			AreaColors = _AreaColors;
		}

		public AstarColor () {
			// Set default colors
			_SolidColor = new Color(30/255f, 102/255f, 201/255f, 0.9F);
			_UnwalkableNode = new Color(1, 0, 0, 0.5F);
			_BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F);
			_ConnectionLowLerp = new Color(0, 1, 0, 0.5F);
			_ConnectionHighLerp = new Color(1, 0, 0, 0.5F);
			_MeshEdgeColor = new Color(0, 0, 0, 0.5F);
		}
	}


	/// <summary>
	/// �׷��� ���� �Ǵ� ���� ĳ��Ʈ���� ��ȯ�Ǵ� ������ �����մϴ�.
	/// �̰��� <see cref="Pathfinding.IRaycastableGraph.Linecast"/> �޼����� ��ȯ���Դϴ�.
	/// �Ϻ� ����� �ƹ��͵� ��Ʈ���� �ʾҴ��� �ʱ�ȭ�˴ϴ�. �� ����� ���� �ڼ��� ������ ���� ��� ������ �����Ͻʽÿ�.
	/// </summary>
	public struct GraphHitInfo {
		/// <summary>
		/// ����/������ ������.
		/// ����ĳ��Ʈ �޼��忡 ���޵� ���� ���� ����� �׺�޽� ���� ���� ���� Ŭ����(clamping)�� �� ���Դϴ�.
		/// </summary>
		public Vector3 origin;
		/// <summary>
		/// ��Ʈ ����.
		/// ��ֹ��� ��Ʈ���� �ʾҴٸ� �� ������ ������ �������� �����˴ϴ�.
		/// </summary>
		public Vector3 point;
		/// <summary>
		/// ��Ʈ�� ������ ������ ���.
		/// ����ĳ��Ʈ�� �ƹ� �͵� ��Ʈ���� �ʾҴٸ� �� ���� ��ο� ���� ������ ���� �����˴ϴ� (�������� �����ϴ� ���).
		///
		/// ���� �׸��� �׷����� ���, ����ĳ��Ʈ�� �ƹ� �͵� ��Ʈ���� ������ (��, �������� ���� �þ߰� ������) �ش� �׷����� ã�ư� �� �� ����� X,Z ��ǥ�� �����˴ϴ�
		/// ������ �� ��尡 �ٸ� ������ �ִ��� (��: �ǹ��� �Ʒ��� �Ǵ� ������ ��ġ�� ���) �� ������ �ƹ��� ��� ������ ������ ��Ʈ���� �����Ƿ� �� �ʵ�� ������ null�� ���Դϴ�.
		/// </summary>
		public GraphNode node;
		/// <summary>
		/// ź��Ʈ ������. tangentOrigin�� tangent�� ��� ��Ʈ�� ������ �����մϴ�.
		/// </summary>
		public Vector3 tangentOrigin;
		/// <summary>
		/// ��Ʈ�� ������ ź��Ʈ.
		/// </summary>
		public Vector3 tangent;

		/// <summary><see cref="origin"/>���� <see cref="point"/>������ �Ÿ�</summary>
		public float distance
		{
			get
			{
				return (point - origin).magnitude;
			}
		}

		public GraphHitInfo (Vector3 point) {
			tangentOrigin  = Vector3.zero;
			origin = Vector3.zero;
			this.point = point;
			node = null;
			tangent = Vector3.zero;
		}
	}

	/// <summary>���� ����� ��� ���� ����. <see cref="AstarPath.GetNearest"/> �Լ��� ��ȯ�ϴ� ��带 �����ϴ� �����Դϴ�.</summary>
	public class NNConstraint
	{
		/// <summary>
		/// �˻� ������� ����� �׷����� �����մϴ�.
		/// �̰��� ��Ʈ����ũ�Դϴ�. ��Ʈ 0�� �׷��� ����� ù ��° �׷����� �˻��� �������� ���θ� �����ϸ�, ��Ʈ 1�� �� ��° �׷����� �������� ���θ� �����ϰ� �̷� ���Դϴ�.
		/// <code>
		/// // ù ��° �� �� ��° �׷����� �����ϵ��� ����
		/// myNNConstraint.graphMask = (1 << 0) | (1 << 2);
		/// </code>
		/// <code>
		/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
		/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
		///
		/// NNConstraint nn = NNConstraint.Default;
		///
		/// nn.graphMask = mask1 | mask2;
		///
		/// // 'My Grid Graph' �Ǵ� 'My Other Grid Graph'���� ���� ����� ���� ã���ϴ�.
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		///
		/// ����: �̰��� <see cref="AstarPath.GetNearest"/> ȣ��� ��ȯ�Ǵ� ��忡 ������ ��ġ����, ��� ��ũ�� ���� ��ȿ���� ���� �׷����� ����� ��쿡�� �˻��� �� �ֽ��ϴ�.
		///
		/// ����: <see cref="AstarPath.GetNearest"/>
		/// ����: <see cref="SuitableGraph"/>
		/// ����: ��Ʈ����ũ (�۵� ��ũ�� ������ �¶��� ������ �����ϼ���)
		/// </summary>
		public GraphMask graphMask = -1;

		/// <summary>������ ��ȿ�ϵ��� �����մϴ�. <see cref="area"/>�� 0 �̸��̸� �ƹ� ������ ��ġ�� �ʽ��ϴ�.</summary>
		public bool constrainArea;

		/// <summary>������ ���� ID. 0 �̸��̰ų� <see cref="constrainArea"/>�� false�̸� �ƹ� ������ ��ġ�� �ʽ��ϴ�.</summary>
		public int area = -1;

		/// <summary>�ȱ� ���� �Ǵ� �ȱ� �Ұ����� ���� �����մϴ�. </summary>
		public bool constrainWalkability = true;

		/// <summary>
		/// <see cref="constrainWalkability"/>�� Ȱ��ȭ�� ��쿡�� �ȱ� ������ ��� �Ǵ� �ȱ� �Ұ����� ��带 �˻��մϴ�.
		/// true�̸� �ȱ� ������ ��常 �˻��ϰ�, �׷��� ������ �ȱ� �Ұ����� ��常 �˻��մϴ�.
		/// <see cref="constrainWalkability"/>�� false�� ��� �ƹ��� ������ ��ġ�� �ʽ��ϴ�.
		/// </summary>
		public bool walkable = true;

		/// <summary>
		/// ������ ��� XZ �࿡�� �˻��ϴ� ���̸�, ��� �࿡�� �˻��ϴ� �� ��� XZ �࿡�� �˻��մϴ�.
		/// �׺���̼� �޽�/��ĳ��Ʈ �׷����� �̸� �����մϴ�.
		///
		/// �̰��� ��� �鿡�� �߿��� �� �ֽ��ϴ�. �Ʒ� �̹������� �Ķ� ������ ���� ����� ���� ���� ������ �� �� �ֽ��ϴ�.
		/// [�¶��� �������� �̹��� Ȯ��]
		///
		/// �׺���̼� �޽�/��ĳ��Ʈ �׷����� ���� �̿� ���� ���� �ɼ��� �����ϰ� �ֽ��ϴ�: <see cref="Pathfinding.NavmeshBase.nearestSearchOnlyXZ"/>.
		/// </summary>
		public bool distanceXZ;

		/// <summary>
		/// �±׸� �����ؾ� �ϴ��� ���θ� �����մϴ�.
		/// ����: <see cref="tags"/>
		/// </summary>
		public bool constrainTags = true;

		/// <summary>
		/// Ư�� �±װ� ������ ��常 �����մϴ�.
		/// �̰��� ��Ʈ����ũ��, ��Ʈ 0�� �±� 0�� ������ ��Ÿ���� ��Ʈ 3�� �±� 3�� ������ ��Ÿ���ϴ�.
		/// ����: <see cref="constrainTags"/>
		/// ����: <see cref="graphMask"/>
		/// ����: ��Ʈ����ũ (�۵� ��ũ�� ������ �¶��� �������� Ȯ��)
		/// </summary>
		public int tags = -1;

		/// <summary>
		/// �������� �Ÿ��� �����մϴ�.
		/// <see cref="AstarPath.maxNearestNodeDistance"/>���� ������ �Ÿ��� ����մϴ�.
		/// �� ���� false�� ��� �Ÿ� ������ ������ �����մϴ�.
		///
		/// �Ÿ� ���� ������ ������ ��尡 ���� ��� ����� null ��带 ��ȯ�մϴ�.
		/// ����: �� ���� �� Ŭ�������� ������ ������, A* Inspector -> ���� -> �ִ� ���� ��� �Ÿ����� ���˴ϴ�.
		/// </summary>
		public bool constrainDistance = true;

		/// <summary>
		/// �� NNConstraint�� ��Ģ�� ������ �׷������� ���θ� ��ȯ�մϴ�.
		/// ����: �� �Լ��� ����Ͽ� ����ϴ� �׷����� ó�� 31�� �׷����� ����˴ϴ�.
		/// <see cref="graphMask"/>�� ��Ʈ 31�� ������ ���(��, ������ ��Ʈ����ũ�� �°ԵǴ� �׷���) �� �̻��� ��� �׷����� �����ϰ� ����˴ϴ�.
		/// </summary>
		public virtual bool SuitableGraph (int graphIndex, NavGraph graph) {
			return graphMask.Contains(graphIndex);
		}

		/// <summary>�� ��尡 �� NNConstraint�� ��Ģ�� �������� ���θ� ��ȯ�մϴ�.</summary>
		public virtual bool Suitable(GraphNode node)
		{
			if (constrainWalkability && node.Walkable != walkable) return false;

			if (constrainArea && area >= 0 && node.Area != area) return false;

			if (constrainTags && ((tags >> (int)node.Tag) & 0x1) == 0) return false;

			return true;
		}

		/// <summary>
		/// �⺻ NNConstraint�Դϴ�.
		/// new NNConstraint ()�� �����մϴ�.
		/// ��κ��� ��� �۵��ϴ� ������ ������ ������, �ȱ� ������ ��常 ã�� A* Inspector -> ���� -> �ִ� ���� ��� �Ÿ��� ������ �Ÿ��� �����մϴ�.
		/// </summary>
		public static NNConstraint Default {
			get {
				return new NNConstraint();
			}
		}

		/// <summary>����� ���͸����� �ʴ� ���� ������ ��ȯ�մϴ�.</summary>
		public static NNConstraint None {
			get {
				return new NNConstraint {
						   constrainWalkability = false,
						   constrainArea = false,
						   constrainTags = false,
						   constrainDistance = false,
						   graphMask = -1,
				};
			}
		}

		/// <summary>�⺻ �������Դϴ�. �Ӽ� <see cref="Default"/>�� �����մϴ�.</summary>
		public NNConstraint()
		{
		}
	}

	/// <summary>
	/// ��ο��� ���� ���� �� ��忡 ���� �ٸ� ���� ����� �� �ִ� Ư�� NNConstraint�Դϴ�.
	/// Path.nnConstraint �ʵ忡 PathNNConstraint�� �Ҵ��ϸ� ��δ� ���� ���� ��带 �˻��� ���� <see cref="SetStart"/>�� ȣ���ϰ� �� ���(���� ��� ����� ��� ���)�� �˻��մϴ�.
	/// �⺻ PathNNConstraint�� ������ �������� ������ ������ ��ġ�ϵ��� �����մϴ�.
	/// </summary>
	public class PathNNConstraint : NNConstraint {
		public static new PathNNConstraint Default {
			get {
				return new PathNNConstraint {
						   constrainArea = true
				};
			}
		}

		/// <summary>���� ��带 ã�� �Ŀ� ȣ��˴ϴ�. ��ο��� ����� ���� �� �� ��忡 ���� �ٸ� �˻� ���� �������� �� ���˴ϴ�.</summary>
		public virtual void SetStart(GraphNode node)
		{
			if (node != null) {
				area = (int)node.Area;
			} else {
				constrainArea = false;
			}
		}
	}

	/// <summary>
	/// ���� ����� ��� ������ ���� ����Դϴ�.
	/// ����: NNInfo
	/// </summary>
	public struct NNInfoInternal
	{
		/// <summary>
		/// ã�� ���� ����� ����Դϴ�.
		/// �� ���� ���޵� NNConstraint�� ���� �ݵ�� �������� ���� �� �ֽ��ϴ�.
		/// ����: constrainedNode
		/// </summary>
		public GraphNode node;

		/// <summary>
		/// ���������� ä���� �� �ֽ��ϴ�.
		/// �˻����� �߰� ��� ���� ����� ��带 ã�� �� �ִ� ��� �̸� ä�� �� �ֽ��ϴ�.
		/// </summary>
		public GraphNode constrainedNode;

		/// <summary>��忡�� ���� ����� ������ ���� ��ġ�Դϴ�.</summary>
		public Vector3 clampedPosition;

		/// <summary>���������� ����� ��忡 ���� Ŭ������ ��ġ�Դϴ�.</summary>
		public Vector3 constClampedPosition;


		public NNInfoInternal (GraphNode node) {
			this.node = node;
			constrainedNode = null;
			clampedPosition = Vector3.zero;
			constClampedPosition = Vector3.zero;

			UpdateInfo();
		}

		/// <summary>��� ��ġ���� <see cref="clampedPosition"/> �� <see cref="constClampedPosition"/>�� ������Ʈ�մϴ�.</summary>
		public void UpdateInfo()
		{
			clampedPosition = node != null ? (Vector3)node.position : Vector3.zero;
			constClampedPosition = constrainedNode != null ? (Vector3)constrainedNode.position : Vector3.zero;
		}
	}

	/// <summary>���� ����� ��� ���� ����Դϴ�.</summary>
	public struct NNInfo
	{
		/// <summary>���� ����� ���</summary>
		public readonly GraphNode node;

		/// <summary>
		/// �׺�޽ÿ��� ���� ����� �����Դϴ�.
		/// �̴� ���� ��ġ�� <see cref="node"/>�� ���� ����� �������� Ŭ������ ���Դϴ�.
		/// </summary>
		public readonly Vector3 position;

		/// <summary>
		/// �׺�޽ÿ��� ���� ����� �����Դϴ�.
		/// �� �ʵ�� <see cref="position"/>���� �̸��� ����Ǿ����ϴ�.
		/// </summary>
		[System.Obsolete("This field has been renamed to 'position'")]
		public Vector3 clampedPosition
		{
			get
			{
				return position;
			}
		}
		/// <summary>
		/// NNInfoInternal�� ���� �������Դϴ�.
		/// internalInfo�� ������ ������� ���� ����� ���� ��ġ�� �����մϴ�.
		/// </summary>
		public NNInfo(NNInfoInternal internalInfo)
		{
			node = internalInfo.node;
			position = internalInfo.clampedPosition;
		}

		/// <summary>
		/// NNInfo�� Vector3�� ��������� ��ȯ�մϴ�.
		/// </summary>
		public static explicit operator Vector3(NNInfo ob)
		{
			return ob.position;
		}

		/// <summary>
		/// NNInfo�� GraphNode���� ��������� ��ȯ�մϴ�.
		/// </summary>
		public static explicit operator GraphNode(NNInfo ob)
		{
			return ob.node;
		}
	}

	/// <summary>
	/// ����� ���� �Ǵ� ���� ������ ���� ���� �����Դϴ�.
	/// ������Ʈ�� ��ĵ ��ɿ��� ���˴ϴ�.
	/// ����: <see cref="AstarPath.ScanAsync"/>
	/// </summary>
	public struct Progress
	{
		/// <summary>0���� 1 ������ ���� �����</summary>
		public readonly float progress;
		/// <summary>���� ���� ���� �۾��� ���� ����</summary>
		public readonly string description;

		public Progress(float progress, string description)
		{
			this.progress = progress;
			this.description = description;
		}

		public Progress MapTo(float min, float max, string prefix = null)
		{
			return new Progress(Mathf.Lerp(min, max, progress), prefix + description);
		}

		public override string ToString()
		{
			return progress.ToString("0.0") + " " + description;
		}
	}


	/// <summary>
	/// ��Ÿ�� �߿� ������Ʈ�� �� �ִ� �׷����Դϴ�.
	/// </summary>
	public interface IUpdatableGraph
	{
		/// <summary>
		/// ������ <see cref="GraphUpdateObject"/>�� ����Ͽ� ������ ������Ʈ�մϴ�.
		///
		/// �����ڿ��� ������ ����:
		/// �� �Լ��� ������ ���� ������ �۵��ؾ� �մϴ�:
		/// -# �� ��忡 ���� GUO�� o.WillUpdateNode�� ȣ���ؾ� �մϴ�. ��忡 ���� ���� ������ �����ϱ� ���� ȣ��Ǿ�� �ϸ�, ��忡 ���� ���� ������ �����ϱ� ���� ȣ���ؾ� �մϴ�.
		/// -# �ȱ� ���ɼ��� ����ϰų� GridGraph�� �Բ� ���Ǵ� usePhysics �÷��׿� ���� Ư���� ������ ����Ͽ� �ȱ� ���ɼ��� ������Ʈ�մϴ�.
		/// -# GUO�� �Բ� ������Ʈ�ؾ� �ϴ� �� ��忡 ���� Apply�� ȣ���մϴ�.
		/// -# ������ ��� ���Ἲ ������ ������Ʈ�մϴ�(GridGraph�� ���Ἲ�� ������Ʈ������ ��κ��� �ٸ� �׷����� ���߿� ���Ἲ�� ������ �� �����Ƿ� ������Ʈ���� �ʽ��ϴ�).
		/// </summary>
		void UpdateArea(GraphUpdateObject o);

		/// <summary>
		/// ������Ʈ�� �����ϱ� ���� Unity �����忡�� ȣ��� �� �ֽ��ϴ�.
		/// ����: CanUpdateAsync
		/// </summary>
		void UpdateAreaInit(GraphUpdateObject o);

		/// <summary>
		/// ������Ʈ�� ������ �Ŀ� Unity �����忡�� ȣ��� �� �ֽ��ϴ�.
		/// ����: CanUpdateAsync
		/// </summary>
		void UpdateAreaPost(GraphUpdateObject o);

		GraphUpdateThreading CanUpdateAsync(GraphUpdateObject o);
	}

	/// <summary>�׷��� ������Ʈ�� ����Ǿ����� ���ο� ���� ����</summary>
	public enum GraphUpdateStage
	{
		/// <summary>
		/// �׷��� ������Ʈ ��ü�� �����Ǿ����� ���� �ƹ� �۾��� ������ �ʾҽ��ϴ�.
		/// �� ���� �⺻���Դϴ�.
		/// </summary>
		Created,
		/// <summary>�׷��� ������Ʈ�� ��� ã�� �ý������� ���۵Ǿ� �׷����� ���� �����Դϴ�.</summary>
		Pending,
		/// <summary>�׷��� ������Ʈ�� ��� �׷����� ����Ǿ����ϴ�.</summary>
		Applied,
		/// <summary>
		/// �׷��� ������Ʈ�� �ߴܵǾ����� ������� ���� ���Դϴ�.
		/// �̰��� �׷��� ������Ʈ�� ���� �����̾����� �׷��� ������Ʈ�� ��⿭�� ���� �� AstarPath ������Ʈ�� �ı��� ��� �߻��� �� �ֽ��ϴ�.
		/// </summary>
		Aborted,
	}

	/// <summary>
	/// �׷����� Ư�� ���� ���� ��带 ������Ʈ�ϱ� ���� ���Ǵ� ������ �÷����� ��Ÿ���ϴ�.
	/// ����: AstarPath.UpdateGraphs
	/// ����: �׷��� ������Ʈ (�۵� ��ũ�� ������ �¶��� �������� Ȯ��)
	/// </summary>
	public class GraphUpdateObject
	{
		/// <summary>
		/// ��带 ������Ʈ�� ����Դϴ�.
		/// ���� �������� ���ǵ˴ϴ�.
		/// </summary>
		public Bounds bounds;

		/// <summary>
		/// �� GUO�� ����� �Ŀ� ȫ�� ä��⸦ �������� ���θ� �����մϴ�.
		/// �� ����� ��Ȱ��ȭ�ϸ� ���� ����� ���� �� ������ �����ؼ� ����ؾ� �մϴ�.
		/// GUO�� �ȱ� ���ɼ��̳� ���Ἲ�� �������� ���� ���� Ȯ���� ��쿡�� false�� ������ �� �ֽ��ϴ�.
		/// ���� ��� ����� ���Ƽ ���� ������Ʈ�ϴ� ��� false�� �����ϸ� ó������ ������ �� �ֽ��ϴ�. Ư�� ū �׷������� �����մϴ�.
		/// ����: �� ���� false�� �����ϸ� �ȱ� ���ɼ��� ����Ǿ����� ��ΰ� �����ϴ� ��찡 �߻��� �� �ְų� ��ΰ� �������� �ұ��ϰ� ��ü �׷������� ��θ� �˻��Ϸ��� �õ��ϸ鼭 ���� ó������ ����� �� �ֽ��ϴ�.
		///
		/// �⺻ GraphUpdateObject (�Ļ� Ŭ������ �ƴ� ���)�� ����ϴ� ��� �̰��� ȫ�� ä��Ⱑ �ʿ����� ���θ� ������ Ȯ���ϴ� ����� 
		/// <see cref="modifyWalkability"/>�� true�̰ų� <see cref="updatePhysics"/>�� true���� Ȯ���ϴ� ���Դϴ�.
		/// ����: �� �̻� �ʿ����� ����
		/// </summary>
		[System.Obsolete("Not necessary anymore")]
		public bool requiresFloodFill { set { } }

		/// <summary>
		/// ��带 ������Ʈ�� �� ���� �˻縦 ����մϴ�.
		/// GridGraph�� ������Ʈ�ϰ� updatePhysics�� true�� ������ ���, ����� ��ġ �� �ȱ� ���ɼ��� "�浹 �׽�Ʈ" �� "���� �׽�Ʈ"���� ������ ���� ���� �˻縦 ����Ͽ� ������Ʈ�˴ϴ�.
		///
		/// PointGraph�� ������Ʈ�ϰ� �� ���� true�� �����ϸ� �׷����� ����ϴ� ��� ������ �ٽ� ���մϴ�.
		///
		/// �� ���� <see cref="modifyWalkability"/>�� ���� �ִ� ��� GridGraph�� ������Ʈ�� ���� �ƹ� ������ ��ġ�� �ʽ��ϴ�.
		/// <see cref="updatePhysics"/>�� <see cref="modifyWalkability"/>�� �����ؼ��� �� �˴ϴ�.
		///
		/// RecastGraph�� ��� �� ���� Ȱ��ȭ�ϸ� ��踦 �����ϴ� ��� Ÿ���� ������ �ٽ� ����մϴ�.
		/// �̰��� ����� �����ϴ�(������ �����մϴ�). ���� ����� ���Ƽ�� ������Ʈ�Ϸ��� ��Ȱ��ȭ�� ���·� �� �� �ֽ��ϴ�.
		/// </summary>
		public bool updatePhysics = true;

		/// <summary>
		/// GridGraph�� ��� ��带 ������Ʈ�� �� �ʱ� ������ ���Ƽ�� �缳���մϴ�.
		/// �׷����� ������Ʈ�� �� ���Ƽ�� �����Ϸ��� �� �ɼ��� ��Ȱ��ȭ�� �� �ֽ��ϴ�.
		///
		/// �Ʒ� �̹����� ���� ��ġ�� �� ���� �׷��� ������Ʈ ��ü�� �����ݴϴ�. ������ ��ü�� ���� ��ü���� ���� ����Ǿ����ϴ�. �� ���� ��ü�� ��� ����� ���Ƽ�� ��� ���� �ø����� �����Ǿ����ϴ�.
		///
		/// ù ��° �̹����� resetPenaltyOnPhysics�� false�� ��� ����� �����ݴϴ�. �� ���� ���Ƽ�� �ùٸ��� �߰��˴ϴ�.
		///
		/// �� ��° �̹����� resetPenaltyOnPhysics�� true�� ������ ��츦 �����ݴϴ�. ù ��° GUO�� �ùٸ��� ����ǰ�, �׷� ���� �� ��° GUO(���� ��ü)�� ����Ǿ� ���Ƽ�� ���� �缳���ϰ� ��忡 ���Ƽ�� �߰��մϴ�. 
		/// ��������� �� GUO�� ���Ƽ�� �������� �ʽ��ϴ�. �׵θ��� ��� ��ġ�� ���� ���(����� ��ġ ����, ��ֹ� Ȯ�� ��)�� ����� ���� GUO ��迡�� ������ ��(Grid Graph -> �浹 �׽�Ʈ -> ���� ����)���� ���� 
		/// �� ū ������ ������ ��ġ�� ������ �߻��մϴ�(�� ������ Ȯ���). ���� �Ϻ� �߰� ����� ���Ƽ�� �缳���˴ϴ�.
		/// </summary>
		public bool resetPenaltyOnPhysics = true;


		/// <summary>
		/// �׸��� �׷����� Erosion ������Ʈ.
		/// Ȱ��ȭ�Ǹ� GUO(GraphUpdateObject)�� ����� �� �׸��� �׷����� Erosion(ħ��)�� ����˴ϴ�.
		///
		/// �Ʒ� �̹������� �پ��� ȿ���� �� �� �ֽ��ϴ�.
		/// ù ��° �̹����� GUO�� ������� �ʾ��� ���� �׷����� �����ݴϴ�. �Ķ� ���ڴ� �׷������� ��ֹ��� �νĵ��� ������,
		/// �ֺ��� ����ȿ�� ��尡 �ִ� ������ ���� ���� �����Դϴ� (���� ���� ���� ��ġ��) ���� Erosion(ħ��)�� ����˴ϴ� (�� �׷��������� Erosion ������ 2�� �����).
		/// ��Ȳ�� ���ڴ� ��ֹ��� �νĵǾ� �ֺ��� ����ȿ�� ��� ������ �ణ �� Ů�ϴ�. Erosion�� �浹 ��η� ���� ��尡 ����ȿ�մϴ�.
		/// ���� GUO�� ��� ����� �ȱ� ���ɼ��� true�� �����ϴ� �ͻ��Դϴ�.
		///
		/// [�¶��� ������ ���� �̹��� ����]
		///
		/// updateErosion�� True�� �� �Ķ� ���� �ֺ��� ������ ����ȿ�� ��尡 �ִ� ������ ������ ���� ���̰� �ֱ� �����Դϴ�.
		/// ��Ȳ�� ���ڴ� ���� ���̰� �����Ƿ� ��� ��尡 �ȱ� �����մϴ�.
		/// 
		/// updateErosion�� False�� ���� �� �������� ��� ����� �ȱ� ���ɼ��� ������ �ȱ� �����ϵ��� �����˴ϴ�.
		///
		/// ����: Pathfinding.GridGraph
		/// </summary>
		public bool updateErosion = true;

		/// <summary>
		/// ����� NNConstraint(��� ���� ����).
		/// NNConstraint.SuitableGraph �Լ��� NNConstraint���� ȣ��˴ϴ�.
		/// � �׷����� ������Ʈ���� ���͸��ϴ� �� ���˴ϴ�.
		/// ����: NNConstraint.SuitableGraph �Լ��� A* Pathfinding Project Pro������ ��� �����ϹǷ� ���� ���������� �� ������ ������ �ƹ� ������ ��ġ�� �ʽ��ϴ�.
		/// </summary>
		public NNConstraint nnConstraint = NNConstraint.None;

		/// <summary>
		/// ��忡 �߰��� �г�Ƽ(ó��).
		/// 1000�� �г�Ƽ�� 1���� ���� ������ �̵��ϴ� ���� �����մϴ�.
		/// </summary>
		public int addPenalty;

		/// <summary>
		/// ���� ���, ��� ����� 'walkable' ������ 'setWalkability'�� �����˴ϴ�.
		/// 'updatePhysics'�� �Բ� ������� �ʴ� ���� �����ϴ�. �׷��� �ϸ� 'updatePhysics'�� ����� ���� ��� �� �ֱ� �����Դϴ�.
		/// </summary>
		public bool modifyWalkability;

		/// <summary>'modifyWalkability'�� ���� ���, ����� 'walkable' ������ �� ������ �����˴ϴ�.</summary>
		public bool setWalkability;

		/// <summary>'modifyTag'�� ���� ���, ��� ����� 'tag'�� 'setTag'�� �����˴ϴ�.</summary>
		public bool modifyTag;

		/// <summary>'modifyTag'�� ���� ���, ��� ����� 'tag'�� �� ������ �����˴ϴ�.</summary>
		public int setTag;

		/// <summary>
		/// ��尡 ����� ���� �����ϰ� ��� �����͸� �����ϴ� �� ���˴ϴ�.
		/// �ʿ��� ��� ���� ������ �ǵ��� �� ���������� ���˴ϴ�.
		/// </summary>
		public bool trackChangedNodes;

		/// <summary>
		/// �� GraphUpdateObject�� ���� ������Ʈ�� �����Դϴ�.
		/// 'trackChangedNodes'�� ���� ��쿡�� ä�����ϴ�.
		/// ����: �׷��� ������Ʈ ������Ʈ�� ����Ǳ���� �� �������� �ɸ� �� �ֽ��ϴ�.
		/// ��� �� ������ �ʿ��� ���, 'AstarPath.FlushGraphUpdates'�� ����ϼ���.
		/// </summary>
		public List<GraphNode> changedNodes;
		private List<uint> backupData;
		private List<Int3> backupPositionData;

		/// <summary>
		/// Bounds ��ü�� ����� ��Ȯ���� �������� �ʴ� ��� ����� ������ �� �ֽ��ϴ�.
		/// ����� �����Ǹ� bounds�� ����� ���ε��� �����ؾ� �մϴ�.
		/// bounds�� ��带 ������ Ȯ���ϴ� �ʱ� Ȯ�� �뵵�� ���˴ϴ�.
		/// </summary>
		public GraphUpdateShape shape;

		/// <summary>
		/// �׷��� ������Ʈ�� ���� ���� �����Դϴ�.
		/// ������(����: STAGE_CREATED �� ������ ���) �Ǵ� �� �׷��� ������Ʈ�� ���� ��� ���� �׷����� ������ ��Ÿ���� ������ �� �� �ֽ��ϴ�.
		/// </summary>
		internal int internalStage = STAGE_CREATED;


		internal const int STAGE_CREATED = -1;
		internal const int STAGE_PENDING = -2;
		internal const int STAGE_ABORTED = -3;
		internal const int STAGE_APPLIED = 0;

		/// <summary>�׷��� ������Ʈ�� ���� ���� ����</summary>
		public GraphUpdateStage stage {
			get {
				switch (internalStage) {
				case STAGE_CREATED:
					return GraphUpdateStage.Created;
				case STAGE_APPLIED:
					return GraphUpdateStage.Applied;
				case STAGE_ABORTED:
					return GraphUpdateStage.Aborted;
					// ����� ���� ���� ������ �ǹ��ϹǷ� ��� ���̱⵵ �մϴ�.
					default:
				case STAGE_PENDING:
					return GraphUpdateStage.Pending;
				}
			}
		}

		/// <summary>
		/// �� GUO�� ����Ͽ� ������Ʈ�Ǵ� �� ��忡 ���� ȣ���ؾ� �մϴ�.
		/// ����: trackChangedNodes
		/// </summary>
		/// <param name="node">�ʵ带 ������ ���. null�� ��� �ƹ� �۾��� ������� �ʽ��ϴ�.</param>
		public virtual void WillUpdateNode(GraphNode node)
		{
			if (trackChangedNodes && node != null) {
				if (changedNodes == null) { changedNodes = ListPool<GraphNode>.Claim(); backupData = ListPool<uint>.Claim(); backupPositionData = ListPool<Int3>.Claim(); }
				changedNodes.Add(node);
				backupPositionData.Add(node.position);
				backupData.Add(node.Penalty);
				backupData.Add(node.Flags);
#if !ASTAR_NO_GRID_GRAPH
				var gridNode = node as GridNode;
				if (gridNode != null) backupData.Add(gridNode.InternalGridFlags);
#endif
			}
		}

		/// <summary>
		/// ������� �г�Ƽ �� �÷���(�ȱ� ���ɼ� ����)�� �����մϴ�.
		/// trackChangedNodes�� true�� ������ ��쿡�� �����Ͱ� ����˴ϴ�.
		///
		/// ����: ��� �����Ͱ� ����Ǵ� ���� �ƴմϴ�. ����� �����Ϳ��� �г�Ƽ, �ȱ� ���ɼ�, �±�, ����, ��ġ �� �׸��� �׷����� ��� (���� ����ȭ���� ���� ���) ���� �����͵� ���Ե˴ϴ�.
		///
		/// �� �޼���� �׷����� �����ϹǷ� �׷����� �����ϴ� ���� ������ ���¿��� ȣ��Ǿ�� �մϴ�.
		/// ���� ��� �Ʒ� ���������� ���� �۾� �׸� ������ ȣ��Ǿ�� �մϴ�.
		///
		/// ����: MiscSnippets.cs GraphUpdateObject.RevertFromBackup
		///
		/// ����: blocking (�¶��� �������� �۵� ��ũ ����)
		/// ����: Pathfinding.PathUtilities.UpdateGraphsNoBlock
		/// </summary>
		public virtual void RevertFromBackup()
		{
			if (trackChangedNodes) {
				if (changedNodes == null) return;

				int counter = 0;
				for (int i = 0; i < changedNodes.Count; i++) {
					changedNodes[i].Penalty = backupData[counter];
					counter++;
					// �÷��׸� ���������� HierarchicalNodeIndex�� ���� ������ ������ ��ĥ �� �����Ƿ� �������� �ʽ��ϴ�.
					var tmp = changedNodes[i].HierarchicalNodeIndex;
					changedNodes[i].Flags = backupData[counter];
					changedNodes[i].HierarchicalNodeIndex = tmp;
					counter++;
#if !ASTAR_NO_GRID_GRAPH
					var gridNode = changedNodes[i] as GridNode;
					if (gridNode != null) {
						gridNode.InternalGridFlags = (ushort)backupData[counter];
						counter++;
					}
#endif
					changedNodes[i].position = backupPositionData[i];
					changedNodes[i].SetConnectivityDirty();
				}

				ListPool<GraphNode>.Release(ref changedNodes);
				ListPool<uint>.Release(ref backupData);
				ListPool<Int3>.Release(ref backupPositionData);
			}
			else
			{
				throw new System.InvalidOperationException("����� ��尡 �������� �ʾ����Ƿ� ������� ������ �� �����ϴ�. ������Ʈ�� �����ϱ� ���� trackChangedNodes�� true�� �����ϼ���.");
			}
		}

		/// <summary>�� GUO�� ������ ����Ͽ� ������ ��带 ������Ʈ�մϴ�.</summary>
		public virtual void Apply(GraphNode node)
		{
			if (shape == null || shape.Contains(node)) {
				// �г�Ƽ �� �ȱ� ���ɼ� ������Ʈ
				node.Penalty = (uint)(node.Penalty+addPenalty);
				if (modifyWalkability) {
					node.Walkable = setWalkability;
				}

				// �±� ������Ʈ
				if (modifyTag) node.Tag = (uint)setTag;
			}
		}

		public GraphUpdateObject () {
		}

		/// <summary>������ �ٿ�� �ڽ��� �� GUO�� �����մϴ�.</summary>
		public GraphUpdateObject (Bounds b) {
			bounds = b;
		}
	}

	/// <summary>�׷��� �������� ���� ���������� ���ǵ� ��ȯ�� ������ �׷����Դϴ�.</summary>
	public interface ITransformedGraph
	{
		GraphTransform transform { get; }
	}


	/// <summary>Linecast �޼��带 �����ϴ� �׷����Դϴ�.</summary>
	public interface IRaycastableGraph {
		bool Linecast(Vector3 start, Vector3 end);
		bool Linecast(Vector3 start, Vector3 end, GraphNode hint);
		bool Linecast(Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit);
		bool Linecast(Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace);
		bool Linecast(Vector3 start, Vector3 end, out GraphHitInfo hit, List<GraphNode> trace, System.Func<GraphNode, bool> filter);
	}

	/// <summary>
	/// ���� ��ǥ ������ ����ϴ� �簢���Դϴ�.
	/// �������� ��ǥ ������ ����մϴ�.
	///
	/// ���� UnityEngine.Rect�� �����ϰ� ���������� ���� ��ǥ�� ����մϴ�.
	/// </summary>
	[System.Serializable]
	public struct IntRect {
		public int xmin, ymin, xmax, ymax;

		public IntRect (int xmin, int ymin, int xmax, int ymax) {
			this.xmin = xmin;
			this.xmax = xmax;
			this.ymin = ymin;
			this.ymax = ymax;
		}

		public bool Contains (int x, int y) {
			return !(x < xmin || y < ymin || x > xmax || y > ymax);
		}

		public int Width {
			get {
				return xmax-xmin+1;
			}
		}

		public int Height {
			get {
				return ymax-ymin+1;
			}
		}

		public int Area {
			get {
				return Width * Height;
			}
		}

		/// <summary>
		/// �� �簢���� ��ȿ���� ���θ� ��ȯ�մϴ�.
		/// ��ȿ���� ���� �簢���� ���� ��� xmin > xmax�� ����Դϴ�.
		/// ������ 0�� �簢���� ��ȿ���� �ʽ��ϴ�.
		/// </summary>
		public bool IsValid () {
			return xmin <= xmax && ymin <= ymax;
		}

		public static bool operator == (IntRect a, IntRect b) {
			return a.xmin == b.xmin && a.xmax == b.xmax && a.ymin == b.ymin && a.ymax == b.ymax;
		}

		public static bool operator != (IntRect a, IntRect b) {
			return a.xmin != b.xmin || a.xmax != b.xmax || a.ymin != b.ymin || a.ymax != b.ymax;
		}

		public override bool Equals (System.Object obj) {
			var rect = (IntRect)obj;

			return xmin == rect.xmin && xmax == rect.xmax && ymin == rect.ymin && ymax == rect.ymax;
		}

		public override int GetHashCode () {
			return xmin*131071 ^ xmax*3571 ^ ymin*3109 ^ ymax*7;
		}

		/// <summary>
		/// �� �簢���� ���� ������ ��ȯ�մϴ�.
		/// ���� ������ �� �簢�� ��� �ȿ� �ִ� �����Դϴ�.
		/// �� �簢���� �������� ������ ��ȿ���� ���� �簢���� ��ȯ�˴ϴ�.
		/// ����: IsValid
		/// </summary>
		public static IntRect Intersection (IntRect a, IntRect b) {
			return new IntRect(
				System.Math.Max(a.xmin, b.xmin),
				System.Math.Max(a.ymin, b.ymin),
				System.Math.Min(a.xmax, b.xmax),
				System.Math.Min(a.ymax, b.ymax)
				);
		}

		/// <summary>�� ���� �簢���� �����ϴ��� ���θ� ��ȯ�մϴ�.</summary>
		public static bool Intersects(IntRect a, IntRect b)
		{
			return !(a.xmin > b.xmax || a.ymin > b.ymax || a.xmax < b.xmin || a.ymax < b.ymin);
		}

		/// <summary>
		/// �� ���� �簢���� �����ϴ� ���ο� �簢���� ��ȯ�մϴ�.
		/// �� �簢���� ��쿡 ���� �� �Է� �簢�� ���� ������ ������ �� �ֽ��ϴ�.
		/// </summary>
		public static IntRect Union (IntRect a, IntRect b) {
			return new IntRect(
				System.Math.Min(a.xmin, b.xmin),
				System.Math.Min(a.ymin, b.ymin),
				System.Math.Max(a.xmax, b.xmax),
				System.Math.Max(a.ymax, b.ymax)
				);
		}

		/// <summary>����Ʈ�� �����ϴ� ���ο� IntRect�� ��ȯ�մϴ�.</summary>
		public IntRect ExpandToContain (int x, int y) {
			return new IntRect(
				System.Math.Min(xmin, x),
				System.Math.Min(ymin, y),
				System.Math.Max(xmax, x),
				System.Math.Max(ymax, y)
				);
		}

		/// <summary>��� �������� ������ Ȯ���� ���ο� �簢���� ��ȯ�մϴ�.</summary>
		/// <param name="range">�󸶳� Ȯ���� ���� ��Ÿ���� ���Դϴ�. ���� ���� ���˴ϴ�.</param>
		public IntRect Expand(int range)
		{
			return new IntRect(xmin-range,
				ymin-range,
				xmax+range,
				ymax+range
				);
		}

		public override string ToString () {
			return "[x: "+xmin+"..."+xmax+", y: " + ymin +"..."+ymax+"]";
		}

		/// <summary>�簢���� ��Ÿ���� ����� ������ �׸��ϴ�.</summary>
		public void DebugDraw (GraphTransform transform, Color color) {
			Vector3 p1 = transform.Transform(new Vector3(xmin, 0, ymin));
			Vector3 p2 = transform.Transform(new Vector3(xmin, 0, ymax));
			Vector3 p3 = transform.Transform(new Vector3(xmax, 0, ymax));
			Vector3 p4 = transform.Transform(new Vector3(xmax, 0, ymin));

			Debug.DrawLine(p1, p2, color);
			Debug.DrawLine(p2, p3, color);
			Debug.DrawLine(p3, p4, color);
			Debug.DrawLine(p4, p1, color);
		}
	}

	/// <summary>
	/// �׷������� ��Ʈ����ũ�� �����մϴ�.
	/// �� ��Ʈ����ũ�� �ִ� 32���� �׷����� ������ �� �ֽ��ϴ�.
	///
	/// ��Ʈ����ũ�� ������ �Ϲ������� ��ȯ �� ��ȯ�� �� �ֽ��ϴ�.
	///
	/// <code>
	/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
	/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
	///
	/// NNConstraint nn = NNConstraint.Default;
	///
	/// nn.graphMask = mask1 | mask2;
	///
	/// // 'My Grid Graph' �Ǵ� 'My Other Grid Graph' �� �ϳ��� ���ϴ� somePoint�� ���� ����� ��带 ã���ϴ�.
	/// var info = AstarPath.active.GetNearest(somePoint, nn);
	/// </code>
	///
	/// ����: bitmasks (�۵� ��ũ�� ���� �¶��� �������� ����)
	/// </summary>
	[System.Serializable]
	public struct GraphMask {
		/// <summary>����ũ�� ��Ÿ���� ��Ʈ����ũ�Դϴ�.</summary>
		public int value;

		/// <summary>��� �׷����� �����ϴ� ����ũ�Դϴ�.</summary>
		public static GraphMask everything { get { return new GraphMask(-1); } }


		public GraphMask (int value) {
			this.value = value;
		}

		public static implicit operator int(GraphMask mask) {
			return mask.value;
		}

		public static implicit operator GraphMask (int mask) {
			return new GraphMask(mask);
		}

		/// <summary>�� ����ũ�� �����Ͽ� ���� �κ��� �����մϴ�.</summary>
		public static GraphMask operator &(GraphMask lhs, GraphMask rhs)
		{
			return new GraphMask(lhs.value & rhs.value);
		}

		/// <summary>�� ����ũ�� �����Ͽ� �������� �����մϴ�.</summary>
		public static GraphMask operator |(GraphMask lhs, GraphMask rhs)
		{
			return new GraphMask(lhs.value | rhs.value);
		}

		/// <summary>����ũ�� �����մϴ�.</summary>
		public static GraphMask operator ~(GraphMask lhs)
		{
			return new GraphMask(~lhs.value);
		}

		/// <summary>�� ����ũ�� �־��� �׷��� �ε����� �����ϴ��� ���θ� ��ȯ�մϴ�.</summary>
		public bool Contains(int graphIndex)
		{
			return ((value >> graphIndex) & 1) != 0;
		}

		/// <summary>�־��� �׷����� �����ϴ� ��Ʈ����ũ�� ��ȯ�մϴ�.</summary>
		public static GraphMask FromGraph(NavGraph graph)
		{
			return 1 << (int)graph.graphIndex;
		}

		public override string ToString () {
			return value.ToString();
		}

		/// <summary>
		/// �־��� �̸��� ���� ù ��° �׷����� �����ϴ� ��Ʈ����ũ�� ��ȯ�մϴ�.
		/// <code>
		/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
		/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
		///
		/// NNConstraint nn = NNConstraint.Default;
		///
		/// nn.graphMask = mask1 | mask2;
		///
		/// // 'My Grid Graph' �Ǵ� 'My Other Grid Graph' �� �ϳ��� ���ϴ� somePoint�� ���� ����� ��带 ã���ϴ�.
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		/// </summary>
		public static GraphMask FromGraphName (string graphName) {
			var graph = AstarData.active.data.FindGraph(g => g.name == graphName);

			if (graph == null) throw new System.ArgumentException("'" + graphName + "' �̸��� ���� �׷����� ã�� �� �����ϴ�.");
			return FromGraph(graph);
		}
	}

	#region Delegates

	/* Path ��ü�� �Ű������� ����ϴ� �븮���Դϴ�.
	 * �̰��� ��� ����� �Ϸ�Ǿ��� �� �ݹ鿡 ���˴ϴ�.
	 * ���� �Լ�:
	 * \snippet MiscSnippets.cs OnPathDelegate
	 */
	public delegate void OnPathDelegate(Path p);

	public delegate void OnGraphDelegate(NavGraph graph);

	public delegate void OnScanDelegate(AstarPath script);

	/// <summary>
	/// ��ĵ ���¿� ���� �븮���Դϴ�.
	/// �� �븮�ڴ� �� �̻� ������ �ʽ��ϴ�. (Deprecated)
	/// </summary>
	/// <param name="progress">���� ���� ����</param>
	public delegate void OnScanStatus(Progress progress);

	#endregion

	#region Enums

	/// <summary>
	/// ��� ������Ʈ ������ ����
	/// </summary>
	public enum GraphUpdateThreading
	{
		/// <summary>
		/// Unity �����忡�� UpdateArea�� ȣ���մϴ�. �⺻���Դϴ�. SeparateThread�� ȣȯ���� �ʽ��ϴ�.
		/// </summary>
		UnityThread = 0,
		/// <summary>UpdateArea�� ������ �����忡�� ȣ���մϴ�. UnityThread�� ȣȯ���� �ʽ��ϴ�.</summary>
		SeparateThread = 1 << 0,
		/// <summary>��� �� ������ Unity �����忡�� UpdateAreaInit�� ȣ���մϴ�.</summary>
		UnityInit = 1 << 1,
		/// <summary>
		/// ��� �� ���Ŀ� Unity �����忡�� UpdateAreaPost�� ȣ���մϴ�.
		/// �̰��� SeparateThread�� �Բ� ���Ǹ� �ٸ� ��ũ��Ʈ�� �׷����� ����� �� �׷����� �������� �ʰ� ���� ������ ��� ����� �����ϱ� ���� ���˴ϴ� (��: GetNearest ȣ��).
		/// </summary>
		UnityPost = 1 << 2,
		/// <summary>SeparateThread �� UnityInit�� ����</summary>
		SeparateAndUnityInit = SeparateThread | UnityInit
	}


	/// <summary>�ý��ۿ��� ��� ����� ����ϴ� ���</summary>
	public enum PathLog
	{
		/// <summary>�ƹ��͵� ������� �ʽ��ϴ�. �������� ����˴ϴ�. ��� ��� ��Ͽ��� ���� ������尡 �ֽ��ϴ�.</summary>
		None,
		/// <summary>��ο� ���� �⺻ ������ ����մϴ�.</summary>
		Normal,
		/// <summary>�߰� ������ �����մϴ�.</summary>
		Heavy,
		/// <summary>���ſ� ������ ���������� ���� �� GUI�� ����Ͽ� ������ ǥ���մϴ�.</summary>
		InGame,
		/// <summary>��ְ� ���������� ������ �߻��� ��θ� ����մϴ�.</summary>
		OnlyErrors
	}

	/// <summary>
	/// ��� ã�� �� ������ �̵��ϴ� ����� ��� ���������� �����մϴ�.
	/// 
	/// �޸���ƽ(heuristic)�� ���� ��忡�� ��ǥ������ ���� ����Դϴ�.
	/// ���� �ٸ� �޸���ƽ�� ��κ��� ��� ������ ���� ���������� �޸���ƽ�� ������� �ʴ� �ɼ�(<see cref="None"/>)�� �Ϲ������� �ſ� �����ϴ�.
	/// �Ʒ� �̹��������� 8-���� �׸���� 4-���� �׸��忡 ���� �پ��� �޸���ƽ �ɼ��� ���� ���Դϴ�.
	/// ��� ��ΰ� ��� ���� ���� ������, �� ���� ���� ��� �� ��� ���� ���õǴ����� ���� �������� �ֽ��ϴ�.
	/// Diagonal Manhattan �� Manhattan �ɼ��� 8-���� �׸��忡�� �ſ� �ٸ��� �۵��ϴ� ��ó�� �������� ���� ���� �ݿø� ���� ������ �ٸ��� �۵��մϴ�. ���� 8-���� �׸��忡�� ���� �����ϰ� �۵��մϴ�.
	/// 
	/// �Ϲ������� 4-���� �׸��忡 ���� Manhattan �ɼ��� ����ϴ� ���� �����ϴ�. �̰��� 4-���� �׸��忡���� ���� �Ÿ��Դϴ�.
	/// 8-���� �׸��忡 ���� Diagonal Manhattan �ɼ��� ���������� ���� ��Ȯ�� �ɼ��Դϴ�. �׷��� Euclidean �ɼ��� ���� ��ȣ�Ǹ�, Ư�� ������(modifier)�� ����Ͽ� ��θ� �ܼ�ȭ�ϴ� ��쿡�� Euclidean �ɼ��� �� ���� �� �ֽ��ϴ�.
	/// �׸��� ����� �ƴ� ��� �׷����� ���ؼ��� Euclidean �ɼ��� ���� �����մϴ�.
	/// 
	/// ����: <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">��Ű���: A* �˻� �˰���</a>
	/// </summary>
	public enum Heuristic {
		/// <summary>����ư �Ÿ��Դϴ�. ����: https://en.wikipedia.org/wiki/Taxicab_geometry</summary>
		Manhattan,
		/// <summary>
		/// ����ư �Ÿ������� �밢�� �̵��� ����մϴ�.
		/// ����: ���� XZ ��鿡 ���� �ϵ� �ڵ��Ǿ� ������ 2D ���� (��, 2D ����)���� ����Ϸ����ϸ� ����ư �Ÿ��� �����մϴ�.
		/// </summary>
		DiagonalManhattan,
		/// <summary>�Ϲ����� �Ÿ��Դϴ�. ����: https://en.wikipedia.org/wiki/Euclidean_distance</summary>
		Euclidean,
		/// <summary>
		/// �޸���ƽ�� ������� �ʽ��ϴ�.
		/// �̷����ϸ� ��� ã�� �˰����� Dijkstra�� �˰������� ��ҵ˴ϴ�.
		/// �̰��� �Ϲ������� Dijkstra�� �˰��� ��� A* �˰����� ����ϱ� ������ ������ �����ϴ�.
		/// �ſ� Ư���� �׷��� (��: Civilization �Ǵ� Asteroids�� ���� wraparound playfield�� ���� ����)������ ��ΰ� ���Ե��� ���� �� �ֽ��ϴ�. 
		/// ���� A* �˰����� wraparound ��ũ�� ã�� �����Ƿ� �ش� ������ Ȯ������ ���� ���Դϴ�. 
		/// ����: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
		/// </summary>
		None
	}

	/// <summary>�����Ϳ��� �׷����� �ð�ȭ�ϴ� ���</summary>
	public enum GraphDebugMode
	{
		/// <summary>�׷����� ���� �ܻ����� �׸��ϴ�.</summary>
		SolidColor,
		/// <summary>
		/// ���������� ���� ����� G ������ ����Ͽ� �׷����� ��ĥ�մϴ�.
		/// G ������ ���� ��忡�� ������ �������� ����Դϴ�.
		/// ����: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		G,
		/// <summary>
		/// ���������� ���� ����� H ����(�޸���ƽ)�� ����Ͽ� �׷����� ��ĥ�մϴ�.
		/// H ������ ���� ��忡�� �������� ���� ����Դϴ�.
		/// ����: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		H,
		/// <summary>
		/// ���������� ���� ����� F ������ ����Ͽ� �׷����� ��ĥ�մϴ�.
		/// F ������ G ���� + H ���� �Ǵ� �ٸ� ���� ����� �� ����� ��Ÿ���ϴ�.
		/// ����: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		F,
		/// <summary>
		/// �� ����� �г�Ƽ�� ����Ͽ� �׷����� ��ĥ�մϴ�.
		/// �̰��� �±׿� ���� �߰��� �г�Ƽ�� ǥ������ �ʽ��ϴ�.
		/// �׷��� ������Ʈ �� ��� �г�Ƽ ���� ������ Ȯ���Ϸ��� �¶��� ������ �����Ͻʽÿ�.
		/// ����: <see cref="Pathfinding.GraphNode.Penalty"/>
		/// </summary>
		Penalty,
		/// <summary>
		/// �׷����� ����� ���� ��Ҹ� �ð�ȭ�մϴ�.
		/// ������ ������ ���� ���� ������ ������ ���� �ٸ� ��忡 ������ �� �ֽ��ϴ�.
		/// ����: <see cref="Pathfinding.HierarchicalGraph"/>
		/// ����: https://en.wikipedia.org/wiki/Connected_component_(graph_theory)
		/// </summary>
		Areas,
		/// <summary>
		/// �� ����� �±׸� ����Ͽ� �׷����� ��ĥ�մϴ�.
		/// ����: �±׸� ���� �¶��� ������ �����Ͻʽÿ�.
		/// ����: <see cref="Pathfinding.GraphNode.Tag"/>
		/// </summary>
		Tags,
		/// <summary>
		/// �׷����� ���� ������ �ð�ȭ�մϴ�.
		/// �̰��� �ַ� ���������� ���˴ϴ�.
		/// ����: <see cref="Pathfinding.HierarchicalGraph"/>
		/// </summary>
		HierarchicalNode,
	}

	/// <summary>����� ������ ��</summary>
	public enum ThreadCount {
		AutomaticLowLoad = -1,
		AutomaticHighLoad = -2,
		None = 0,
		One = 1,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight
	}

	/// <summary>���������� ������ ����� ���� ����</summary>
	public enum PathState {
		Created = 0,
		PathQueue = 1,
		Processing = 2,
		ReturnQueue = 3,
		Returned = 4
	}

	/// <summary>��� ��û�� ����</summary>
	public enum PathCompleteState
	{
		/// <summary>
		/// ���� ��ΰ� ������ �ʾҽ��ϴ�.
		/// ����: <see cref="Pathfinding.Path.IsDone()"/>
		/// </summary>
		NotCalculated = 0,
		/// <summary>
		/// ��� ����� �Ϸ�Ǿ����� �����߽��ϴ�.
		/// ����: <see cref="Pathfinding.Path.error"/>
		/// </summary>
		Error = 1,
		/// <summary>��� ����� ���������� �Ϸ�Ǿ����ϴ�.</summary>
		Complete = 2,
		/// <summary>
		/// ��ΰ� ���Ǿ����� �κ� ��θ� ã�� �� �־����ϴ�.
		/// ����: <see cref="Pathfinding.ABPath.calculatePartial"/>
		/// </summary>
		Partial = 3,
	}

	/// <summary>����� ����� �� ĳ������ ������ �����մϴ�.</summary>
	public enum CloseToDestinationMode
	{
		/// <summary>������ �Ÿ��� endReachedDistance(��κ��� �̵� ��ũ��Ʈ�� �ִ� �ʵ�) ���� ������ ĳ���ʹ� ������ ���� �����մϴ�.</summary>
		Stop,
		/// <summary>ĳ���ʹ� ����� ��Ȯ�� ��ġ�� �̵��մϴ�.</summary>
		ContinueToExactDestination,
	}

	/// <summary>���� ��ġ�� ���� ������ ��Ÿ���ϴ�.</summary>
	public enum Side : byte
	{
		/// <summary>���� ��Ȯ�� �� ���� �ִ� ���</summary>
		Colinear = 0,
		/// <summary>���� ���� ���ʿ� �ִ� ���</summary>
		Left = 1,
		/// <summary>���� ���� �����ʿ� �ִ� ���</summary>
		Right = 2
	}

	public enum InspectorGridHexagonNodeSize
	{
		/// <summary>�������� �� �ݴ� �� ������ �Ÿ��Դϴ�.</summary>
		Width,
		/// <summary>�������� �� �ݴ� ������ ������ �Ÿ��Դϴ�.</summary>
		Diameter,
		/// <summary>������ ���� ��� ũ���Դϴ�.</summary>
		NodeSize
	}

	public enum InspectorGridMode {
		Grid,
		IsometricGrid,
		Hexagonal,
		Advanced
	}

	/// <summary>
	/// ������Ʈ�� �̵��ϴ� ������ �����մϴ�.
	/// 3D ������ ��� ZAxisIsForward �ɼ��� ����ϴ� ���� �����ϴ�. �̰��� 3D ������ �����Դϴ�.
	/// 2D ������ ��� YAxisIsForward �ɼ��� ����ϴ� ���� �����ϴ�. �̰��� 2D ������ �����Դϴ�.
	/// </summary>
	public enum OrientationMode
	{
		ZAxisForward,
		YAxisForward,
	}

	#endregion
}

namespace Pathfinding.Util {
	/// <summary>�ڵ� ���Ÿ� �����մϴ�. �ڼ��� ������ ���� ��ũ�� �����ϼ���: https://docs.unity3d.com/Manual/ManagedCodeStripping.html</summary>
	public class PreserveAttribute : System.Attribute {
	}
}
