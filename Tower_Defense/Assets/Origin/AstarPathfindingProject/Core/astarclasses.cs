using UnityEngine;
using System.Collections.Generic;


// RVO 네임스페이스에 클래스가 없는 무료 버전에서 오류를 방지하기 위한 빈 네임스페이스 선언
namespace Pathfinding.RVO {}

namespace Pathfinding {
	using Pathfinding.Util;

#if UNITY_5_0
	/// <summary>Unity 5.0에서 사용되며, HelpURLAttribute는 Unity 5.1에서 처음 추가되었습니다.</summary>
	public class HelpURLAttribute : Attribute {
	}
#endif

	[System.Serializable]
	/// <summary>에디터 색상을 저장합니다.</summary>
	public class AstarColor {
		public Color _SolidColor; // 솔리드 컬러
		public Color _UnwalkableNode; // 걷기 불가능한 노드 컬러
		public Color _BoundsHandles; // 경계 핸들 컬러

		public Color _ConnectionLowLerp; // 연결 로우 러프 컬러
		public Color _ConnectionHighLerp; // 연결 하이 러프 컬러

		public Color _MeshEdgeColor; // 메시 엣지 컬러

		/// <summary>
		/// 사용자가 설정한 영역 색상을 보유합니다.
		/// 영역 색상을 얻으려면 GetAreaColor를 사용하세요.
		/// </summary>
		public Color[] _AreaColors;

		public static Color SolidColor = new Color(30 / 255f, 102 / 255f, 201 / 255f, 0.9F); // 솔리드 컬러 기본값
		public static Color UnwalkableNode = new Color(1, 0, 0, 0.5F); // 걷기 불가능한 노드 컬러 기본값
		public static Color BoundsHandles = new Color(0.29F, 0.454F, 0.741F, 0.9F); // 경계 핸들 컬러 기본값

		public static Color ConnectionLowLerp = new Color(0, 1, 0, 0.5F); // 연결 로우 러프 컬러 기본값
		public static Color ConnectionHighLerp = new Color(1, 0, 0, 0.5F); // 연결 하이 러프 컬러 기본값

		public static Color MeshEdgeColor = new Color(0, 0, 0, 0.5F); // 메시 엣지 컬러 기본값


		private static Color[] AreaColors = new Color[1];

		public static int ColorHash () {
			var hash = SolidColor.GetHashCode() ^ UnwalkableNode.GetHashCode() ^ BoundsHandles.GetHashCode() ^ ConnectionLowLerp.GetHashCode() ^ ConnectionHighLerp.GetHashCode() ^ MeshEdgeColor.GetHashCode();

			for (int i = 0; i < AreaColors.Length; i++) hash = 7*hash ^ AreaColors[i].GetHashCode();
			return hash;
		}

		/// <summary>
		/// 영역에 대한 색상을 반환합니다. 사용자가 설정한 색상과 계산된 색상을 모두 사용합니다.
		/// 사용자가 영역에 대한 색상을 설정한 경우 해당 색상을 사용하고, 그렇지 않은 경우 AstarMath.IntToColor를 사용하여 색상을 계산합니다.
		/// </summary>
		public static Color GetAreaColor (uint area) {
			if (area >= AreaColors.Length) return AstarMath.IntToColor((int)area, 1F);
			return AreaColors[(int)area];
		}

		/// <summary>
		/// 태그에 대한 색상을 반환합니다. 사용자가 설정한 색상과 계산된 색상을 모두 사용합니다.
		/// 사용자가 태그에 대한 색상을 설정한 경우 해당 색상을 사용하고, 그렇지 않은 경우 AstarMath.IntToColor를 사용하여 색상을 계산합니다.
		/// </summary>
		public static Color GetTagColor (uint tag) {
			if (tag >= AreaColors.Length) return AstarMath.IntToColor((int)tag, 1F);
			return AreaColors[(int)tag];
		}

		/// <summary>
		/// 모든 로컬 변수를 정적 변수로 밀어넣습니다.
		/// 이것은 Gizmo 렌더링 중에 색상에 쉽게 액세스하기 때문에 수행됩니다.
		/// 또한 성능에 긍정적인 영향을 미칩니다 (Gizmo 렌더링은 핫 코드입니다).
		/// 약간 더러운 방법이지만 기껏해야하는 일입니다.
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
	/// 그래프 레이 또는 라인 캐스트에서 반환되는 정보를 포함합니다.
	/// 이것은 <see cref="Pathfinding.IRaycastableGraph.Linecast"/> 메서드의 반환값입니다.
	/// 일부 멤버는 아무것도 히트하지 않았더라도 초기화됩니다. 각 멤버에 대한 자세한 내용은 개별 멤버 설명을 참조하십시오.
	/// </summary>
	public struct GraphHitInfo {
		/// <summary>
		/// 라인/레이의 시작점.
		/// 라인캐스트 메서드에 전달된 점은 가장 가까운 네비메쉬 상의 점에 대한 클램핑(clamping)이 될 것입니다.
		/// </summary>
		public Vector3 origin;
		/// <summary>
		/// 히트 지점.
		/// 장애물이 히트되지 않았다면 이 지점은 라인의 끝점으로 설정됩니다.
		/// </summary>
		public Vector3 point;
		/// <summary>
		/// 히트된 엣지를 포함한 노드.
		/// 라인캐스트가 아무 것도 히트하지 않았다면 이 값은 경로에 따른 마지막 노드로 설정됩니다 (최종점을 포함하는 노드).
		///
		/// 층별 그리드 그래프의 경우, 라인캐스트가 아무 것도 히트하지 않으면 (즉, 자유롭지 않은 시야가 없으면) 해당 그래프를 찾아갈 때 끝 노드의 X,Z 좌표로 설정됩니다
		/// 심지어 끝 노드가 다른 레벨에 있더라도 (예: 건물의 아래층 또는 위층에 위치한 경우) 이 때문에 아무런 노드 엣지도 실제로 히트하지 않으므로 이 필드는 여전히 null일 것입니다.
		/// </summary>
		public GraphNode node;
		/// <summary>
		/// 탄젠트 시작점. tangentOrigin과 tangent은 사실 히트된 엣지를 설명합니다.
		/// </summary>
		public Vector3 tangentOrigin;
		/// <summary>
		/// 히트된 엣지의 탄젠트.
		/// </summary>
		public Vector3 tangent;

