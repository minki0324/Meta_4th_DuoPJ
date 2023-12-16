using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.RVO;
	using Pathfinding.Util;

	/// <summary>
	/// 패스를 따라 이동하는 AI입니다.
	/// 이 AI는 A* Pathfinding Project와 함께 제공되는 기본 이동 스크립트로, 게임 내 캐릭터의 이동을 설정하기 쉽게 도와주는 스크립트입니다.
	/// 이 스크립트는 다양한 유닛 유형에 잘 작동하지만 가장 높은 성능이 필요한 경우 (예: 수백 개의 캐릭터를 이동시키는 경우) 
	/// 이 스크립트를 사용자 지정하거나 게임에 특별히 최적화된 사용자 정의 이동 스크립트를 작성할 수 있습니다.
	///
	/// 이 스크립트는 주어진 <see cref="destination"/>로 이동하려고 시도합니다. <see cref="repathRate"/> 간격으로 대상까지의 경로가 다시 계산됩니다.
	/// 대상을 따라가기를 원한다면 <see cref="Pathfinding.AIDestinationSetter"/> 구성 요소를 연결할 수 있습니다.
	/// 자세한 정보는 [getstarted(온라인 설명서에서 링크를 확인하세요)] 튜토리얼을 참조하여 이 스크립트를 구성하는 방법에 대한 지침을 얻을 수 있습니다.
	///
	/// 이 스크립트를 사용하여 에이전트를 이동시키는 방법에 대한 비디오는 다음과 같습니다
	/// (기술적으로는 이 스크립트를 상속하되 예제 씬에 대한 약간의 애니메이션 지원을 추가한 <see cref="Pathfinding.Examples.MineBotAI"/> 스크립트를 사용합니다):
	/// [온라인 설명서에서 비디오를 보려면 여기를 클릭하세요]
	///
	/// \section 변수 변수의 간략한 개요
	/// Unity의 인스펙터에서 여러 변수를 볼 수 있습니다. 자세한 정보는 아래에서 확인할 수 있지만 간략한 개요는 다음과 같습니다.
	///
	/// <see cref="repathRate"/>는 새로운 경로를 검색하는 빈도를 결정합니다. 빠른 이동 대상이 있는 경우 값을 낮게 설정할 수 있습니다.
	/// <see cref="destination"/> 필드는 AI가 이동하려고 시도할 위치로, 예를 들어 RTS에서 플레이어가 클릭한 지점일 수 있습니다.
	/// 또는 좀비 게임에서 플레이어 개체가 될 수 있습니다.
	/// <see cref="maxSpeed"/>는 자명한 변수이며, <see cref="rotationSpeed"/>도 그렇습니다. 그러나 <see cref="slowdownDistance"/>는 약간의 설명이 필요합니다.
	/// 이것은 대략적으로 대상에서 AI가 감속을 시작하는 거리입니다. 큰 값을 설정하면 AI가 매우 점진적으로 감속하게 됩니다.
	/// <see cref="pickNextWaypointDist"/>는 AI가 이동할 지점까지의 거리를 결정합니다 (아래 이미지 참조).
	///
	/// 아래 이미지는 이 클래스에서 노출된 여러 변수를 설명하는 데 사용됩니다 (<see cref="pickNextWaypointDist"/>, <see cref="steeringTarget"/>, <see cref="desiredVelocity)"/>.
	/// [온라인 설명서에서 이미지를 보려면 여기를 클릭하세요]
	///
	/// 이 스크립트에는 여러 이동 대체 방법이 있습니다.
	/// 이 스크립트가 같은 게임 오브젝트에 연결된 RVOController를 찾으면 해당 대체 방법을 사용합니다. 캐릭터 컨트롤러를 찾으면 그것도 사용합니다.
	/// 리지드바디를 찾으면 그것도 사용합니다. 마지막으로 대체로 이동에 Transform.position을 수정하는 대신 사용되며 항상 작동하며 가장 효율적인 옵션입니다.
	///
	/// \section how-aipath-works 작동 방식
	/// 이 섹션에서는 이 스크립트의 구조 및 정보 흐름에 대해 설명하고 있습니다.
	/// 이 스크립트를 변경하려거나 작동 방식을 자세히 이해하려는 경우에 유용합니다.
	/// 그러나 스크립트를 그대로 사용할 계획이라면 이 섹션을 읽을 필요가 없습니다.
	///
	/// 이 스크립트는 <see cref="AIBase"/> 클래스에서 상속합니다. 이동은 Unity의 표준 <see cref="Update"/> 또는 <see cref="FixedUpdate"/> 메서드 중 하나에서 수행됩니다.
	/// 이러한 메서드는 AIBase 클래스에서 정의되어 있습니다. 어떤 메서드가 실제로 사용되는지는 움직임에 리지드바디가 사용되는지 여부에 따라 다릅니다.
	/// Rigidbody 이동은 FixedUpdate 메서드에서 수행해야 하지만, 리지드바디가 사용되지 않는 경우에는 Update에서 수행하는 것이 더 나을 수 있습니다.
	///
	/// 여기서 <see cref="MovementUpdate"/> 메서드(이 메서드는 <see cref="MovementUpdateInternal)"/>를 호출합니다)로 호출이 이루어지며, 
	/// 이 메서드에는 주된 코드가 포함되어 있으며 AI가 이동을 *원하는* 방법을 계산합니다.
	/// 그러나 실제 이동은 수행하지 않습니다. 대신 이 프레임의 끝에 AI가 이동하려는 위치와 회전을 반환합니다.
	/// <see cref="Update"/> (또는 <see cref="FixedUpdate)"/> 메서드는 이러한 값을 <see cref="FinalizeMovement"/> 메서드에 전달하며, 실제 캐릭터 이동을 담당합니다.
	/// 이 메서드는 또한 레이캐스팅을 사용하여 AI가 지면에서 떨어지지 않도록 하는 작업과 같은 작업도 처리합니다.
	///
	/// AI는 정기적으로 경로를 다시 계산합니다. 이것은 Update 메서드에서 <see cref="shouldRecalculatePath"/>를 확인하고 true를 반환하면 <see cref="SearchPath"/>를 호출합니다.
	/// <see cref="SearchPath"/> 메서드는 경로 요청을 준비하고 이 스크립트와 동일한 GameObject에 연결된 <see cref="Pathfinding.Seeker"/> 구성 요소에게 보냅니다.
	/// 이 스크립트는 대상별로 <see cref="Pathfinding.Seeker.pathCallback"/> 대리자에 등록하므로 이 스크립트는 경로가 <see cref="OnPathComplete"/> 메서드가 호출될 때마다 계산될 때마다 통지받게 됩니다.
	/// 경로가 계산되는 데 한 프레임 또는 여러 프레임이 걸릴 수 있지만, 마지막으로 <see cref="OnPathComplete"/> 메서드가 호출되고 AI가 따르는 현재 경로가 대체됩니다.
	/// </summary>
	[AddComponentMenu("Pathfinding/AI/AIPath (2D,3D)")]
	public partial class AIPath : AIBase, IAstarAI {
		/// <summary>
		/// 에이전트의 가속도입니다.
		/// 양수 값은 초당 월드 단위의 가속을 나타냅니다.
		/// 음수 값은 에이전트가 최대 속도에 도달하는 데 걸리는 시간의 역수로 해석됩니다.
		/// 예를 들어 에이전트가 최대 속도에 도달하는 데 약 0.4초가 걸린다면 이 필드는 -1/0.4 = -2.5로 설정해야 합니다.
		/// 음수 값의 경우 최종 가속도는 다음과 같이 계산됩니다: -가속도*최대 속도.
		/// 이 동작은 주로 호환성을 위해 존재합니다.
		///
		/// Unity 인스펙터에서는 두 가지 모드가 있습니다. Default 및 Custom 모드입니다. Default 모드에서는 이 필드가 -2.5로 설정되어 에이전트가 최대 속도에 도달하는 데 약 0.4초가 걸립니다.
		/// Custom 모드에서는 가속도를 양의 값으로 설정할 수 있습니다.
		/// </summary>
		public float maxAcceleration = -2.5f;

		/// <summary>
		/// 초당 회전 속도(도 단위).
		/// 회전은 Quaternion.RotateTowards를 사용하여 계산됩니다. 이 변수는 초당 회전 속도를 나타냅니다.
		/// 값이 높을수록 캐릭터가 빠르게 회전할 수 있습니다.
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("turningSpeed")]
		public float rotationSpeed = 360;

		/// <summary>대상 끝에서부터 에이전트가 감속을 시작할 거리</summary>
		public float slowdownDistance = 0.6F;

		/// <summary>
		/// AI가 경로 상에서 이동 지점을 결정하는 데 얼마나 멀리 앞을 바라보는지 나타내는 값입니다.
		/// 월드 단위로 표시됩니다.
		/// 만약 <see cref="alwaysDrawGizmos"/> 토글을 활성화하면이 값은 에이전트 주변에 파란 원으로 시각화됩니다.
		/// [온라인 문서에서 이미지를 보려면 열기]
		///
		/// 여기에는 이 값이 너무 낮거나 너무 높을 때의 일반적인 결과와 함께 몇 가지 예제 비디오가 나와 있습니다.
		/// <table>
		/// <tr><td>[온라인 문서에서 비디오를 보려면 열기]</td><td>\xmlonly <verbatim><span class="label label-danger">너무 낮음</span><br/></verbatim>\endxmlonly> 너무 낮은 값과 너무 낮은 가속도는 에이전트가 많이 overshooting하고 경로를 따라가지 못하는 결과를 낳을 수 있습니다.</td></tr>
		/// <tr><td>[온라인 문서에서 비디오를 보려면 열기]</td><td>\xmlonly <verbatim><span class="label label-warning">적절함</span><br/></verbatim>\endxmlonly> 낮은 값이지만 높은 가속도는 AI가 경로를 더 가깝게 따르도록 작동하여 좋은 결과를 가져올 수 있습니다. 경로를 따르는 것을 원하는 경우 <see cref="Pathfinding.AILerp"/> 컴포넌트가 더 적합합니다.</td></tr>
		/// <tr><td>[온라인 문서에서 비디오를 보려면 열기]</td><td>\xmlonly <verbatim><span class="label label-success">적절함</span><br/></verbatim></span> 이 예제에서 합리적인 값입니다.</td></tr>
		/// <tr><td>[온라인 문서에서 비디오를 보려면 열기]</td><td>\xmlonly <verbatim><span class="label label-success">적절함</span><br/></verbatim>\endxmlonly> 이 예제에서도 합리적인 값이지만 경로가 이전 비디오보다 약간 더 느슨하게 따라갑니다.</td></tr>
		/// <tr><td>[온라인 문서에서 비디오를 보려면 열기]</td><td>\xmlonly <verbatim><span class="label label-danger">너무 높음</span><br/></verbatim>\endxmlonly> 너무 높은 값은 에이전트가 경로를 너무 느슨하게 따르고 장애물을 통과하려 시도할 수 있습니다.</td></tr>
		/// </table>
		/// </summary>
		public float pickNextWaypointDist = 2;

		/// <summary>
		/// 경로의 끝점까지 도달로 간주되는 거리입니다.
		/// 끝점이 이 거리 내에 있으면 <see cref="OnTargetReached"/>가 호출되고 <see cref="reachedEndOfPath"/>가 true를 반환합니다.
		/// </summary>
		public float endReachedDistance = 0.2F;

		/// <summary>선택된 에이전트와 설정이 수정 중일 때에만 씬 뷰에서 자세한 gizmo를 계속 그립니다.</summary>
		public bool alwaysDrawGizmos;

		/// <summary>
		/// 대상 방향을 향하지 않았을 때 감속합니다.
		/// 약간의 성능 오버헤드가 발생합니다.
		/// </summary>
		public bool slowWhenNotFacingTarget = true;

		/// <summary>
		/// 대상에서 <see cref="endReachedDistance"/> 단위 내일 때 무엇을 할지 결정합니다.
		/// 캐릭터는 그 거리 내에 들어왔을 때 즉시 멈출 수 있으며, 활을 쏘거나 대상에 대응하는 활을 사용하는 궁사와 같은 대상에 유용합니다.
		/// 또는 캐릭터는 정확한 대상 지점에 도달하려고 시도하고 거기에서 완전히 멈출 수 있습니다. 이것은 지정한 대상 지점에 도달하려는 경우 유용합니다.
		///
		/// 참고: 이 필드가 설정되어 있지 않더라도 <see cref="reachedEndOfPath"/>는 대상에서 <see cref="endReachedDistance"/> 단위 내에 있을 때 true가 됩니다.
		/// </summary>
		public CloseToDestinationMode whenCloseToDestination = CloseToDestinationMode.Stop;


		/// <summary>
		/// 캐릭터가 항상 네비게이션 메시의 탐험 가능한 표면에 있는지 확인합니다.
		/// 이 옵션을 활성화하면 매 프레임마다 <see cref="AstarPath.GetNearest"/> 쿼리가 실행되어 에이전트가 걸을 수 있는 가장 가까운 노드를 찾습니다.
		/// 그리고 에이전트가 그 노드 안에 있지 않으면 에이전트가 해당 위치로 이동합니다.
		///
		/// 특히 로컬 회피와 함께 사용하면 에이전트가 서로를 벽에 밀어넣는 것을 방지하는 데 유용합니다.
		/// 더 많은 정보를 보려면: 로컬 피하는 (온라인 문서에서 작동 링크 보기)를 참조하세요.
		///
		/// 이 옵션은 로컬 회피와 통합되어 있어 다른 에이전트에 의해 벽에 강제로 밀려들어가면 로컬 회피 시스템이 해당 벽을 고려할 수 있습니다.
		///
		/// 이 옵션을 활성화하면 그래프 유형에 따라 성능 영향이 있습니다 (그리드 그래프의 경우 꽤 빠르고, 네비메시/리캐스트 그래프의 경우 약간 느립니다).
		/// 네비메시/리캐스트 그래프를 사용하는 경우 네비메시/리캐스트 그래프 전용으로 작성된 <see cref="Pathfinding.RichAI"/> 이동 스크립트로 전환하는 것이 좋습니다.
		/// 많은 경우에 경로 주위의 경로를 더 부드럽게 따라갈 수 있습니다.
		///
		/// 그리드 그래프의 경우 이 옵션을 페널 모디파이어와 함께 사용하지 않는 것이 좋습니다. 페널 모디파이어는 경로를 그래프의 가장자리에 매우 가깝게 만들기 때문에
		/// 이 스크립트가 약간 모서리를 잘라 내려고 시도할 가능성이 있습니다. 이렇게 하면 모서리 근처의 탐색 가능한 표면 밖으로 나가려고 시도하는 것처럼 보일 것입니다.
		/// 이 옵션을 활성화하면 이렇게 하면 안됩니다.
		///
		/// 경고: 이 옵션은 포인트 그래프에서 사용할 때 의미가 없습니다. 포인트 그래프에는 표면이 없습니다.
		/// 포인트 그래프를 사용할 때 이 옵션을 활성화하면 에이전트가 매 프레임마다 가장 가까운 노드로 이동되며, 이는 일반적으로 원하는 동작이 아닐 것입니다.
		///
		/// 아래 이미지에서 로컬 회피를 사용하는 여러 에이전트가 모두 모서리에있는 동일한 지점으로 이동하도록 지시되었습니다.
		/// 에이전트를 그래프에 제약을 걸지 않으면 쉽게 장애물 안으로 밀려납니다.
		/// [온라인 문서에서 이미지 보기]
		/// </summary>
		public bool constrainInsideGraph = false;

		/// <summary>현재 따르는 경로</summary>
		protected Path path;

		/// <summary>현재 경로를 따라 점을 계산하는 도우미</summary>
		protected PathInterpolator interpolator = new PathInterpolator();

		#region IAstarAI implementation

		/// <summary>\copydoc Pathfinding::IAstarAI::Teleport</summary>
		public override void Teleport (Vector3 newPosition, bool clearPath = true) {
			reachedEndOfPath = false;
			base.Teleport(newPosition, clearPath);
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::remainingDistance</summary>
		public float remainingDistance {
			get {
				return interpolator.valid ? interpolator.remainingDistance + movementPlane.ToPlane(interpolator.position - position).magnitude : float.PositiveInfinity;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::reachedDestination</summary>
		public bool reachedDestination {
			get {
				if (!reachedEndOfPath) return false;
				if (!interpolator.valid || remainingDistance + movementPlane.ToPlane(destination - interpolator.endPoint).magnitude > endReachedDistance) return false;

				// 2D 모드에서는 높이 검사를 수행하지 않습니다.
				if (orientation != OrientationMode.YAxisForward) {
					// 대상이 캐릭터의 머리 위 또는 발 아래로 매우 멀리 있는지 확인합니다.
					float yDifference;
					movementPlane.ToPlane(destination - position, out yDifference);
					var h = tr.localScale.y * height;
					if (yDifference > h || yDifference < -h*0.5) return false;
				}

				return true;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::reachedEndOfPath</summary>
		public bool reachedEndOfPath { get; protected set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::hasPath</summary>
		public bool hasPath {
			get {
				return interpolator.valid;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::pathPending</summary>
		public bool pathPending {
			get {
				return waitingForPathCalculation;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::steeringTarget</summary>
		public Vector3 steeringTarget {
			get {
				return interpolator.valid ? interpolator.position : position;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		float IAstarAI.radius { get { return radius; } set { radius = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::height</summary>
		float IAstarAI.height { get { return height; } set { height = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::maxSpeed</summary>
		float IAstarAI.maxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::canSearch</summary>
		bool IAstarAI.canSearch { get { return canSearch; } set { canSearch = value; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::canMove</summary>
		bool IAstarAI.canMove { get { return canMove; } set { canMove = value; } }

		#endregion

		/// <summary>\copydoc Pathfinding::IAstarAI::GetRemainingPath</summary>
		public void GetRemainingPath (List<Vector3> buffer, out bool stale) {
			buffer.Clear();
			buffer.Add(position);
			if (!interpolator.valid) {
				stale = true;
				return;
			}

			stale = false;
			interpolator.GetRemainingPath(buffer);
		}

		protected override void OnDisable () {
			base.OnDisable();

			// 현재 경로를 해제하여 풀에 반환합니다.
			if (path != null) path.Release(this);
			path = null;
			interpolator.SetPath(null);
			reachedEndOfPath = false;
		}

		/// <summary>
		/// 경로의 끝에 도달했습니다.
		/// AI가 목적지에 도달했을 때 사용자 정의 로직을 추가하려면 이곳에 추가합니다. 또한 이를 상속한 새 스크립트를 만들고 해당 스크립트에서 이 함수를 재정의할 수도 있습니다.
		///
		/// 이 메서드는 새 경로가 계산되면 호출됩니다. 목적지가 변경될 수 있기 때문입니다.
		/// 따라서 에이전트가 목적지에 가까울 때 이 메서드는 일반적으로 매 <see cref="repathRate"/> 초마다 호출됩니다.
		/// </summary>
		public virtual void OnTargetReached () {
		}

		/// <summary>
		/// 요청한 경로가 계산되었을 때 호출됩니다.
		/// 경로는 먼저 <see cref="UpdatePath"/>에 의해 요청되고, 아마도 동일한 프레임이나 다음 프레임에서 계산됩니다.
		/// 마지막으로 이 함수로 전달됩니다.
		/// </summary>
		protected override void OnPathComplete(Path newPath)
		{
			ABPath p = newPath as ABPath;

			if (p == null) throw new System.Exception("이 함수는 ABPaths만 처리하며 특수 경로 유형을 사용하지 마십시오.");

			waitingForPathCalculation = false;

			// 새 경로의 참조 횟수를 증가시킵니다.
			// 이것은 할당량을 줄이기 위한 객체 풀링에 사용됩니다.
			p.Claim(this);

			// 경로가 어떤 이유로든 계산되지 못했습니다.
			// 자세한 정보는 p.errorLog(디버그 문자열)에 있습니다.
			if (p.error)
			{ 
				p.Release(this);
				SetPath(null);
				return;
			}

			// 이전 경로 해제
			if (path != null) path.Release(this);

			// 이전 경로 대체
			path = p;

			// 경로에 최소한 2개의 지점이 포함되도록 합니다.
			if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
			interpolator.SetPath(path.vectorPath);

			var graph = path.path.Count > 0 ? AstarData.GetGraph(path.path[0]) as ITransformedGraph : null;
			movementPlane = graph != null ? graph.transform : (orientation == OrientationMode.YAxisForward ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 270, 90), Vector3.one)) : GraphTransform.identityTransform);

			// 일부 변수 재설정
			reachedEndOfPath = false;

			// 경로를 요청한 지점에서 현재 위치까지 이동을 시뮬레이트합니다.
			// 이렇게 하면 에이전트가 경로의 첫 번째 지점이 현재 위치에서 멀리 떨어진 경우에 혼란스러워지는 위험이 줄어듭니다.
			interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint) * 0.5f);
			interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

			// 이동할 지점을 업데이트합니다.
			// 참고로 이것을 여기서 수행해야합니다. 그렇지 않으면 remainingDistance 필드가 1프레임 동안 잘못될 수 있습니다.
			// (interpolator.remainingDistance가 정확하지 않기 때문).
			interpolator.MoveToCircleIntersection2D(position, pickNextWaypointDist, movementPlane);

			var distanceToEnd = remainingDistance;
			if (distanceToEnd <= endReachedDistance) {
				reachedEndOfPath = true;
				OnTargetReached();
			}
		}

		protected override void ClearPath () {
			CancelCurrentPathRequest();
			if (path != null) path.Release(this);
			path = null;
			interpolator.SetPath(null);
			reachedEndOfPath = false;
		}

		/// <summary>Update 또는 FixedUpdate 중에 호출됩니다. 물체 이동에 리지드 바디를 사용하는지 여부에 따라 다릅니다.</summary>
		protected override void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
		{
			float currentAcceleration = maxAcceleration;

			// 음수인 경우 최대 속도에서 가속을 계산합니다.
			if (currentAcceleration < 0) currentAcceleration *= -maxSpeed;

			if (updatePosition)
			{
				// 가능한 한 적게 transform.position에서 읽기 때문에 로컬 변수보다는 transform.position에서 읽습니다.
				// (적어도 로컬 변수에 비해 상대적으로 느립니다)
				simulatedPosition = tr.position;
			}
			if (updateRotation) simulatedRotation = tr.rotation;

			var currentPosition = simulatedPosition;

			// 이동할 지점을 업데이트합니다.
			interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
			var dir = movementPlane.ToPlane(steeringTarget - currentPosition);

			// 경로 끝까지의 거리를 계산합니다.
			float distanceToEnd = dir.magnitude + Mathf.Max(0, interpolator.remainingDistance);

			// 목표에 도달했는지 확인합니다.
			var prevTargetReached = reachedEndOfPath;
			reachedEndOfPath = distanceToEnd <= endReachedDistance && interpolator.valid;
			if (!prevTargetReached && reachedEndOfPath) OnTargetReached();
			float slowdown;

			// 에이전트가 보고있는 방향의 정규화된 방향
			var forwards = movementPlane.ToPlane(simulatedRotation * (orientation == OrientationMode.YAxisForward ? Vector3.up : Vector3.forward));

			// 유효한 경로를 따라야 하고 다른 스크립트가 캐릭터를 중지하지 않았는지 확인합니다.
			bool stopped = isStopped || (reachedDestination && whenCloseToDestination == CloseToDestinationMode.Stop);
			if (interpolator.valid && !stopped)
			{
				// 목적지에 가까워짐에 따라 움직이는 속도를 계산합니다.
				// 캐릭터가 목적지에 가까워질수록 더 느리게 이동합니다.
				// 항상 0과 1 사이의 값입니다.
				slowdown = distanceToEnd < slowdownDistance? Mathf.Sqrt(distanceToEnd / slowdownDistance) : 1;

				if (reachedEndOfPath && whenCloseToDestination == CloseToDestinationMode.Stop) {
					// Slow down as quickly as possible
					velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
				} else {
					velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir, dir.normalized*maxSpeed, velocity2D, currentAcceleration, rotationSpeed, maxSpeed, forwards) * deltaTime;
				}
			} else {
				slowdown = 1;
				// 가능한 빨리 감속합니다.
				velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
			}

			velocity2D = MovementUtilities.ClampVelocity(velocity2D, maxSpeed, slowdown, slowWhenNotFacingTarget && enableRotation, forwards);

			ApplyGravity(deltaTime);


			// 이 프레임 동안 에이전트가 이동하려는 양을 설정합니다.
			var delta2D = lastDeltaPosition = CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);
			nextPosition = currentPosition + movementPlane.ToWorld(delta2D, verticalVelocity * lastDeltaTime);
			CalculateNextRotation(slowdown, out nextRotation);
		}

		protected virtual void CalculateNextRotation (float slowdown, out Quaternion nextRotation) {
			if (lastDeltaTime > 0.00001f && enableRotation) {
				Vector2 desiredRotationDirection;
				desiredRotationDirection = velocity2D;

				// 우리가 움직이는 방향을 향해 회전합니다.
				// 목표 지점에 매우 가까울 때는 회전하지 않습니다.
				var currentRotationSpeed = rotationSpeed * Mathf.Max(0, (slowdown - 0.3f) / 0.7f);
				nextRotation = SimulateRotationTowards(desiredRotationDirection, currentRotationSpeed * lastDeltaTime);
			} else {
				// TODO: simulatedRotation
				nextRotation = rotation;
			}
		}

		static NNConstraint cachedNNConstraint = NNConstraint.Default;
		protected override Vector3 ClampToNavmesh (Vector3 position, out bool positionChanged) {
			if (constrainInsideGraph) {
				cachedNNConstraint.tags = seeker.traversableTags;
				cachedNNConstraint.graphMask = seeker.graphMask;
				cachedNNConstraint.distanceXZ = true;
				var clampedPosition = AstarPath.active.GetNearest(position, cachedNNConstraint).position;

				// 우리는 단순히 등호를 확인할 수 없습니다. 몇몇 정밀도가 손실될 수 있기 때문입니다.
				var difference = movementPlane.ToPlane(clampedPosition - position);
				float sqrDifference = difference.sqrMagnitude;
				if (sqrDifference > 0.001f*0.001f) {
					// 에이전트가 네비메시 바깥에 있었습니다. 벽의 방향으로만 이동 속도가 이동하도록 하여
					// 속도가 벽을 향해 이동하지 않도록 합니다.
					velocity2D -= difference * Vector2.Dot(difference, velocity2D) / sqrDifference;

					positionChanged = true;
					// 새 위치를 반환합니다. 그러나 ClampToNavmesh 메서드에서 y 좌표의 변경 사항을 무시합니다. navmesh의 y 좌표는 대개 매우 정확하지 않기 때문입니다.
					return position + movementPlane.ToWorld(difference);
				}
			}

			positionChanged = false;
			return position;
		}

#if UNITY_EDITOR
		[System.NonSerialized]
		int gizmoHash = 0;

		[System.NonSerialized]
		float lastChangedTime = float.NegativeInfinity;

		protected static readonly Color GizmoColor = new Color(46.0f/255, 104.0f/255, 201.0f/255);

		protected override void OnDrawGizmos () {
			base.OnDrawGizmos();
			if (alwaysDrawGizmos) OnDrawGizmosInternal();
		}

		protected override void OnDrawGizmosSelected () {
			base.OnDrawGizmosSelected();
			if (!alwaysDrawGizmos) OnDrawGizmosInternal();
		}

		void OnDrawGizmosInternal () {
			var newGizmoHash = pickNextWaypointDist.GetHashCode() ^ slowdownDistance.GetHashCode() ^ endReachedDistance.GetHashCode();

			if (newGizmoHash != gizmoHash && gizmoHash != 0) lastChangedTime = Time.realtimeSinceStartup;
			gizmoHash = newGizmoHash;
			float alpha = alwaysDrawGizmos ? 1 : Mathf.SmoothStep(1, 0, (Time.realtimeSinceStartup - lastChangedTime - 5f)/0.5f) * (UnityEditor.Selection.gameObjects.Length == 1 ? 1 : 0);

			if (alpha > 0) {
				// 항상 Gizmo가 표시될 때 Scene 뷰가 다시 그려지도록 합니다.
				if (!alwaysDrawGizmos) UnityEditor.SceneView.RepaintAll();
				Draw.Gizmos.Line(position, steeringTarget, GizmoColor * new Color(1, 1, 1, alpha));
				Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation * (orientation == OrientationMode.YAxisForward ? Quaternion.Euler(-90, 0, 0) : Quaternion.identity), Vector3.one);
				Draw.Gizmos.CircleXZ(Vector3.zero, pickNextWaypointDist, GizmoColor * new Color(1, 1, 1, alpha));
				Draw.Gizmos.CircleXZ(Vector3.zero, slowdownDistance, Color.Lerp(GizmoColor, Color.red, 0.5f) * new Color(1, 1, 1, alpha));
				Draw.Gizmos.CircleXZ(Vector3.zero, endReachedDistance, Color.Lerp(GizmoColor, Color.red, 0.8f) * new Color(1, 1, 1, alpha));
			}
		}
#endif

		protected override int OnUpgradeSerializedData (int version, bool unityThread) {
			// 약간의 수정: 감속 값을 초당 회전 각도로 대략 변환합니다.
			if (version < 1) rotationSpeed *= 90;
			return base.OnUpgradeSerializedData(version, unityThread);
		}

        public void Move(Vector3 deltaPosition)
        {
            throw new System.NotImplementedException();
        }
    }
}
