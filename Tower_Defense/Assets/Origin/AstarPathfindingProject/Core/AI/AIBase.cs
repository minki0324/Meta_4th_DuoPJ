using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace Pathfinding
{
	using Pathfinding.RVO;
	using Pathfinding.Util;

	/// <summary>
	/// AIPath와 RichAI의 기본 클래스입니다.
	/// 이 클래스는 AIPath와 RichAI 둘 다에 공통적인 여러 메서드와 필드를 담고 있습니다.
	///
	/// 참조: <see cref="Pathfinding.AIPath"/>
	/// 참조: <see cref="Pathfinding.RichAI"/>
	/// 참조: <see cref="Pathfinding.IAstarAI"/> (모든 이동 스크립트가 이 인터페이스를 구현합니다)
	/// </summary>
	[RequireComponent(typeof(Seeker))]
	public abstract class AIBase : VersionedMonoBehaviour
	{
		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		public float radius = 0.5f;

		/// <summary>\copydoc Pathfinding::IAstarAI::height</summary>
		public float height = 2;

		/// <summary>
		/// 에이전트가 새로운 경로를 얼마나 자주 찾을지 결정합니다 (초 단위).
		/// 에이전트는 매 N초마다 목표까지 새로운 경로를 계획할 것입니다.
		///
		/// 만약 빠르게 움직이는 목표나 AI가 있다면, 이 값을 낮은 값으로 설정할 수 있습니다.
		///
		/// 참조: <see cref="shouldRecalculatePath"/>
		/// 참조: <see cref="SearchPath"/>
		///
		/// Deprecated: 이것은 \reflink{autoRepath.interval}로 이름이 변경되었습니다.
		/// 참조: \reflink{AutoRepathPolicy}
		/// </summary>
		public float repathRate
		{
			get
			{
				return this.autoRepath.interval;
			}
			set
			{
				this.autoRepath.interval = value;
			}
		}

		/// <summary>
		/// \copydoc Pathfinding::IAstarAI::canSearch
		/// Deprecated: 이것은 \reflink{autoRepath.mode}로 대체되었습니다.
		/// </summary>
		public bool canSearch
		{
			get
			{
				return this.autoRepath.mode != AutoRepathPolicy.Mode.Never;
			}
			set
			{
				if (value)
				{
					if (this.autoRepath.mode == AutoRepathPolicy.Mode.Never)
					{
						this.autoRepath.mode = AutoRepathPolicy.Mode.EveryNSeconds;
					}
				}
				else
				{
					this.autoRepath.mode = AutoRepathPolicy.Mode.Never;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::canMove</summary>
		public bool canMove = true;

		/// <summary>초당 월드 단위 최대 속도</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("speed")]
		public float maxSpeed = 1;

		/// <summary>
		/// 사용할 중력.
		/// 만약 (NaN, NaN, NaN)으로 설정되면, Unity 프로젝트 설정에 구성된 Physics.Gravity가 사용됩니다.
		/// 만약 (0,0,0)으로 설정되면 중력이 적용되지 않고 지면 침투를 체크하는 레이캐스트도 수행되지 않습니다.
		/// </summary>
		public Vector3 gravity = new Vector3(float.NaN, float.NaN, float.NaN);

		/// <summary>
		/// 지면 배치에 사용할 레이어 마스크.
		/// 이 게임오브젝트에 부착된 콜라이더의 레이어를 포함하지 않도록 주의하세요.
		///
		/// 참조: <see cref="gravity"/>
		/// 참조: https://docs.unity3d.com/Manual/Layers.html
		/// </summary>
		public LayerMask groundMask = -1;

		/// <summary>
		/// 지면 레이캐스트 시작 위치에 대한 Y 좌표 오프셋.
		/// 일반적으로 캐릭터의 피봇은 캐릭터의 발 아래에 있지만, 레이캐스트는 캐릭터의 중심에서 발사하는 것이 일반적이므로
		/// 이 값은 캐릭터의 높이의 절반이 되어야 합니다.
		///
		/// 캐릭터의 피봇 포인트에서부터 위쪽으로 녹색 기즈모 선이 그려져 레이캐스트가 어디서 시작될지를 나타냅니다.
		///
		/// 참조: <see cref="gravity"/>
		/// Deprecated: 대신 <see cref="height"/> 속성을 사용하세요 (이 값의 2배)
		/// </summary>
		[System.Obsolete("Use the height property instead (2x this value)")]
		public float centerOffset
		{
			get { return height * 0.5f; }
			set { height = value * 2; }
		}

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("centerOffset")]
		float centerOffsetCompatibility = float.NaN;

		[SerializeField]
		[HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs("repathRate")]
		float repathRateCompatibility = float.NaN;

		[SerializeField]
		[HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs("canSearch")]
		[UnityEngine.Serialization.FormerlySerializedAs("repeatedlySearchPaths")]
		bool canSearchCompability = false;

		/// <summary>
		/// 에이전트가 어느 방향으로 이동할지 결정합니다.
		/// 3D 게임의 경우, ZAxisIsForward 옵션이 게임의 관례이므로 대부분 이 옵션을 원할 것입니다.
		/// 2D 게임의 경우, YAxisIsForward 옵션이 게임의 관례이므로 대부분 이 옵션을 원할 것입니다.
		///
		/// YAxisForward 옵션을 사용하면 에이전트는 이동이 XZ 평면이 아닌 XY 평면에서 발생한다고 가정할 수 있습니다.
		/// 이는 위 방향이 잘 정의되어 있지 않은 포인트 그래프에만 중요합니다. 다른 내장 그래프들 (예: 그리드 그래프)은
		/// 모두 에이전트에게 어떤 이동 평면을 사용해야 하는지 알려줍니다.
		///
		/// [이미지를 보려면 온라인 문서를 확인하세요]
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("rotationIn2D")]
		public OrientationMode orientation = OrientationMode.ZAxisForward;

		/// <summary>
		/// 참이면, AI는 이동 방향을 바라보도록 회전할 것입니다.
		/// 참조: <see cref="orientation"/>
		/// </summary>
		public bool enableRotation = true;

		/// <summary>
		/// 에이전트의 위치.
		/// <see cref="updatePosition"/>이 참이라면 이 값은 매 프레임 Transform.position과 동기화될 것입니다.
		/// </summary>
		protected Vector3 simulatedPosition;

		/// <summary>
		/// 에이전트의 회전.
		/// <see cref="updateRotation"/>이 참이라면 이 값은 매 프레임 Transform.rotation과 동기화될 것입니다.
		/// </summary>
		protected Quaternion simulatedRotation;

		/// <summary>
		/// 에이전트의 위치입니다.
		/// 월드 공간에서의 위치이며,
		/// 만약 <see cref="updatePosition"/>이 true라면 이 값은 transform.position과 동일합니다.
		/// 참조: <see cref="Teleport"/>
		/// 참조: <see cref="Move"/>
		/// </summary>
		public Vector3 position { get { return updatePosition ? tr.position : simulatedPosition; } }

		/// <summary>
		/// 에이전트의 회전입니다.
		/// 만약 <see cref="updateRotation"/>이 true라면 이 값은 transform.rotation과 동일합니다.
		/// </summary>
		public Quaternion rotation
		{
			get { return updateRotation ? tr.rotation : simulatedRotation; }
			set
			{
				if (updateRotation)
				{
					tr.rotation = value;
				}
				else
				{
					simulatedRotation = value;
				}
			}
		}

		/// <summary><see cref="Move"/> 메소드로부터 누적된 이동 델타 값입니다.</summary>
		Vector3 accumulatedMovementDelta = Vector3.zero;

		/// <summary>
		/// 에이전트가 원하는 현재 속도입니다 (로컬 회피와 물리를 포함하지 않습니다).
		/// 이동 평면상에 있습니다.
		/// </summary>
		protected Vector2 velocity2D;

		/// <summary>
		/// 중력에 의한 속도입니다.
		/// 이동 평면에 수직입니다.
		///
		/// 에이전트가 지면에 있을 때 이 값은 에이전트의 속도를 정확하게 반영하지 않을 수 있습니다.
		/// 에이전트가 움직이지 않음에도 불구하고 비(非)제로 값일 수 있습니다.
		/// </summary>
		protected float verticalVelocity;

		/// <summary>캐시된 Seeker 컴포넌트입니다.</summary>
		protected Seeker seeker;

		/// <summary>캐시된 Transform 컴포넌트입니다.</summary>
		protected Transform tr;

		/// <summary>캐시된 Rigidbody 컴포넌트입니다.</summary>
		protected Rigidbody rigid;

		/// <summary>캐시된 Rigidbody2D 컴포넌트입니다.</summary>
		protected Rigidbody2D rigid2D;

		/// <summary>캐시된 CharacterController 컴포넌트입니다.</summary>
		protected CharacterController controller;


		/// <summary>
		/// 이 에이전트가 움직이는 평면입니다.
		/// 이것은 월드 공간과 이동 평면 사이를 변환하여, 이 스크립트를
		/// 2D 게임과 3D 게임 양쪽에서 사용할 수 있도록 합니다.
		/// </summary>
		public IMovementPlane movementPlane = GraphTransform.identityTransform;

		/// <summary>
		/// 캐릭터의 위치가 Transform의 위치와 연결되어야 하는지를 결정합니다.
		/// false일 경우 모든 이동 계산은 평소대로 수행되지만, 이 컴포넌트가 부착된 객체는 움직이지 않고
		/// 대신 <see cref="position"/> 속성만 변경됩니다.
		///
		/// 이는 에이전트를 다른 방법으로 제어하고 싶을 때 유용합니다.
		/// 예를 들어, 루트 모션을 사용하지만 여전히 AI가 자유롭게 움직이길 원하는 경우 등이 있습니다.
		/// 참조: <see cref="canMove"/>는 이 필드와 달리 모든 이동 계산을 비활성화합니다.
		/// 참조: <see cref="updateRotation"/>
		/// </summary>
		[System.NonSerialized]
		public bool updatePosition = true;

		/// <summary>
		/// 캐릭터의 회전이 Transform의 회전과 연결되어야 하는지를 결정합니다.
		/// false일 경우 모든 이동 계산은 평소대로 수행되지만, 이 컴포넌트가 부착된 객체는 회전하지 않고
		/// 대신 <see cref="rotation"/> 속성만 변경됩니다.
		///
		/// 참조: <see cref="updatePosition"/>
		/// </summary>
		[System.NonSerialized]
		public bool updateRotation = true;

		/// <summary>
		/// 에이전트가 움직일 수 있는지 여부를 결정합니다.
		/// false일 경우 에이전트는 새 위치나 회전을 계산하지 않습니다.
		/// 참조: <see cref="updatePosition"/>
		/// 참조: <see cref="updateRotation"/>
		/// </summary>
		public AutoRepathPolicy autoRepath = new AutoRepathPolicy();

		/// <summary>현재 프레임에서 중력이 사용되는지 여부를 나타냅니다.</summary>
		protected bool usingGravity { get; set; }

		/// <summary>지난 프레임 동안의 이동에 사용된 델타 시간입니다.</summary>
		protected float lastDeltaTime;

		/// <summary><see cref="prevPosition1"/>이 업데이트된 마지막 프레임 인덱스입니다.</summary>
		protected int prevFrame;

		/// <summary>지난 프레임의 끝에서 캐릭터의 위치입니다.</summary>
		protected Vector3 prevPosition1;

		/// <summary>지난 프레임 이전 프레임에서 캐릭터의 위치입니다.</summary>
		protected Vector3 prevPosition2;

		/// <summary>지난 프레임 동안 캐릭터가 움직이려고 하는 양입니다.</summary>
		protected Vector2 lastDeltaPosition;

		/// <summary>이전 경로가 계산된 경우에만 스크립트가 새 경로를 검색하도록해야합니다.</summary>
		protected bool waitingForPathCalculation = false;

		[UnityEngine.Serialization.FormerlySerializedAs("target")]
		[SerializeField]
		[HideInInspector]
		Transform targetCompatibility;

		/// <summary>
		/// Start 메서드가 실행되었는지 여부를 나타냅니다.
		/// OnEnable에서 코루틴을 시작하지 않도록하기 위해 사용됩니다.
		/// Awake 단계(또는 정확히는 시작 프레임 이전)에서 경로를 계산하지 않도록합니다.
		/// </summary>
		bool startHasRun = false;

		/// <summary>
		/// 이동 대상입니다.
		/// AI는 이 대상을 따라가거나 이 대상으로 이동하려고 시도합니다.
		/// 예를 들어 RTS 게임에서는 플레이어가 클릭한 지면 위의 점일 수 있으며,
		/// 좀비 게임에서는 좀비가 플레이어 오브젝트를 나타낼 수 있습니다.
		///
		/// 사용하지 않는 것이 좋습니다: 4.1 버전에서는 자동으로 <see cref="Pathfinding.AIDestinationSetter"/> 구성 요소를 추가하고 해당 구성 요소에 대상을 설정합니다.
		/// 대신에 <see cref="destination"/> 속성을 사용하거나 대상으로 사용할 트랜스폼을 생성하지 않도록 대신에
		/// 직접 AIDestinationSetter 구성 요소를 사용하십시오.
		/// </summary>
		[System.Obsolete("Use the destination property or the AIDestinationSetter component instead")]
		public Transform target
		{
			get
			{
				var setter = GetComponent<AIDestinationSetter>();
				return setter != null ? setter.target : null;
			}
			set
			{
				targetCompatibility = null;
				var setter = GetComponent<AIDestinationSetter>();
				if (setter == null) setter = gameObject.AddComponent<AIDestinationSetter>();
				setter.target = value;
				destination = value != null ? value.position : new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::destination</summary>
		public Vector3 destination { get; set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::velocity</summary>
		public Vector3 velocity
		{
			get
			{
				return lastDeltaTime > 0.000001f ? (prevPosition1 - prevPosition2) / lastDeltaTime : Vector3.zero;
			}
		}

		/// <summary>
		/// 이 에이전트가 이동하려는 원하는 속도입니다.
		/// 중력 및 필요한 경우 로컬 방해를 포함합니다.
		/// </summary>
		public Vector3 desiredVelocity { get { return lastDeltaTime > 0.00001f ? movementPlane.ToWorld(lastDeltaPosition / lastDeltaTime, verticalVelocity) : Vector3.zero; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::isStopped</summary>
		public bool isStopped { get; set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::onSearchPath</summary>
		public System.Action onSearchPath { get; set; }

		/// <summary>경로를 가능한 빨리 자동으로 다시 계산해야하는지 여부입니다.</summary>
		protected virtual bool shouldRecalculatePath
		{
			get
			{
				return !waitingForPathCalculation && autoRepath.ShouldRecalculatePath((IAstarAI)this);
			}
		}

		protected AIBase()
		{
			// 이것은 Awake와 같은 메서드가 아닌 여기에서 설정되어야합니다.
			// 다른 코드가 Awake 메서드가 이 스크립트에서 실행되기 전에 대상 속성을 설정할 수 있기 때문입니다.
			destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		/// <summary>
		/// RVOController 및 CharacterController 등과 같은 연결된 구성 요소를 찾습니다.
		///
		/// 이것은 <see cref="OnEnable"/>에서 수행됩니다. 런타임 중에 구성 요소를 추가/제거하는 경우
		/// 이 함수를 호출하여 스크립트가 해당 구성 요소를 찾도록 하여야 합니다.
		/// 성능 측면에서 매 프레임마다 구성 요소를 찾는 것은 비효율적입니다.
		/// </summary>
		public virtual void FindComponents()
		{
			tr = transform;
			seeker = GetComponent<Seeker>();
			// 연결된 이동 구성 요소 찾기
			controller = GetComponent<CharacterController>();
			rigid = GetComponent<Rigidbody>();
			rigid2D = GetComponent<Rigidbody2D>();
		}

		/// <summary>컴포넌트가 활성화될 때 호출됩니다.</summary>
		protected virtual void OnEnable()
		{
			FindComponents();
			// 경로가 계산될 때 콜백을 수신하도록합니다.
			seeker.pathCallback += OnPathComplete;
			Init();
		}

		/// <summary>
		/// 경로를 검색을 시작합니다.
		/// 이 메서드를 재정의하는 경우 대부분의 경우 시작 부분에서 base.Start ()를 호출해야합니다.
		/// 참조: <see cref="Init"/>
		/// </summary>
		protected virtual void Start()
		{
			startHasRun = true;
			Init();
		}

		void Init()
		{
			if (startHasRun)
			{
				// 에이전트를 네비메쉬에 고정시킵니다 (텔레포트 호출이 본질적으로 수행하는 것과 같습니다. RichAI와 같은 일부 이동 스크립트만 필요합니다).
				// 텔레포트 호출은 일부 이동 스크립트 (예: RichAI)와 같이 일부 변수가 올바르게 초기화되도록합니다 (#prevPosition1 및 #prevPosition2와 같은 변수).
				if (canMove) Teleport(position, false);
				autoRepath.Reset();
				if (shouldRecalculatePath) SearchPath();
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::Teleport</summary>
		public virtual void Teleport(Vector3 newPosition, bool clearPath = true)
		{
			if (clearPath) ClearPath();
			prevPosition1 = prevPosition2 = simulatedPosition = newPosition;
			if (updatePosition) tr.position = newPosition;
			if (clearPath) SearchPath();
		}

		/// <summary>현재 경로 요청을 취소합니다.</summary>
		protected void CancelCurrentPathRequest()
		{
			waitingForPathCalculation = false;
			// 현재 경로의 계산을 중단합니다.
			if (seeker != null) seeker.CancelCurrentPathRequest();
		}

		protected virtual void OnDisable()
		{
			ClearPath();

			// 경로가 완료될 때 더 이상 콜백을 수신하지 않도록합니다.
			seeker.pathCallback -= OnPathComplete;

			velocity2D = Vector3.zero;
			accumulatedMovementDelta = Vector3.zero;
			verticalVelocity = 0f;
			lastDeltaTime = 0;
		}

		/// <summary>
		/// 매 프레임마다 호출됩니다.
		/// RigidBody를 사용하지 않으면 모든 움직임이 여기에서 발생합니다.
		/// </summary>
		protected virtual void Update()
		{
			if (shouldRecalculatePath) SearchPath();

			// 중력 사용 여부는 여러 가지 요소에 따라 다릅니다.
			// 예를 들어, non-kinematic RigidBody를 사용하면 RigidBody 자체에서 중력을 적용합니다.
			// 중력은 NaN을 포함 할 수 있으므로 비교는 a == b 대신 !(a == b)를 사용합니다.
			usingGravity = !(gravity == Vector3.zero) && (!updatePosition || ((rigid == null || rigid.isKinematic) && (rigid2D == null || rigid2D.isKinematic)));
			if (rigid == null && rigid2D == null && canMove)
			{
				Vector3 nextPosition;
				Quaternion nextRotation;
				MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);
				FinalizeMovement(nextPosition, nextRotation);
			}
		}

		/// <summary>
		/// 매 물리 업데이트마다 호출됩니다.
		/// RigidBody를 사용하는 경우 모든 움직임이 여기에서 발생합니다.
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (!(rigid == null && rigid2D == null) && canMove)
			{
				Vector3 nextPosition;
				Quaternion nextRotation;
				MovementUpdate(Time.fixedDeltaTime, out nextPosition, out nextRotation);
				FinalizeMovement(nextPosition, nextRotation);
			}
		}


		/// <summary>\copydoc Pathfinding::IAstarAI::MovementUpdate</summary>
		public void MovementUpdate(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
		{
			lastDeltaTime = deltaTime;
			MovementUpdateInternal(deltaTime, out nextPosition, out nextRotation);
		}

		/// <summary>Update 또는 FixedUpdate에 따라 호출됩니다. RigidBody를 사용하여 움직임을 처리하거나 그렇지 않은 경우입니다.</summary>
		protected abstract void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);

		/// <summary>
		/// 다음 자동 경로 요청의 시작점과 끝점을 출력합니다.
		/// 하위 클래스가 경로 요청의 끝점을 바꾸는 것을 쉽게하기 위해 별도의 메서드로 사용됩니다.
		/// 예를 들어, 그래프 공간으로 변환하기 위해 끝점을 변환해야하는 LocalSpaceRichAI 스크립트와 같이 사용됩니다.
		/// </summary>
		protected virtual void CalculatePathRequestEndpoints(out Vector3 start, out Vector3 end)
		{
			start = GetFeetPosition();
			end = destination;
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::SearchPath</summary>
		public virtual void SearchPath()
		{
			if (float.IsPositiveInfinity(destination.x)) return;
			if (onSearchPath != null) onSearchPath();

			Vector3 start, end;
			CalculatePathRequestEndpoints(out start, out end);

			// 현재 위치에서 대상까지 경로를 계산하도록 요청합니다.
			ABPath p = ABPath.Construct(start, end, null);
			SetPath(p);
		}

		/// <summary>
		/// 캐릭터의 기점 위치입니다.
		/// 이것은 캐릭터의 중심에 대신 발 위치에 피벗 포인트가 배치되는 경우가 종종 있기 때문에
		/// 경로 찾기에 사용됩니다. 여러 층으로 이루어진 건물에서
		/// 캐릭터의 중심이 종종 아래층보다는 윗층의 네비메시에 더 가까울 수 있으며 이로 인해 잘못된 경로가 계산 될 수 있습니다.
		/// 이를 해결하기 위해 요청된 경로의 시작점은 항상 캐릭터의 기점에 위치합니다.
		/// </summary>
		public virtual Vector3 GetFeetPosition()
		{
			return position;
		}

		/// <summary>요청된 경로가 계산된 경우 호출됩니다.</summary>
		protected abstract void OnPathComplete(Path newPath);

		/// <summary>
		/// 에이전트의 현재 경로를 지웁니다.
		///
		/// 일반적으로 <see cref="SetPath(null)"/>를 사용하여 호출됩니다.
		///
		/// 참조: <see cref="SetPath"/>
		/// 참조: <see cref="isStopped"/>
		/// </summary>
		protected abstract void ClearPath();

		/// <summary>\copydoc Pathfinding::IAstarAI::SetPath</summary>
		public void SetPath(Path path)
		{
			if (path == null)
			{
				CancelCurrentPathRequest();
				ClearPath();
			}
			else if (path.PipelineState == PathState.Created)
			{
				// 경로가 아직 계산되지 않았습니다.
				waitingForPathCalculation = true;
				seeker.CancelCurrentPathRequest();
				seeker.StartPath(path);
				autoRepath.DidRecalculatePath(destination);
			}
			else if (path.PipelineState == PathState.Returned)
			{
				// 경로가 이미 계산되었습니다.
				// 동시에 다른 경로를 계산할 수 있으며, 이 경로가 이를 덮어쓰지 않도록하려면 취소합니다.
				if (seeker.GetCurrentPath() != path) seeker.CancelCurrentPathRequest();
				else throw new System.ArgumentException("seeker.StartPath를 사용하여 경로를 계산하는 경우 이 스크립트는 모든 경로를 듣기 때문에 계산이 완료되는 모든 경로에 대해 이 스크립트가 경로를 가져옵니다. 이 경우 SetPath를 호출하지 않아야합니다.");

				OnPathComplete(path);
			}
			else
			{
				// 경로 계산이 시작되었지만 아직 완료되지 않았습니다. 이를 처리할 수 없습니다.
				throw new System.ArgumentException("이 스크립트를 사용하여 아예 경로 계산을 시작하지 않은 경로이거나 이미 완전히 계산된 경로를 가진 경로로 SetPath 메서드를 호출해야합니다. 사용하려는 경로의 경로 계산이 시작되었지만 아직 완료되지 않은 것 같습니다.");
			}
		}

		/// <summary>
		/// 에이전트를 아래로 가속합니다.
		/// 참조: <see cref="verticalVelocity"/>
		/// 참조: <see cref="gravity"/>
		/// </summary>
		protected void ApplyGravity(float deltaTime)
		{
			// 중력 적용
			if (usingGravity)
			{
				float verticalGravity;
				velocity2D += movementPlane.ToPlane(deltaTime * (float.IsNaN(gravity.x) ? Physics.gravity : gravity), out verticalGravity);
				verticalVelocity += verticalGravity;
			}
			else
			{
				verticalVelocity = 0;
			}
		}

		/// <summary>단일 프레임 동안 이동해야하는 거리를 계산합니다.</summary>
		protected Vector2 CalculateDeltaToMoveThisFrame(Vector2 position, float distanceToEndOfPath, float deltaTime)
		{
			// 이동 방향 및 거리
			return Vector2.ClampMagnitude(velocity2D * deltaTime, distanceToEndOfPath);
		}

		/// <summary>
		/// 지정된 방향으로 에이전트를 회전한 것처럼 시뮬레이션하고 새로운 회전을 반환합니다.
		///
		/// 이것은 새로운 회전을 계산하는 것만 포함하며 에이전트의 실제 회전을 변경하지 않습니다.
		/// FinalizeMovement를 사용하여 움직임을 외부에서 처리하지만 내장 회전 코드를 사용하려는 경우 유용합니다.
		///
		/// 참조: <see cref="orientation"/>
		/// <paramref name="maxDegrees"/>는 이 프레임 동안 최대로 회전할 수 있는 각도입니다.
		/// </summary>
		/// <param name="direction">월드 공간에서 회전할 방향입니다.</param>
		/// <param name="maxDegrees">이 프레임에서 최대 회전 각도입니다.</param>
		public Quaternion SimulateRotationTowards(Vector3 direction, float maxDegrees)
		{
			return SimulateRotationTowards(movementPlane.ToPlane(direction), maxDegrees);
		}

		/// <summary>
		/// 지정된 방향으로 에이전트를 회전한 것처럼 시뮬레이션하고 새로운 회전을 반환합니다.
		///
		/// 이것은 새로운 회전을 계산하는 것만 포함하며 에이전트의 실제 회전을 변경하지 않습니다.
		///
		/// 참조: <see cref="orientation"/>
		/// 참조: <see cref="movementPlane"/>
		/// </summary>
		/// <param name="direction">움직임 평면에서 회전할 방향입니다.</param>
		/// <param name="maxDegrees">이 프레임에서 최대 회전 각도입니다.</param>
		protected Quaternion SimulateRotationTowards(Vector2 direction, float maxDegrees)
		{
			if (direction != Vector2.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation(movementPlane.ToWorld(direction, 0), movementPlane.ToWorld(Vector2.zero, 1));
				// 이것은 캐릭터가 Z 축 주위로만 회전하도록 만듭니다.
				if (orientation == OrientationMode.YAxisForward) targetRotation *= Quaternion.Euler(90, 0, 0);
				return Quaternion.RotateTowards(simulatedRotation, targetRotation, maxDegrees);
			}
			return simulatedRotation;
		}

		/// <summary>
		/// 에이전트를 위치로 이동합니다.
		///
		/// 이것은 에이전트의 움직임을 어떻게 처리할지 무시하려는 경우 사용됩니다. 예를 들어, Mecanim을 사용하여 루트 모션을 사용하는 경우입니다.
		///
		/// 이것은 CharacterController, Rigidbody, Rigidbody2D 또는 가능한 옵션에 따라 사용됩니다.
		///
		/// 움직임 후 에이전트는 네비메시에 클램프됩니다 (일반적으로 RichAI 구성 요소만이 이를 수행합니다).
		///
		/// 참조: <see cref="MovementUpdate"/>에 몇 가지 예제 코드가 포함되어 있습니다.
		/// 참조: <see cref="controller"/>, <see cref="rigid"/>, <see cref="rigid2D"/>
		/// </summary>
		/// <param name="nextPosition">에이전트의 새 위치입니다.</param>
		/// <param name="nextRotation">에이전트의 새 회전입니다. #enableRotation이 false이면이 매개 변수는 무시됩니다.</param>
		public virtual void FinalizeMovement(Vector3 nextPosition, Quaternion nextRotation)
		{
			if (enableRotation) FinalizeRotation(nextRotation);
			FinalizePosition(nextPosition);
		}

		void FinalizeRotation(Quaternion nextRotation)
		{
			simulatedRotation = nextRotation;
			if (updateRotation)
			{
				if (rigid != null) rigid.MoveRotation(nextRotation);
				else if (rigid2D != null) rigid2D.MoveRotation(nextRotation.eulerAngles.z);
				else tr.rotation = nextRotation;
			}
		}

		void FinalizePosition(Vector3 nextPosition)
		{
			// 로컬 변수를 사용하며 상당히 빠릅니다.
			Vector3 currentPosition = simulatedPosition;
			bool positionDirty1 = false;

			if (controller != null && controller.enabled && updatePosition)
			{
				// CharacterController 사용
				// Transform이 navmesh 바깥에 있고 가장 가까운 유효한 위치로 이동해야 하는 경우 위치가 #position에 있지 않을 수 있습니다.
				tr.position = currentPosition;
				controller.Move((nextPosition - currentPosition) + accumulatedMovementDelta);
				// 이동 후 물리를 고려할 수 있는 위치를 캡처합니다.
				// RVO가 물리에 더 잘 반응하도록 clampedPosition 계산에 추가하세요.
				currentPosition = tr.position;
				if (controller.isGrounded) verticalVelocity = 0;
			}
			else
			{
				// Transform, Rigidbody, Rigidbody2D 또는 (updatePosition = false 인 경우) 아무 것도 사용하지 않습니다.
				float lastElevation;
				movementPlane.ToPlane(currentPosition, out lastElevation);
				currentPosition = nextPosition + accumulatedMovementDelta;

				// 캐릭터를 땅에 위치시킵니다.
				if (usingGravity) currentPosition = RaycastPosition(currentPosition, lastElevation);
				positionDirty1 = true;
			}

			// 이동이 끝난 후 위치를 navmesh에 맞춥니다.
			bool positionDirty2 = false;
			currentPosition = ClampToNavmesh(currentPosition, out positionDirty2);

			// 위치를 이미 설정하지 않은 경우 최종 위치를 캐릭터에 할당합니다 (주로 성능을 위해 위치 설정이 느릴 수 있으므로).
			if ((positionDirty1 || positionDirty2) && updatePosition)
			{
				// rigid.MovePosition은 캐릭터를 즉시 이동시키거나 그렇지 않을 수 있습니다.
				// 특수한 경우에 대한 Unity 문서를 확인하세요.
				if (rigid != null) rigid.MovePosition(currentPosition);
				else if (rigid2D != null) rigid2D.MovePosition(currentPosition);
				else tr.position = currentPosition;
			}

			accumulatedMovementDelta = Vector3.zero;
			simulatedPosition = currentPosition;
			UpdateVelocity();
		}

		protected void UpdateVelocity()
		{
			var currentFrame = Time.frameCount;

			if (currentFrame != prevFrame) prevPosition2 = prevPosition1;
			prevPosition1 = position;
			prevFrame = currentFrame;
		}

		/// <summary>
		/// 캐릭터의 위치를 navmesh에 맞도록 제한합니다.
		/// 모든 이동 스크립트가 이를 지원하지 않을 수 있습니다.
		///
		/// 반환값: navmesh에 제한된 캐릭터의 새 위치입니다.
		/// </summary>
		/// <param name="position">캐릭터의 현재 위치입니다.</param>
		/// <param name="positionChanged">이 메서드에 의해 캐릭터의 위치가 변경되었는지 여부입니다.</param>
		protected virtual Vector3 ClampToNavmesh(Vector3 position, out bool positionChanged)
		{
			positionChanged = false;
			return position;
		}

		/// <summary>
		/// 캐릭터가 땅에 붙어 있는지 확인하고 땅과의 충돌을 방지합니다.
		///
		/// <see cref="verticalVelocity"/>를 0으로 설정하면 캐릭터가 땅에 붙어 있을 때입니다.
		///
		/// 반환값: 캐릭터의 새 위치입니다.
		/// </summary>
		/// <param name="position">세계에서 캐릭터의 위치입니다.</param>
		/// <param name="lastElevation">캐릭터가 이동되기 전에 고려된 고도 좌표입니다. 이것은 #movementPlane의 'up' 축을 따릅니다.</param>
		protected Vector3 RaycastPosition(Vector3 position, float lastElevation)
		{
			RaycastHit hit;
			float elevation;

			movementPlane.ToPlane(position, out elevation);
			float rayLength = tr.localScale.y * height * 0.5f + Mathf.Max(0, lastElevation - elevation);
			Vector3 rayOffset = movementPlane.ToWorld(Vector2.zero, rayLength);

			if (Physics.Raycast(position + rayOffset, -rayOffset, out hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
			{
				// 땅에 붙어 있음
				// 수직 속도를 지수적으로 줄입니다. 물리적인 측면에서 합리적입니다. 캐릭터는 완전히 경직되지 않으며 지면에 닿을 때 모든 수직 속도가 즉시 없어지지 않습니다.
				// AI는 여전히 아래로 움직이려고 시도하지만 땅에 닿아 있으므로 수직 속도는 0입니다. 이것은 경사로 아래로 이동할 때 매우 도움이 됩니다.
				// 지면에 닿을 때 캐릭터의 속도가 제로로 설정되면 일종의 '바운싱' 행동이 발생합니다. (테스트해보세요, 설명하기 어려운 동작입니다).
				// 이 공식은 물리적으로 더 정확한 공식을 사용해야 하지만 이것은 좋은 근사값이며 성능이 더 좋습니다. 아래 식의 상수 '5'는 수렴 속도를 결정하지만 높은 값은 너무 많은 노이즈를 초래할 수 있습니다.
				verticalVelocity *= System.Math.Max(0, 1 - 5 * lastDeltaTime);
				return hit.point;
			}
			return position;
		}

		protected virtual void OnDrawGizmosSelected()
		{
			// Unity 인스펙터에서 선택될 때 다른 구성 요소가 연결/해제되거나 활성화/비활성화되면 즉시 반응하도록 합니다.
			// 프레임마다 이 작업을 수행하지 않으려고 합니다. (비용이 비쌉니다).
			if (Application.isPlaying) FindComponents();
		}

		public static readonly Color ShapeGizmoColor = new Color(240 / 255f, 213 / 255f, 30 / 255f);

		protected virtual void OnDrawGizmos()
		{
			if (!Application.isPlaying || !enabled) FindComponents();

			var color = ShapeGizmoColor;
			if (orientation == OrientationMode.YAxisForward)
			{
				Draw.Gizmos.Cylinder(position, Vector3.forward, 0, radius * tr.localScale.x, color);
			}
			else
			{
				Draw.Gizmos.Cylinder(position, rotation * Vector3.up, tr.localScale.y * height, radius * tr.localScale.x, color);
			}

			if (!float.IsPositiveInfinity(destination.x) && Application.isPlaying) Draw.Gizmos.CircleXZ(destination, 0.2f, Color.blue);

			autoRepath.DrawGizmos((IAstarAI)this);
		}

		protected override void Reset()
		{
			ResetShape();
			base.Reset();
		}

		void ResetShape()
		{
			var cc = GetComponent<CharacterController>();

			if (cc != null)
			{
				radius = cc.radius;
				height = Mathf.Max(radius * 2, cc.height);
			}
		}

		protected override int OnUpgradeSerializedData(int version, bool unityThread)
		{
			if (unityThread && !float.IsNaN(centerOffsetCompatibility))
			{
				height = centerOffsetCompatibility * 2;
				ResetShape();
				centerOffsetCompatibility = float.NaN;
			}
#pragma warning disable 618
			if (unityThread && targetCompatibility != null) target = targetCompatibility;
#pragma warning restore 618
			if (version <= 3)
			{
				repathRate = repathRateCompatibility;
				canSearch = canSearchCompability;
			}
			return 5;
		}
	}
}