		/// <summary><see cref="origin"/>에서 <see cref="point"/>까지의 거리</summary>
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

	/// <summary>가장 가까운 노드 제한 조건. <see cref="AstarPath.GetNearest"/> 함수가 반환하는 노드를 제한하는 조건입니다.</summary>
	public class NNConstraint
	{
		/// <summary>
		/// 검색 대상으로 사용할 그래프를 지정합니다.
		/// 이것은 비트마스크입니다. 비트 0은 그래프 목록의 첫 번째 그래프를 검색에 포함할지 여부를 지정하며, 비트 1은 두 번째 그래프를 포함할지 여부를 지정하고 이런 식입니다.
		/// <code>
		/// // 첫 번째 및 세 번째 그래프를 포함하도록 설정
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
		/// // 'My Grid Graph' 또는 'My Other Grid Graph'에서 가장 가까운 점을 찾습니다.
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		///
		/// 참고: 이것은 <see cref="AstarPath.GetNearest"/> 호출로 반환되는 노드에 영향을 미치지만, 노드 링크를 통해 유효하지 않은 그래프에 연결된 경우에는 검색될 수 있습니다.
		///
		/// 참조: <see cref="AstarPath.GetNearest"/>
		/// 참조: <see cref="SuitableGraph"/>
		/// 참조: 비트마스크 (작동 링크를 보려면 온라인 문서를 참조하세요)
		/// </summary>
		public GraphMask graphMask = -1;

		/// <summary>영역만 유효하도록 제한합니다. <see cref="area"/>가 0 미만이면 아무 영향을 미치지 않습니다.</summary>
		public bool constrainArea;

		/// <summary>제한할 영역 ID. 0 미만이거나 <see cref="constrainArea"/>가 false이면 아무 영향을 미치지 않습니다.</summary>
		public int area = -1;

		/// <summary>걷기 가능 또는 걷기 불가능한 노드로 제한합니다. </summary>
		public bool constrainWalkability = true;

		/// <summary>
		/// <see cref="constrainWalkability"/>가 활성화된 경우에만 걷기 가능한 노드 또는 걷기 불가능한 노드를 검색합니다.
		/// true이면 걷기 가능한 노드만 검색하고, 그렇지 않으면 걷기 불가능한 노드만 검색합니다.
		/// <see cref="constrainWalkability"/>가 false인 경우 아무런 영향을 미치지 않습니다.
		/// </summary>
		public bool walkable = true;

		/// <summary>
		/// 가능한 경우 XZ 축에서 검사하는 것이며, 모든 축에서 검사하는 것 대신 XZ 축에서 검사합니다.
		/// 네비게이션 메시/리캐스트 그래프는 이를 지원합니다.
		///
		/// 이것은 경사 면에서 중요할 수 있습니다. 아래 이미지에서 파란 점마다 가장 가까운 점에 대한 쿼리를 볼 수 있습니다.
		/// [온라인 문서에서 이미지 확인]
		///
		/// 네비게이션 메시/리캐스트 그래프는 또한 이에 대한 전역 옵션을 포함하고 있습니다: <see cref="Pathfinding.NavmeshBase.nearestSearchOnlyXZ"/>.
		/// </summary>
		public bool distanceXZ;

		/// <summary>
		/// 태그를 제한해야 하는지 여부를 설정합니다.
		/// 참조: <see cref="tags"/>
		/// </summary>
		public bool constrainTags = true;

		/// <summary>
		/// 특정 태그가 설정된 노드만 적합합니다.
		/// 이것은 비트마스크로, 비트 0은 태그 0이 좋음을 나타내고 비트 3은 태그 3이 좋음을 나타냅니다.
		/// 참조: <see cref="constrainTags"/>
		/// 참조: <see cref="graphMask"/>
		/// 참조: 비트마스크 (작동 링크를 보려면 온라인 문서에서 확인)
		/// </summary>
		public int tags = -1;

		/// <summary>
		/// 노드까지의 거리를 제한합니다.
		/// <see cref="AstarPath.maxNearestNodeDistance"/>에서 설정한 거리를 사용합니다.
		/// 이 값이 false인 경우 거리 제한을 완전히 무시합니다.
		///
		/// 거리 제한 내에서 적합한 노드가 없는 경우 결과로 null 노드를 반환합니다.
		/// 참고: 이 값은 이 클래스에서 사용되지 않으며, A* Inspector -> 설정 -> 최대 인접 노드 거리에서 사용됩니다.
		/// </summary>
		public bool constrainDistance = true;

		/// <summary>
		/// 이 NNConstraint의 규칙을 따르는 그래프인지 여부를 반환합니다.
		/// 주의: 이 함수를 사용하여 고려하는 그래프는 처음 31개 그래프만 고려됩니다.
		/// <see cref="graphMask"/>가 비트 31을 설정한 경우(즉, 마지막 비트마스크에 맞게되는 그래프) 그 이상의 모든 그래프도 적합하게 고려됩니다.
		/// </summary>
		public virtual bool SuitableGraph (int graphIndex, NavGraph graph) {
			return graphMask.Contains(graphIndex);
		}

		/// <summary>이 노드가 이 NNConstraint의 규칙을 따르는지 여부를 반환합니다.</summary>
		public virtual bool Suitable(GraphNode node)
		{
			if (constrainWalkability && node.Walkable != walkable) return false;

			if (constrainArea && area >= 0 && node.Area != area) return false;

			if (constrainTags && ((tags >> (int)node.Tag) & 0x1) == 0) return false;

			return true;
		}

		/// <summary>
		/// 기본 NNConstraint입니다.
		/// new NNConstraint ()와 동등합니다.
		/// 대부분의 경우 작동하는 설정을 가지고 있으며, 걷기 가능한 노드만 찾고 A* Inspector -> 설정 -> 최대 인접 노드 거리로 설정된 거리를 제한합니다.
		/// </summary>
		public static NNConstraint Default {
			get {
				return new NNConstraint();
			}
		}

		/// <summary>결과를 필터링하지 않는 제약 조건을 반환합니다.</summary>
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

		/// <summary>기본 생성자입니다. 속성 <see cref="Default"/>와 동일합니다.</summary>
		public NNConstraint()
		{
		}
	}

	/// <summary>
	/// 경로에서 시작 노드와 끝 노드에 대해 다른 논리를 사용할 수 있는 특수 NNConstraint입니다.
	/// Path.nnConstraint 필드에 PathNNConstraint를 할당하면 경로는 먼저 시작 노드를 검색한 다음 <see cref="SetStart"/>를 호출하고 끝 노드(다중 대상 경로의 경우 노드)를 검색합니다.
	/// 기본 PathNNConstraint는 끝점을 시작점과 동일한 영역에 위치하도록 제한합니다.
	/// </summary>
	public class PathNNConstraint : NNConstraint {
		public static new PathNNConstraint Default {
			get {
				return new PathNNConstraint {
						   constrainArea = true
				};
			}
		}

		/// <summary>시작 노드를 찾은 후에 호출됩니다. 경로에서 경로의 시작 및 끝 노드에 대해 다른 검색 논리를 가져오는 데 사용됩니다.</summary>
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
	/// 가장 가까운 노드 쿼리의 내부 결과입니다.
	/// 참조: NNInfo
	/// </summary>
	public struct NNInfoInternal
	{
		/// <summary>
		/// 찾은 가장 가까운 노드입니다.
		/// 이 노드는 전달된 NNConstraint에 의해 반드시 수락되지 않을 수 있습니다.
		/// 참조: constrainedNode
		/// </summary>
		public GraphNode node;

		/// <summary>
		/// 선택적으로 채워질 수 있습니다.
		/// 검색에서 추가 노력 없이 제약된 노드를 찾을 수 있는 경우 이를 채울 수 있습니다.
		/// </summary>
		public GraphNode constrainedNode;

		/// <summary>노드에서 가장 가까운 지점에 대한 위치입니다.</summary>
		public Vector3 clampedPosition;

		/// <summary>선택적으로 제약된 노드에 대한 클램프된 위치입니다.</summary>
		public Vector3 constClampedPosition;


		public NNInfoInternal (GraphNode node) {
			this.node = node;
			constrainedNode = null;
			clampedPosition = Vector3.zero;
			constClampedPosition = Vector3.zero;

			UpdateInfo();
		}

		/// <summary>노드 위치에서 <see cref="clampedPosition"/> 및 <see cref="constClampedPosition"/>을 업데이트합니다.</summary>
		public void UpdateInfo()
		{
			clampedPosition = node != null ? (Vector3)node.position : Vector3.zero;
			constClampedPosition = constrainedNode != null ? (Vector3)constrainedNode.position : Vector3.zero;
		}
	}

	/// <summary>가장 가까운 노드 쿼리 결과입니다.</summary>
	public struct NNInfo
	{
		/// <summary>가장 가까운 노드</summary>
		public readonly GraphNode node;

		/// <summary>
		/// 네비메시에서 가장 가까운 지점입니다.
		/// 이는 쿼리 위치를 <see cref="node"/>의 가장 가까운 지점으로 클램핑한 것입니다.
		/// </summary>
		public readonly Vector3 position;

		/// <summary>
		/// 네비메시에서 가장 가까운 지점입니다.
		/// 이 필드는 <see cref="position"/>으로 이름이 변경되었습니다.
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
		/// NNInfoInternal에 대한 생성자입니다.
		/// internalInfo의 정보를 기반으로 가장 가까운 노드와 위치를 설정합니다.
		/// </summary>
		public NNInfo(NNInfoInternal internalInfo)
		{
			node = internalInfo.node;
			position = internalInfo.clampedPosition;
		}

		/// <summary>
		/// NNInfo를 Vector3로 명시적으로 변환합니다.
		/// </summary>
		public static explicit operator Vector3(NNInfo ob)
		{
			return ob.position;
		}

		/// <summary>
		/// NNInfo를 GraphNode으로 명시적으로 변환합니다.
		/// </summary>
		public static explicit operator GraphNode(NNInfo ob)
		{
			return ob.node;
		}
	}

	/// <summary>
	/// 진행률 막대 또는 진행 정보에 대한 진행 정보입니다.
	/// 프로젝트의 스캔 기능에서 사용됩니다.
	/// 참조: <see cref="AstarPath.ScanAsync"/>
	/// </summary>
	public struct Progress
	{
		/// <summary>0에서 1 사이의 현재 진행률</summary>
		public readonly float progress;
		/// <summary>현재 수행 중인 작업에 대한 설명</summary>
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
	/// 런타임 중에 업데이트할 수 있는 그래프입니다.
	/// </summary>
	public interface IUpdatableGraph
	{
		/// <summary>
		/// 지정된 <see cref="GraphUpdateObject"/>를 사용하여 영역을 업데이트합니다.
		///
		/// 구현자에게 유의할 사항:
		/// 이 함수는 다음과 같은 순서로 작동해야 합니다:
		/// -# 각 노드에 대해 GUO의 o.WillUpdateNode를 호출해야 합니다. 노드에 대한 변경 사항을 수행하기 전에 호출되어야 하며, 노드에 대한 변경 사항을 수행하기 전에 호출해야 합니다.
		/// -# 걷기 가능성을 사용하거나 GridGraph와 함께 사용되는 usePhysics 플래그와 같은 특별한 설정을 사용하여 걷기 가능성을 업데이트합니다.
		/// -# GUO와 함께 업데이트해야 하는 각 노드에 대해 Apply를 호출합니다.
		/// -# 적절한 경우 연결성 정보를 업데이트합니다(GridGraph는 연결성을 업데이트하지만 대부분의 다른 그래프는 나중에 연결성을 복원할 수 없으므로 업데이트하지 않습니다).
		/// </summary>
		void UpdateArea(GraphUpdateObject o);

		/// <summary>
		/// 업데이트를 시작하기 전에 Unity 스레드에서 호출될 수 있습니다.
		/// 참조: CanUpdateAsync
		/// </summary>
		void UpdateAreaInit(GraphUpdateObject o);

		/// <summary>
		/// 업데이트를 실행한 후에 Unity 스레드에서 호출될 수 있습니다.
		/// 참조: CanUpdateAsync
		/// </summary>
		void UpdateAreaPost(GraphUpdateObject o);

		GraphUpdateThreading CanUpdateAsync(GraphUpdateObject o);
	}

	/// <summary>그래프 업데이트가 적용되었는지 여부에 대한 정보</summary>
	public enum GraphUpdateStage
	{
		/// <summary>
		/// 그래프 업데이트 개체가 생성되었지만 아직 아무 작업에 사용되지 않았습니다.
		/// 이 값이 기본값입니다.
		/// </summary>
		Created,
		/// <summary>그래프 업데이트가 경로 찾기 시스템으로 전송되어 그래프에 적용 예정입니다.</summary>
		Pending,
		/// <summary>그래프 업데이트가 모든 그래프에 적용되었습니다.</summary>
		Applied,
		/// <summary>
		/// 그래프 업데이트가 중단되었으며 적용되지 않을 것입니다.
		/// 이것은 그래프 업데이트가 적용 예정이었지만 그래프 업데이트가 대기열에 있을 때 AstarPath 컴포넌트가 파괴된 경우 발생할 수 있습니다.
		/// </summary>
		Aborted,
	}

	/// <summary>
	/// 그래프의 특정 영역 내의 노드를 업데이트하기 위해 사용되는 설정의 컬렉션을 나타냅니다.
	/// 참조: AstarPath.UpdateGraphs
	/// 참조: 그래프 업데이트 (작동 링크를 보려면 온라인 문서에서 확인)
	/// </summary>
	public class GraphUpdateObject
	{
		/// <summary>
		/// 노드를 업데이트할 경계입니다.
		/// 월드 공간에서 정의됩니다.
		/// </summary>
		public Bounds bounds;

		/// <summary>
		/// 이 GUO가 적용된 후에 홍수 채우기를 수행할지 여부를 제어합니다.
		/// 이 기능을 비활성화하면 성능 향상을 얻을 수 있지만 주의해서 사용해야 합니다.
		/// GUO가 걷기 가능성이나 연결성을 수정하지 않을 것이 확실한 경우에만 false로 설정할 수 있습니다.
		/// 예를 들어 노드의 페널티 값을 업데이트하는 경우 false로 설정하면 처리량을 절약할 수 있습니다. 특히 큰 그래프에서 유용합니다.
		/// 참고: 이 값을 false로 설정하면 걷기 가능성이 변경되었더라도 경로가 실패하는 경우가 발생할 수 있거나 경로가 없음에도 불구하고 전체 그래프에서 경로를 검색하려고 시도하면서 많은 처리량을 사용할 수 있습니다.
		///
		/// 기본 GraphUpdateObject (파생 클래스가 아닌 경우)를 사용하는 경우 이것이 홍수 채우기가 필요한지 여부를 빠르게 확인하는 방법은 
		/// <see cref="modifyWalkability"/>가 true이거나 <see cref="updatePhysics"/>가 true인지 확인하는 것입니다.
		/// 폐기됨: 더 이상 필요하지 않음
		/// </summary>
		[System.Obsolete("Not necessary anymore")]
		public bool requiresFloodFill { set { } }

		/// <summary>
		/// 노드를 업데이트할 때 물리 검사를 사용합니다.
		/// GridGraph를 업데이트하고 updatePhysics가 true로 설정된 경우, 노드의 위치 및 걷기 가능성이 "충돌 테스트" 및 "높이 테스트"에서 설정에 따라 물리 검사를 사용하여 업데이트됩니다.
		///
		/// PointGraph를 업데이트하고 이 값을 true로 설정하면 그래프를 통과하는 모든 연결을 다시 평가합니다.
		///
		/// 이 값은 <see cref="modifyWalkability"/>가 켜져 있는 경우 GridGraph를 업데이트할 때는 아무 영향을 미치지 않습니다.
		/// <see cref="updatePhysics"/>와 <see cref="modifyWalkability"/>를 결합해서는 안 됩니다.
		///
		/// RecastGraph의 경우 이 값을 활성화하면 경계를 교차하는 모든 타일을 완전히 다시 계산합니다.
		/// 이것은 상당히 느립니다(하지만 강력합니다). 기존 노드의 페널티만 업데이트하려면 비활성화된 상태로 둘 수 있습니다.
		/// </summary>
		public bool updatePhysics = true;

		/// <summary>
		/// GridGraph의 경우 노드를 업데이트할 때 초기 값으로 페널티를 재설정합니다.
		/// 그래프를 업데이트할 때 페널티를 유지하려면 이 옵션을 비활성화할 수 있습니다.
		///
		/// 아래 이미지는 서로 겹치는 두 개의 그래프 업데이트 개체를 보여줍니다. 오른쪽 개체가 왼쪽 개체보다 먼저 적용되었습니다. 두 개의 개체는 모두 노드의 페널티를 어느 정도 늘리도록 설정되었습니다.
		///
		/// 첫 번째 이미지는 resetPenaltyOnPhysics가 false인 경우 결과를 보여줍니다. 두 개의 페널티가 올바르게 추가됩니다.
		///
		/// 두 번째 이미지는 resetPenaltyOnPhysics를 true로 설정한 경우를 보여줍니다. 첫 번째 GUO가 올바르게 적용되고, 그런 다음 두 번째 GUO(왼쪽 개체)가 적용되어 페널티를 먼저 재설정하고 노드에 페널티를 추가합니다. 
		/// 결과적으로 두 GUO의 페널티가 더해지지 않습니다. 테두리의 녹색 패치는 물리 계산(노드의 위치 변경, 장애물 확인 등)이 경계의 원래 GUO 경계에서 지정된 값(Grid Graph -> 충돌 테스트 -> 직경 설정)으로 인해 
		/// 더 큰 영역에 영향을 미치기 때문에 발생합니다(이 값으로 확장됨). 따라서 일부 추가 노드의 페널티가 재설정됩니다.
		/// </summary>
		public bool resetPenaltyOnPhysics = true;


		/// <summary>
		/// 그리드 그래프용 Erosion 업데이트.
		/// 활성화되면 GUO(GraphUpdateObject)가 적용된 후 그리드 그래프의 Erosion(침식)이 재계산됩니다.
		///
		/// 아래 이미지에서 다양한 효과를 볼 수 있습니다.
		/// 첫 번째 이미지는 GUO가 적용되지 않았을 때의 그래프를 보여줍니다. 파란 상자는 그래프에서 장애물로 인식되지 않으며,
		/// 주변에 비포효한 노드가 있는 이유는 높이 차이 때문입니다 (노드는 상자 위에 배치됨) 따라서 Erosion(침식)이 적용됩니다 (이 그래프에서는 Erosion 값으로 2를 사용함).
		/// 주황색 상자는 장애물로 인식되어 주변의 비포효한 노드 영역이 약간 더 큽니다. Erosion과 충돌 모두로 인해 노드가 비포효합니다.
		/// 사용된 GUO는 모든 노드의 걷기 가능성을 true로 설정하는 것뿐입니다.
		///
		/// [온라인 문서를 열어 이미지 보기]
		///
		/// updateErosion이 True일 때 파란 상자 주변에 여전히 비포효한 노드가 있는 이유는 여전히 높이 차이가 있기 때문입니다.
		/// 주황색 상자는 높이 차이가 없으므로 모든 노드가 걷기 가능합니다.
		/// 
		/// updateErosion이 False일 때는 이 예제에서 모든 노드의 걷기 가능성이 간단히 걷기 가능하도록 설정됩니다.
		///
		/// 참조: Pathfinding.GridGraph
		/// </summary>
		public bool updateErosion = true;

		/// <summary>
		/// 사용할 NNConstraint(노드 제약 조건).
		/// NNConstraint.SuitableGraph 함수가 NNConstraint에서 호출됩니다.
		/// 어떤 그래프를 업데이트할지 필터링하는 데 사용됩니다.
		/// 참고: NNConstraint.SuitableGraph 함수는 A* Pathfinding Project Pro에서만 사용 가능하므로 무료 버전에서는 이 변수가 실제로 아무 영향을 미치지 않습니다.
		/// </summary>
		public NNConstraint nnConstraint = NNConstraint.None;

		/// <summary>
		/// 노드에 추가할 패널티(처벌).
		/// 1000의 패널티는 1개의 월드 유닛을 이동하는 비용과 동일합니다.
		/// </summary>
		public int addPenalty;

		/// <summary>
		/// 참일 경우, 모든 노드의 'walkable' 변수가 'setWalkability'로 설정됩니다.
		/// 'updatePhysics'와 함께 사용하지 않는 것이 좋습니다. 그렇게 하면 'updatePhysics'가 계산한 것을 덮어쓸 수 있기 때문입니다.
		/// </summary>
		public bool modifyWalkability;

		/// <summary>'modifyWalkability'가 참일 경우, 노드의 'walkable' 변수는 이 값으로 설정됩니다.</summary>
		public bool setWalkability;

		/// <summary>'modifyTag'가 참일 경우, 모든 노드의 'tag'가 'setTag'로 설정됩니다.</summary>
		public bool modifyTag;

		/// <summary>'modifyTag'가 참일 경우, 모든 노드의 'tag'가 이 값으로 설정됩니다.</summary>
		public int setTag;

		/// <summary>
		/// 노드가 변경된 것을 추적하고 백업 데이터를 저장하는 데 사용됩니다.
		/// 필요한 경우 변경 내용을 되돌릴 때 내부적으로 사용됩니다.
		/// </summary>
		public bool trackChangedNodes;

		/// <summary>
		/// 이 GraphUpdateObject에 의해 업데이트된 노드들입니다.
		/// 'trackChangedNodes'가 참일 경우에만 채워집니다.
		/// 참고: 그래프 업데이트 오브젝트가 적용되기까지 몇 프레임이 걸릴 수 있습니다.
		/// 즉시 이 정보가 필요한 경우, 'AstarPath.FlushGraphUpdates'를 사용하세요.
		/// </summary>
		public List<GraphNode> changedNodes;
		private List<uint> backupData;
		private List<Int3> backupPositionData;

		/// <summary>
		/// Bounds 개체가 충분한 정확성을 제공하지 않는 경우 모양을 지정할 수 있습니다.
		/// 모양이 설정되면 bounds가 모양을 감싸도록 설정해야 합니다.
		/// bounds는 노드를 빠르게 확인하는 초기 확인 용도로 사용됩니다.
		/// </summary>
		public GraphUpdateShape shape;

		/// <summary>
		/// 그래프 업데이트의 적용 여부 정보입니다.
		/// 열거형(참조: STAGE_CREATED 및 연관된 상수) 또는 이 그래프 업데이트를 적용 대기 중인 그래프의 개수를 나타내는 음수가 될 수 있습니다.
		/// </summary>
		internal int internalStage = STAGE_CREATED;


		internal const int STAGE_CREATED = -1;
		internal const int STAGE_PENDING = -2;
		internal const int STAGE_ABORTED = -3;
		internal const int STAGE_APPLIED = 0;

		/// <summary>그래프 업데이트의 적용 여부 정보</summary>
		public GraphUpdateStage stage {
			get {
				switch (internalStage) {
				case STAGE_CREATED:
					return GraphUpdateStage.Created;
				case STAGE_APPLIED:
					return GraphUpdateStage.Applied;
				case STAGE_ABORTED:
					return GraphUpdateStage.Aborted;
					// 양수는 현재 적용 중임을 의미하므로 대기 중이기도 합니다.
					default:
				case STAGE_PENDING:
					return GraphUpdateStage.Pending;
				}
			}
		}

		/// <summary>
		/// 이 GUO를 사용하여 업데이트되는 각 노드에 대해 호출해야 합니다.
		/// 참조: trackChangedNodes
		/// </summary>
		/// <param name="node">필드를 저장할 노드. null인 경우 아무 작업도 수행되지 않습니다.</param>
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
		/// 백업에서 패널티 및 플래그(걷기 가능성 포함)를 복원합니다.
		/// trackChangedNodes가 true로 설정된 경우에만 데이터가 저장됩니다.
		///
		/// 참고: 모든 데이터가 저장되는 것은 아닙니다. 저장된 데이터에는 패널티, 걷기 가능성, 태그, 영역, 위치 및 그리드 그래프의 경우 (계층 구조화되지 않은 경우) 연결 데이터도 포함됩니다.
		///
		/// 이 메서드는 그래프를 수정하므로 그래프를 수정하는 것이 안전한 상태에서 호출되어야 합니다.
		/// 예를 들어 아래 예제에서와 같이 작업 항목 내에서 호출되어야 합니다.
		///
		/// 참조: MiscSnippets.cs GraphUpdateObject.RevertFromBackup
		///
		/// 참조: blocking (온라인 문서에서 작동 링크 보기)
		/// 참조: Pathfinding.PathUtilities.UpdateGraphsNoBlock
		/// </summary>
		public virtual void RevertFromBackup()
		{
			if (trackChangedNodes) {
				if (changedNodes == null) return;

				int counter = 0;
				for (int i = 0; i < changedNodes.Count; i++) {
					changedNodes[i].Penalty = backupData[counter];
					counter++;
					// 플래그를 복원하지만 HierarchicalNodeIndex는 내부 데이터 구조를 망칠 수 있으므로 복원하지 않습니다.
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
				throw new System.InvalidOperationException("변경된 노드가 추적되지 않았으므로 백업에서 복원할 수 없습니다. 업데이트를 적용하기 전에 trackChangedNodes를 true로 설정하세요.");
			}
		}

		/// <summary>이 GUO의 설정을 사용하여 지정된 노드를 업데이트합니다.</summary>
		public virtual void Apply(GraphNode node)
		{
			if (shape == null || shape.Contains(node)) {
				// 패널티 및 걷기 가능성 업데이트
				node.Penalty = (uint)(node.Penalty+addPenalty);
				if (modifyWalkability) {
					node.Walkable = setWalkability;
				}

				// 태그 업데이트
				if (modifyTag) node.Tag = (uint)setTag;
			}
		}

		public GraphUpdateObject () {
		}

		/// <summary>지정된 바운딩 박스로 새 GUO를 생성합니다.</summary>
		public GraphUpdateObject (Bounds b) {
			bounds = b;
		}
	}

	/// <summary>그래프 공간에서 월드 공간으로의 정의된 변환을 가지는 그래프입니다.</summary>
	public interface ITransformedGraph
	{
		GraphTransform transform { get; }
	}


	/// <summary>Linecast 메서드를 지원하는 그래프입니다.</summary>
	public interface IRaycastableGraph {
		bool Linecast(Vector3 start, Vector3 end);
		bool Linecast(Vector3 start, Vector3 end, GraphNode hint);
		bool Linecast(Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit);
		bool Linecast(Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace);
		bool Linecast(Vector3 start, Vector3 end, out GraphHitInfo hit, List<GraphNode> trace, System.Func<GraphNode, bool> filter);
	}

	/// <summary>
	/// 정수 좌표 범위를 사용하는 사각형입니다.
	/// 포함적인 좌표 범위를 사용합니다.
	///
	/// 거의 UnityEngine.Rect와 유사하게 동작하지만 정수 좌표를 사용합니다.
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
		/// 이 사각형이 유효한지 여부를 반환합니다.
		/// 유효하지 않은 사각형은 예를 들어 xmin > xmax인 경우입니다.
		/// 영역이 0인 사각형도 유효하지 않습니다.
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
		/// 두 사각형의 교차 영역을 반환합니다.
		/// 교차 영역은 두 사각형 모두 안에 있는 영역입니다.
		/// 두 사각형이 교차하지 않으면 유효하지 않은 사각형이 반환됩니다.
		/// 참조: IsValid
		/// </summary>
		public static IntRect Intersection (IntRect a, IntRect b) {
			return new IntRect(
				System.Math.Max(a.xmin, b.xmin),
				System.Math.Max(a.ymin, b.ymin),
				System.Math.Min(a.xmax, b.xmax),
				System.Math.Min(a.ymax, b.ymax)
				);
		}

		/// <summary>두 개의 사각형이 교차하는지 여부를 반환합니다.</summary>
		public static bool Intersects(IntRect a, IntRect b)
		{
			return !(a.xmin > b.xmax || a.ymin > b.ymax || a.xmax < b.xmin || a.ymax < b.ymin);
		}

		/// <summary>
		/// 두 개의 사각형을 포함하는 새로운 사각형을 반환합니다.
		/// 이 사각형은 경우에 따라 두 입력 사각형 밖의 영역을 포함할 수 있습니다.
		/// </summary>
		public static IntRect Union (IntRect a, IntRect b) {
			return new IntRect(
				System.Math.Min(a.xmin, b.xmin),
				System.Math.Min(a.ymin, b.ymin),
				System.Math.Max(a.xmax, b.xmax),
				System.Math.Max(a.ymax, b.ymax)
				);
		}

		/// <summary>포인트를 포함하는 새로운 IntRect를 반환합니다.</summary>
		public IntRect ExpandToContain (int x, int y) {
			return new IntRect(
				System.Math.Min(xmin, x),
				System.Math.Min(ymin, y),
				System.Math.Max(xmax, x),
				System.Math.Max(ymax, y)
				);
		}

		/// <summary>모든 방향으로 범위를 확장한 새로운 사각형을 반환합니다.</summary>
		/// <param name="range">얼마나 확장할 지를 나타내는 값입니다. 음수 값도 허용됩니다.</param>
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

		/// <summary>사각형을 나타내는 디버그 라인을 그립니다.</summary>
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
	/// 그래프들의 비트마스크를 보유합니다.
	/// 이 비트마스크는 최대 32개의 그래프를 보유할 수 있습니다.
	///
	/// 비트마스크는 정수로 암묵적으로 변환 및 변환할 수 있습니다.
	///
	/// <code>
	/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
	/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
	///
	/// NNConstraint nn = NNConstraint.Default;
	///
	/// nn.graphMask = mask1 | mask2;
	///
	/// // 'My Grid Graph' 또는 'My Other Grid Graph' 중 하나에 속하는 somePoint에 가장 가까운 노드를 찾습니다.
	/// var info = AstarPath.active.GetNearest(somePoint, nn);
	/// </code>
	///
	/// 참조: bitmasks (작동 링크를 위해 온라인 문서에서 보기)
	/// </summary>
	[System.Serializable]
	public struct GraphMask {
		/// <summary>마스크를 나타내는 비트마스크입니다.</summary>
		public int value;

		/// <summary>모든 그래프를 포함하는 마스크입니다.</summary>
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

		/// <summary>두 마스크를 결합하여 교차 부분을 형성합니다.</summary>
		public static GraphMask operator &(GraphMask lhs, GraphMask rhs)
		{
			return new GraphMask(lhs.value & rhs.value);
		}

		/// <summary>두 마스크를 결합하여 합집합을 형성합니다.</summary>
		public static GraphMask operator |(GraphMask lhs, GraphMask rhs)
		{
			return new GraphMask(lhs.value | rhs.value);
		}

		/// <summary>마스크를 반전합니다.</summary>
		public static GraphMask operator ~(GraphMask lhs)
		{
			return new GraphMask(~lhs.value);
		}

		/// <summary>이 마스크가 주어진 그래프 인덱스를 포함하는지 여부를 반환합니다.</summary>
		public bool Contains(int graphIndex)
		{
			return ((value >> graphIndex) & 1) != 0;
		}

		/// <summary>주어진 그래프를 포함하는 비트마스크를 반환합니다.</summary>
		public static GraphMask FromGraph(NavGraph graph)
		{
			return 1 << (int)graph.graphIndex;
		}

		public override string ToString () {
			return value.ToString();
		}

		/// <summary>
		/// 주어진 이름을 가진 첫 번째 그래프를 포함하는 비트마스크를 반환합니다.
		/// <code>
		/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
		/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
		///
		/// NNConstraint nn = NNConstraint.Default;
		///
		/// nn.graphMask = mask1 | mask2;
		///
		/// // 'My Grid Graph' 또는 'My Other Grid Graph' 중 하나에 속하는 somePoint에 가장 가까운 노드를 찾습니다.
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		/// </summary>
		public static GraphMask FromGraphName (string graphName) {
			var graph = AstarData.active.data.FindGraph(g => g.name == graphName);

			if (graph == null) throw new System.ArgumentException("'" + graphName + "' 이름을 가진 그래프를 찾을 수 없습니다.");
			return FromGraph(graph);
		}
	}

	#region Delegates

	/* Path 객체를 매개변수로 사용하는 대리자입니다.
	 * 이것은 경로 계산이 완료되었을 때 콜백에 사용됩니다.
	 * 예제 함수:
	 * \snippet MiscSnippets.cs OnPathDelegate
	 */
	public delegate void OnPathDelegate(Path p);

	public delegate void OnGraphDelegate(NavGraph graph);

	public delegate void OnScanDelegate(AstarPath script);

	/// <summary>
	/// 스캔 상태에 대한 대리자입니다.
	/// 이 대리자는 더 이상 사용되지 않습니다. (Deprecated)
	/// </summary>
	/// <param name="progress">진행 상태 정보</param>
	public delegate void OnScanStatus(Progress progress);

	#endregion

	#region Enums

	/// <summary>
	/// 경로 업데이트 스레딩 설정
	/// </summary>
	public enum GraphUpdateThreading
	{
		/// <summary>
		/// Unity 스레드에서 UpdateArea를 호출합니다. 기본값입니다. SeparateThread와 호환되지 않습니다.
		/// </summary>
		UnityThread = 0,
		/// <summary>UpdateArea를 별도의 스레드에서 호출합니다. UnityThread와 호환되지 않습니다.</summary>
		SeparateThread = 1 << 0,
		/// <summary>모든 것 이전에 Unity 스레드에서 UpdateAreaInit를 호출합니다.</summary>
		UnityInit = 1 << 1,
		/// <summary>
		/// 모든 것 이후에 Unity 스레드에서 UpdateAreaPost를 호출합니다.
		/// 이것은 SeparateThread와 함께 사용되며 다른 스크립트가 그래프를 사용할 때 그래프를 수정하지 않고 다중 스레드 계산 결과를 적용하기 위해 사용됩니다 (예: GetNearest 호출).
		/// </summary>
		UnityPost = 1 << 2,
		/// <summary>SeparateThread 및 UnityInit의 조합</summary>
		SeparateAndUnityInit = SeparateThread | UnityInit
	}


	/// <summary>시스템에서 경로 결과를 기록하는 방법</summary>
	public enum PathLog
	{
		/// <summary>아무것도 기록하지 않습니다. 릴리스에 권장됩니다. 경로 결과 기록에는 성능 오버헤드가 있습니다.</summary>
		None,
		/// <summary>경로에 대한 기본 정보를 기록합니다.</summary>
		Normal,
		/// <summary>추가 정보를 포함합니다.</summary>
		Heavy,
		/// <summary>무거운 정보와 동일하지만 게임 내 GUI를 사용하여 정보를 표시합니다.</summary>
		InGame,
		/// <summary>노멀과 동일하지만 오류가 발생한 경로만 기록합니다.</summary>
		OnlyErrors
	}

	/// <summary>
	/// 경로 찾기 중 대상까지 이동하는 비용을 어떻게 추정할지를 결정합니다.
	/// 
	/// 휴리스틱(heuristic)은 현재 노드에서 목표까지의 추정 비용입니다.
	/// 서로 다른 휴리스틱은 대부분의 경우 성능이 거의 동일하지만 휴리스틱을 사용하지 않는 옵션(<see cref="None"/>)은 일반적으로 매우 느립니다.
	/// 아래 이미지에서는 8-연결 그리드와 4-연결 그리드에 대한 다양한 휴리스틱 옵션을 비교한 것입니다.
	/// 모든 경로가 녹색 영역 내에 있으며, 이 영역 내의 경로 중 어느 것이 선택되는지에 대한 차이점만 있습니다.
	/// Diagonal Manhattan 및 Manhattan 옵션은 8-연결 그리드에서 매우 다르게 작동하는 것처럼 보이지만 아주 작은 반올림 오차 때문에 다르게 작동합니다. 보통 8-연결 그리드에서 거의 동일하게 작동합니다.
	/// 
	/// 일반적으로 4-연결 그리드에 대한 Manhattan 옵션을 사용하는 것이 좋습니다. 이것은 4-연결 그리드에서의 실제 거리입니다.
	/// 8-연결 그리드에 대한 Diagonal Manhattan 옵션이 수학적으로 가장 정확한 옵션입니다. 그러나 Euclidean 옵션이 종종 선호되며, 특히 수정기(modifier)를 사용하여 경로를 단순화하는 경우에는 Euclidean 옵션이 더 나을 수 있습니다.
	/// 그리드 기반이 아닌 모든 그래프에 대해서는 Euclidean 옵션이 가장 적합합니다.
	/// 
	/// 참조: <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">위키백과: A* 검색 알고리즘</a>
	/// </summary>
	public enum Heuristic {
		/// <summary>맨해튼 거리입니다. 참조: https://en.wikipedia.org/wiki/Taxicab_geometry</summary>
		Manhattan,
		/// <summary>
		/// 맨해튼 거리이지만 대각선 이동도 허용합니다.
		/// 참고: 현재 XZ 평면에 대해 하드 코딩되어 있으며 2D 게임 (즉, 2D 게임)에서 사용하려고하면 맨해튼 거리와 동일합니다.
		/// </summary>
		DiagonalManhattan,
		/// <summary>일반적인 거리입니다. 참조: https://en.wikipedia.org/wiki/Euclidean_distance</summary>
		Euclidean,
		/// <summary>
		/// 휴리스틱을 사용하지 않습니다.
		/// 이렇게하면 경로 찾기 알고리즘이 Dijkstra의 알고리즘으로 축소됩니다.
		/// 이것은 일반적으로 Dijkstra의 알고리즘 대신 A* 알고리즘을 사용하기 때문에 성능이 느립니다.
		/// 매우 특수한 그래프 (예: Civilization 또는 Asteroids와 같은 wraparound playfield를 가진 세계)에서는 경로가 포함되지 않을 수 있습니다. 
		/// 보통 A* 알고리즘은 wraparound 링크를 찾지 않으므로 해당 방향을 확인하지 않을 것입니다. 
		/// 참조: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
		/// </summary>
		None
	}

	/// <summary>에디터에서 그래프를 시각화하는 방법</summary>
	public enum GraphDebugMode
	{
		/// <summary>그래프를 단일 단색으로 그립니다.</summary>
		SolidColor,
		/// <summary>
		/// 마지막으로 계산된 경로의 G 점수를 사용하여 그래프를 색칠합니다.
		/// G 점수는 시작 노드에서 지정된 노드까지의 비용입니다.
		/// 참조: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		G,
		/// <summary>
		/// 마지막으로 계산된 경로의 H 점수(휴리스틱)를 사용하여 그래프를 색칠합니다.
		/// H 점수는 현재 노드에서 대상까지의 추정 비용입니다.
		/// 참조: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		H,
		/// <summary>
		/// 마지막으로 계산된 경로의 F 점수를 사용하여 그래프를 색칠합니다.
		/// F 점수는 G 점수 + H 점수 또는 다른 말로 경로의 총 비용을 나타냅니다.
		/// 참조: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		F,
		/// <summary>
		/// 각 노드의 패널티를 사용하여 그래프를 색칠합니다.
		/// 이것은 태그에 의해 추가된 패널티를 표시하지 않습니다.
		/// 그래프 업데이트 및 노드 패널티 관련 정보를 확인하려면 온라인 문서를 참조하십시오.
		/// 참조: <see cref="Pathfinding.GraphNode.Penalty"/>
		/// </summary>
		Penalty,
		/// <summary>
		/// 그래프의 연결된 구성 요소를 시각화합니다.
		/// 동일한 색상을 가진 노드는 동일한 색상을 가진 다른 노드에 도달할 수 있습니다.
		/// 참조: <see cref="Pathfinding.HierarchicalGraph"/>
		/// 참조: https://en.wikipedia.org/wiki/Connected_component_(graph_theory)
		/// </summary>
		Areas,
		/// <summary>
		/// 각 노드의 태그를 사용하여 그래프를 색칠합니다.
		/// 참조: 태그를 위한 온라인 문서를 참조하십시오.
		/// 참조: <see cref="Pathfinding.GraphNode.Tag"/>
		/// </summary>
		Tags,
		/// <summary>
		/// 그래프의 계층 구조를 시각화합니다.
		/// 이것은 주로 내부적으로 사용됩니다.
		/// 참조: <see cref="Pathfinding.HierarchicalGraph"/>
		/// </summary>
		HierarchicalNode,
	}

	/// <summary>사용할 스레드 수</summary>
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

	/// <summary>파이프라인 내에서 경로의 내부 상태</summary>
	public enum PathState {
		Created = 0,
		PathQueue = 1,
		Processing = 2,
		ReturnQueue = 3,
		Returned = 4
	}

	/// <summary>경로 요청의 상태</summary>
	public enum PathCompleteState
	{
		/// <summary>
		/// 아직 경로가 계산되지 않았습니다.
		/// 참조: <see cref="Pathfinding.Path.IsDone()"/>
		/// </summary>
		NotCalculated = 0,
		/// <summary>
		/// 경로 계산이 완료되었지만 실패했습니다.
		/// 참조: <see cref="Pathfinding.Path.error"/>
		/// </summary>
		Error = 1,
		/// <summary>경로 계산이 성공적으로 완료되었습니다.</summary>
		Complete = 2,
		/// <summary>
		/// 경로가 계산되었지만 부분 경로만 찾을 수 있었습니다.
		/// 참조: <see cref="Pathfinding.ABPath.calculatePartial"/>
		/// </summary>
		Partial = 3,
	}

	/// <summary>대상이 가까울 때 캐릭터의 동작을 결정합니다.</summary>
	public enum CloseToDestinationMode
	{
		/// <summary>대상과의 거리가 endReachedDistance(대부분의 이동 스크립트에 있는 필드) 내에 있으면 캐릭터는 가능한 빨리 정지합니다.</summary>
		Stop,
		/// <summary>캐릭터는 대상의 정확한 위치로 이동합니다.</summary>
		ContinueToExactDestination,
	}

	/// <summary>점이 위치한 선의 한쪽을 나타냅니다.</summary>
	public enum Side : byte
	{
		/// <summary>점이 정확히 선 위에 있는 경우</summary>
		Colinear = 0,
		/// <summary>점이 선의 왼쪽에 있는 경우</summary>
		Left = 1,
		/// <summary>점이 선의 오른쪽에 있는 경우</summary>
		Right = 2
	}

	public enum InspectorGridHexagonNodeSize
	{
		/// <summary>육각형의 두 반대 변 사이의 거리입니다.</summary>
		Width,
		/// <summary>육각형의 두 반대 꼭지점 사이의 거리입니다.</summary>
		Diameter,
		/// <summary>격자의 원시 노드 크기입니다.</summary>
		NodeSize
	}

	public enum InspectorGridMode {
		Grid,
		IsometricGrid,
		Hexagonal,
		Advanced
	}

	/// <summary>
	/// 에이전트가 이동하는 방향을 결정합니다.
	/// 3D 게임의 경우 ZAxisIsForward 옵션을 사용하는 것이 좋습니다. 이것은 3D 게임의 관행입니다.
	/// 2D 게임의 경우 YAxisIsForward 옵션을 사용하는 것이 좋습니다. 이것은 2D 게임의 관행입니다.
	/// </summary>
	public enum OrientationMode
	{
		ZAxisForward,
		YAxisForward,
	}

	#endregion
}

namespace Pathfinding.Util {
	/// <summary>코드 제거를 방지합니다. 자세한 내용은 다음 링크를 참조하세요: https://docs.unity3d.com/Manual/ManagedCodeStripping.html</summary>
	public class PreserveAttribute : System.Attribute {
	}
}
