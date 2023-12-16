using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace Pathfinding
{
	using Pathfinding.RVO;
	using Pathfinding.Util;

	/// <summary>
	/// AIPath�� RichAI�� �⺻ Ŭ�����Դϴ�.
	/// �� Ŭ������ AIPath�� RichAI �� �ٿ� �������� ���� �޼���� �ʵ带 ��� �ֽ��ϴ�.
	///
	/// ����: <see cref="Pathfinding.AIPath"/>
	/// ����: <see cref="Pathfinding.RichAI"/>
	/// ����: <see cref="Pathfinding.IAstarAI"/> (��� �̵� ��ũ��Ʈ�� �� �������̽��� �����մϴ�)
	/// </summary>
	[RequireComponent(typeof(Seeker))]
	public abstract class AIBase : VersionedMonoBehaviour
	{
		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		public float radius = 0.5f;

		/// <summary>\copydoc Pathfinding::IAstarAI::height</summary>
		public float height = 2;

		/// <summary>
		/// ������Ʈ�� ���ο� ��θ� �󸶳� ���� ã���� �����մϴ� (�� ����).
		/// ������Ʈ�� �� N�ʸ��� ��ǥ���� ���ο� ��θ� ��ȹ�� ���Դϴ�.
		///
		/// ���� ������ �����̴� ��ǥ�� AI�� �ִٸ�, �� ���� ���� ������ ������ �� �ֽ��ϴ�.
		///
		/// ����: <see cref="shouldRecalculatePath"/>
		/// ����: <see cref="SearchPath"/>
		///
		/// Deprecated: �̰��� \reflink{autoRepath.interval}�� �̸��� ����Ǿ����ϴ�.
		/// ����: \reflink{AutoRepathPolicy}
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
		/// Deprecated: �̰��� \reflink{autoRepath.mode}�� ��ü�Ǿ����ϴ�.
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

		/// <summary>�ʴ� ���� ���� �ִ� �ӵ�</summary>
		[UnityEngine.Serialization.FormerlySerializedAs("speed")]
		public float maxSpeed = 1;

		/// <summary>
		/// ����� �߷�.
		/// ���� (NaN, NaN, NaN)���� �����Ǹ�, Unity ������Ʈ ������ ������ Physics.Gravity�� ���˴ϴ�.
		/// ���� (0,0,0)���� �����Ǹ� �߷��� ������� �ʰ� ���� ħ���� üũ�ϴ� ����ĳ��Ʈ�� ������� �ʽ��ϴ�.
		/// </summary>
		public Vector3 gravity = new Vector3(float.NaN, float.NaN, float.NaN);

		/// <summary>
		/// ���� ��ġ�� ����� ���̾� ����ũ.
		/// �� ���ӿ�����Ʈ�� ������ �ݶ��̴��� ���̾ �������� �ʵ��� �����ϼ���.
		///
		/// ����: <see cref="gravity"/>
		/// ����: https://docs.unity3d.com/Manual/Layers.html
		/// </summary>
		public LayerMask groundMask = -1;

		/// <summary>
		/// ���� ����ĳ��Ʈ ���� ��ġ�� ���� Y ��ǥ ������.
		/// �Ϲ������� ĳ������ �Ǻ��� ĳ������ �� �Ʒ��� ������, ����ĳ��Ʈ�� ĳ������ �߽ɿ��� �߻��ϴ� ���� �Ϲ����̹Ƿ�
		/// �� ���� ĳ������ ������ ������ �Ǿ�� �մϴ�.
		///
		/// ĳ������ �Ǻ� ����Ʈ�������� �������� ��� ����� ���� �׷��� ����ĳ��Ʈ�� ��� ���۵����� ��Ÿ���ϴ�.
		///
		/// ����: <see cref="gravity"/>
		/// Deprecated: ��� <see cref="height"/> �Ӽ��� ����ϼ��� (�� ���� 2��)
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
		/// ������Ʈ�� ��� �������� �̵����� �����մϴ�.
		/// 3D ������ ���, ZAxisIsForward �ɼ��� ������ �����̹Ƿ� ��κ� �� �ɼ��� ���� ���Դϴ�.
		/// 2D ������ ���, YAxisIsForward �ɼ��� ������ �����̹Ƿ� ��κ� �� �ɼ��� ���� ���Դϴ�.
		///
		/// YAxisForward �ɼ��� ����ϸ� ������Ʈ�� �̵��� XZ ����� �ƴ� XY ��鿡�� �߻��Ѵٰ� ������ �� �ֽ��ϴ�.
		/// �̴� �� ������ �� ���ǵǾ� ���� ���� ����Ʈ �׷������� �߿��մϴ�. �ٸ� ���� �׷����� (��: �׸��� �׷���)��
		/// ��� ������Ʈ���� � �̵� ����� ����ؾ� �ϴ��� �˷��ݴϴ�.
		///
		/// [�̹����� ������ �¶��� ������ Ȯ���ϼ���]
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("rotationIn2D")]
		public OrientationMode orientation = OrientationMode.ZAxisForward;

		/// <summary>
		/// ���̸�, AI�� �̵� ������ �ٶ󺸵��� ȸ���� ���Դϴ�.
		/// ����: <see cref="orientation"/>
		/// </summary>
		public bool enableRotation = true;

		/// <summary>
		/// ������Ʈ�� ��ġ.
		/// <see cref="updatePosition"/>�� ���̶�� �� ���� �� ������ Transform.position�� ����ȭ�� ���Դϴ�.
		/// </summary>
		protected Vector3 simulatedPosition;

		/// <summary>
		/// ������Ʈ�� ȸ��.
		/// <see cref="updateRotation"/>�� ���̶�� �� ���� �� ������ Transform.rotation�� ����ȭ�� ���Դϴ�.
		/// </summary>
		protected Quaternion simulatedRotation;

		/// <summary>
		/// ������Ʈ�� ��ġ�Դϴ�.
		/// ���� ���������� ��ġ�̸�,
		/// ���� <see cref="updatePosition"/>�� true��� �� ���� transform.position�� �����մϴ�.
		/// ����: <see cref="Teleport"/>
		/// ����: <see cref="Move"/>
		/// </summary>
		public Vector3 position { get { return updatePosition ? tr.position : simulatedPosition; } }

		/// <summary>
		/// ������Ʈ�� ȸ���Դϴ�.
		/// ���� <see cref="updateRotation"/>�� true��� �� ���� transform.rotation�� �����մϴ�.
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

		/// <summary><see cref="Move"/> �޼ҵ�κ��� ������ �̵� ��Ÿ ���Դϴ�.</summary>
		Vector3 accumulatedMovementDelta = Vector3.zero;

		/// <summary>
		/// ������Ʈ�� ���ϴ� ���� �ӵ��Դϴ� (���� ȸ�ǿ� ������ �������� �ʽ��ϴ�).
		/// �̵� ���� �ֽ��ϴ�.
		/// </summary>
		protected Vector2 velocity2D;

		/// <summary>
		/// �߷¿� ���� �ӵ��Դϴ�.
		/// �̵� ��鿡 �����Դϴ�.
		///
		/// ������Ʈ�� ���鿡 ���� �� �� ���� ������Ʈ�� �ӵ��� ��Ȯ�ϰ� �ݿ����� ���� �� �ֽ��ϴ�.
		/// ������Ʈ�� �������� �������� �ұ��ϰ� ��(ު)���� ���� �� �ֽ��ϴ�.
		/// </summary>
		protected float verticalVelocity;

		/// <summary>ĳ�õ� Seeker ������Ʈ�Դϴ�.</summary>
		protected Seeker seeker;

		/// <summary>ĳ�õ� Transform ������Ʈ�Դϴ�.</summary>
		protected Transform tr;

		/// <summary>ĳ�õ� Rigidbody ������Ʈ�Դϴ�.</summary>
		protected Rigidbody rigid;

		/// <summary>ĳ�õ� Rigidbody2D ������Ʈ�Դϴ�.</summary>
		protected Rigidbody2D rigid2D;

		/// <summary>ĳ�õ� CharacterController ������Ʈ�Դϴ�.</summary>
		protected CharacterController controller;


		/// <summary>
		/// �� ������Ʈ�� �����̴� ����Դϴ�.
		/// �̰��� ���� ������ �̵� ��� ���̸� ��ȯ�Ͽ�, �� ��ũ��Ʈ��
		/// 2D ���Ӱ� 3D ���� ���ʿ��� ����� �� �ֵ��� �մϴ�.
		/// </summary>
		public IMovementPlane movementPlane = GraphTransform.identityTransform;

		/// <summary>
		/// ĳ������ ��ġ�� Transform�� ��ġ�� ����Ǿ�� �ϴ����� �����մϴ�.
		/// false�� ��� ��� �̵� ����� ��Ҵ�� ���������, �� ������Ʈ�� ������ ��ü�� �������� �ʰ�
		/// ��� <see cref="position"/> �Ӽ��� ����˴ϴ�.
		///
		/// �̴� ������Ʈ�� �ٸ� ������� �����ϰ� ���� �� �����մϴ�.
		/// ���� ���, ��Ʈ ����� ��������� ������ AI�� �����Ӱ� �����̱� ���ϴ� ��� ���� �ֽ��ϴ�.
		/// ����: <see cref="canMove"/>�� �� �ʵ�� �޸� ��� �̵� ����� ��Ȱ��ȭ�մϴ�.
		/// ����: <see cref="updateRotation"/>
		/// </summary>
		[System.NonSerialized]
		public bool updatePosition = true;

		/// <summary>
		/// ĳ������ ȸ���� Transform�� ȸ���� ����Ǿ�� �ϴ����� �����մϴ�.
		/// false�� ��� ��� �̵� ����� ��Ҵ�� ���������, �� ������Ʈ�� ������ ��ü�� ȸ������ �ʰ�
		/// ��� <see cref="rotation"/> �Ӽ��� ����˴ϴ�.
		///
		/// ����: <see cref="updatePosition"/>
		/// </summary>
		[System.NonSerialized]
		public bool updateRotation = true;

		/// <summary>
		/// ������Ʈ�� ������ �� �ִ��� ���θ� �����մϴ�.
		/// false�� ��� ������Ʈ�� �� ��ġ�� ȸ���� ������� �ʽ��ϴ�.
		/// ����: <see cref="updatePosition"/>
		/// ����: <see cref="updateRotation"/>
		/// </summary>
		public AutoRepathPolicy autoRepath = new AutoRepathPolicy();

		/// <summary>���� �����ӿ��� �߷��� ���Ǵ��� ���θ� ��Ÿ���ϴ�.</summary>
		protected bool usingGravity { get; set; }

		/// <summary>���� ������ ������ �̵��� ���� ��Ÿ �ð��Դϴ�.</summary>
		protected float lastDeltaTime;

		/// <summary><see cref="prevPosition1"/>�� ������Ʈ�� ������ ������ �ε����Դϴ�.</summary>
		protected int prevFrame;

		/// <summary>���� �������� ������ ĳ������ ��ġ�Դϴ�.</summary>
		protected Vector3 prevPosition1;

		/// <summary>���� ������ ���� �����ӿ��� ĳ������ ��ġ�Դϴ�.</summary>
		protected Vector3 prevPosition2;

		/// <summary>���� ������ ���� ĳ���Ͱ� �����̷��� �ϴ� ���Դϴ�.</summary>
		protected Vector2 lastDeltaPosition;

		/// <summary>���� ��ΰ� ���� ��쿡�� ��ũ��Ʈ�� �� ��θ� �˻��ϵ����ؾ��մϴ�.</summary>
		protected bool waitingForPathCalculation = false;

		[UnityEngine.Serialization.FormerlySerializedAs("target")]
		[SerializeField]
		[HideInInspector]
		Transform targetCompatibility;

		/// <summary>
		/// Start �޼��尡 ����Ǿ����� ���θ� ��Ÿ���ϴ�.
		/// OnEnable���� �ڷ�ƾ�� �������� �ʵ����ϱ� ���� ���˴ϴ�.
		/// Awake �ܰ�(�Ǵ� ��Ȯ���� ���� ������ ����)���� ��θ� ������� �ʵ����մϴ�.
		/// </summary>
		bool startHasRun = false;

		/// <summary>
		/// �̵� ����Դϴ�.
		/// AI�� �� ����� ���󰡰ų� �� ������� �̵��Ϸ��� �õ��մϴ�.
		/// ���� ��� RTS ���ӿ����� �÷��̾ Ŭ���� ���� ���� ���� �� ������,
		/// ���� ���ӿ����� ���� �÷��̾� ������Ʈ�� ��Ÿ�� �� �ֽ��ϴ�.
		///
		/// ������� �ʴ� ���� �����ϴ�: 4.1 ���������� �ڵ����� <see cref="Pathfinding.AIDestinationSetter"/> ���� ��Ҹ� �߰��ϰ� �ش� ���� ��ҿ� ����� �����մϴ�.
		/// ��ſ� <see cref="destination"/> �Ӽ��� ����ϰų� ������� ����� Ʈ�������� �������� �ʵ��� ��ſ�
		/// ���� AIDestinationSetter ���� ��Ҹ� ����Ͻʽÿ�.
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
		/// �� ������Ʈ�� �̵��Ϸ��� ���ϴ� �ӵ��Դϴ�.
		/// �߷� �� �ʿ��� ��� ���� ���ظ� �����մϴ�.
		/// </summary>
		public Vector3 desiredVelocity { get { return lastDeltaTime > 0.00001f ? movementPlane.ToWorld(lastDeltaPosition / lastDeltaTime, verticalVelocity) : Vector3.zero; } }

		/// <summary>\copydoc Pathfinding::IAstarAI::isStopped</summary>
		public bool isStopped { get; set; }

		/// <summary>\copydoc Pathfinding::IAstarAI::onSearchPath</summary>
		public System.Action onSearchPath { get; set; }

		/// <summary>��θ� ������ ���� �ڵ����� �ٽ� ����ؾ��ϴ��� �����Դϴ�.</summary>
		protected virtual bool shouldRecalculatePath
		{
			get
			{
				return !waitingForPathCalculation && autoRepath.ShouldRecalculatePath((IAstarAI)this);
			}
		}

		protected AIBase()
		{
			// �̰��� Awake�� ���� �޼��尡 �ƴ� ���⿡�� �����Ǿ���մϴ�.
			// �ٸ� �ڵ尡 Awake �޼��尡 �� ��ũ��Ʈ���� ����Ǳ� ���� ��� �Ӽ��� ������ �� �ֱ� �����Դϴ�.
			destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		/// <summary>
		/// RVOController �� CharacterController ��� ���� ����� ���� ��Ҹ� ã���ϴ�.
		///
		/// �̰��� <see cref="OnEnable"/>���� ����˴ϴ�. ��Ÿ�� �߿� ���� ��Ҹ� �߰�/�����ϴ� ���
		/// �� �Լ��� ȣ���Ͽ� ��ũ��Ʈ�� �ش� ���� ��Ҹ� ã���� �Ͽ��� �մϴ�.
		/// ���� ���鿡�� �� �����Ӹ��� ���� ��Ҹ� ã�� ���� ��ȿ�����Դϴ�.
		/// </summary>
		public virtual void FindComponents()
		{
			tr = transform;
			seeker = GetComponent<Seeker>();
			// ����� �̵� ���� ��� ã��
			controller = GetComponent<CharacterController>();
			rigid = GetComponent<Rigidbody>();
			rigid2D = GetComponent<Rigidbody2D>();
		}

		/// <summary>������Ʈ�� Ȱ��ȭ�� �� ȣ��˴ϴ�.</summary>
		protected virtual void OnEnable()
		{
			FindComponents();
			// ��ΰ� ���� �� �ݹ��� �����ϵ����մϴ�.
			seeker.pathCallback += OnPathComplete;
			Init();
		}

		/// <summary>
		/// ��θ� �˻��� �����մϴ�.
		/// �� �޼��带 �������ϴ� ��� ��κ��� ��� ���� �κп��� base.Start ()�� ȣ���ؾ��մϴ�.
		/// ����: <see cref="Init"/>
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
				// ������Ʈ�� �׺�޽��� ������ŵ�ϴ� (�ڷ���Ʈ ȣ���� ���������� �����ϴ� �Ͱ� �����ϴ�. RichAI�� ���� �Ϻ� �̵� ��ũ��Ʈ�� �ʿ��մϴ�).
				// �ڷ���Ʈ ȣ���� �Ϻ� �̵� ��ũ��Ʈ (��: RichAI)�� ���� �Ϻ� ������ �ùٸ��� �ʱ�ȭ�ǵ����մϴ� (#prevPosition1 �� #prevPosition2�� ���� ����).
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

		/// <summary>���� ��� ��û�� ����մϴ�.</summary>
		protected void CancelCurrentPathRequest()
		{
			waitingForPathCalculation = false;
			// ���� ����� ����� �ߴ��մϴ�.
			if (seeker != null) seeker.CancelCurrentPathRequest();
		}

		protected virtual void OnDisable()
		{
			ClearPath();

			// ��ΰ� �Ϸ�� �� �� �̻� �ݹ��� �������� �ʵ����մϴ�.
			seeker.pathCallback -= OnPathComplete;

			velocity2D = Vector3.zero;
			accumulatedMovementDelta = Vector3.zero;
			verticalVelocity = 0f;
			lastDeltaTime = 0;
		}

		/// <summary>
		/// �� �����Ӹ��� ȣ��˴ϴ�.
		/// RigidBody�� ������� ������ ��� �������� ���⿡�� �߻��մϴ�.
		/// </summary>
		protected virtual void Update()
		{
			if (shouldRecalculatePath) SearchPath();

			// �߷� ��� ���δ� ���� ���� ��ҿ� ���� �ٸ��ϴ�.
			// ���� ���, non-kinematic RigidBody�� ����ϸ� RigidBody ��ü���� �߷��� �����մϴ�.
			// �߷��� NaN�� ���� �� �� �����Ƿ� �񱳴� a == b ��� !(a == b)�� ����մϴ�.
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
		/// �� ���� ������Ʈ���� ȣ��˴ϴ�.
		/// RigidBody�� ����ϴ� ��� ��� �������� ���⿡�� �߻��մϴ�.
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

		/// <summary>Update �Ǵ� FixedUpdate�� ���� ȣ��˴ϴ�. RigidBody�� ����Ͽ� �������� ó���ϰų� �׷��� ���� ����Դϴ�.</summary>
		protected abstract void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);

		/// <summary>
		/// ���� �ڵ� ��� ��û�� �������� ������ ����մϴ�.
		/// ���� Ŭ������ ��� ��û�� ������ �ٲٴ� ���� �����ϱ� ���� ������ �޼���� ���˴ϴ�.
		/// ���� ���, �׷��� �������� ��ȯ�ϱ� ���� ������ ��ȯ�ؾ��ϴ� LocalSpaceRichAI ��ũ��Ʈ�� ���� ���˴ϴ�.
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

			// ���� ��ġ���� ������ ��θ� ����ϵ��� ��û�մϴ�.
			ABPath p = ABPath.Construct(start, end, null);
			SetPath(p);
		}

		/// <summary>
		/// ĳ������ ���� ��ġ�Դϴ�.
		/// �̰��� ĳ������ �߽ɿ� ��� �� ��ġ�� �ǹ� ����Ʈ�� ��ġ�Ǵ� ��찡 ���� �ֱ� ������
		/// ��� ã�⿡ ���˴ϴ�. ���� ������ �̷���� �ǹ�����
		/// ĳ������ �߽��� ���� �Ʒ������ٴ� ������ �׺�޽ÿ� �� ����� �� ������ �̷� ���� �߸��� ��ΰ� ��� �� �� �ֽ��ϴ�.
		/// �̸� �ذ��ϱ� ���� ��û�� ����� �������� �׻� ĳ������ ������ ��ġ�մϴ�.
		/// </summary>
		public virtual Vector3 GetFeetPosition()
		{
			return position;
		}

		/// <summary>��û�� ��ΰ� ���� ��� ȣ��˴ϴ�.</summary>
		protected abstract void OnPathComplete(Path newPath);

		/// <summary>
		/// ������Ʈ�� ���� ��θ� ����ϴ�.
		///
		/// �Ϲ������� <see cref="SetPath(null)"/>�� ����Ͽ� ȣ��˴ϴ�.
		///
		/// ����: <see cref="SetPath"/>
		/// ����: <see cref="isStopped"/>
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
				// ��ΰ� ���� ������ �ʾҽ��ϴ�.
				waitingForPathCalculation = true;
				seeker.CancelCurrentPathRequest();
				seeker.StartPath(path);
				autoRepath.DidRecalculatePath(destination);
			}
			else if (path.PipelineState == PathState.Returned)
			{
				// ��ΰ� �̹� ���Ǿ����ϴ�.
				// ���ÿ� �ٸ� ��θ� ����� �� ������, �� ��ΰ� �̸� ����� �ʵ����Ϸ��� ����մϴ�.
				if (seeker.GetCurrentPath() != path) seeker.CancelCurrentPathRequest();
				else throw new System.ArgumentException("seeker.StartPath�� ����Ͽ� ��θ� ����ϴ� ��� �� ��ũ��Ʈ�� ��� ��θ� ��� ������ ����� �Ϸ�Ǵ� ��� ��ο� ���� �� ��ũ��Ʈ�� ��θ� �����ɴϴ�. �� ��� SetPath�� ȣ������ �ʾƾ��մϴ�.");

				OnPathComplete(path);
			}
			else
			{
				// ��� ����� ���۵Ǿ����� ���� �Ϸ���� �ʾҽ��ϴ�. �̸� ó���� �� �����ϴ�.
				throw new System.ArgumentException("�� ��ũ��Ʈ�� ����Ͽ� �ƿ� ��� ����� �������� ���� ����̰ų� �̹� ������ ���� ��θ� ���� ��η� SetPath �޼��带 ȣ���ؾ��մϴ�. ����Ϸ��� ����� ��� ����� ���۵Ǿ����� ���� �Ϸ���� ���� �� �����ϴ�.");
			}
		}

		/// <summary>
		/// ������Ʈ�� �Ʒ��� �����մϴ�.
		/// ����: <see cref="verticalVelocity"/>
		/// ����: <see cref="gravity"/>
		/// </summary>
		protected void ApplyGravity(float deltaTime)
		{
			// �߷� ����
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

		/// <summary>���� ������ ���� �̵��ؾ��ϴ� �Ÿ��� ����մϴ�.</summary>
		protected Vector2 CalculateDeltaToMoveThisFrame(Vector2 position, float distanceToEndOfPath, float deltaTime)
		{
			// �̵� ���� �� �Ÿ�
			return Vector2.ClampMagnitude(velocity2D * deltaTime, distanceToEndOfPath);
		}

		/// <summary>
		/// ������ �������� ������Ʈ�� ȸ���� ��ó�� �ùķ��̼��ϰ� ���ο� ȸ���� ��ȯ�մϴ�.
		///
		/// �̰��� ���ο� ȸ���� ����ϴ� �͸� �����ϸ� ������Ʈ�� ���� ȸ���� �������� �ʽ��ϴ�.
		/// FinalizeMovement�� ����Ͽ� �������� �ܺο��� ó�������� ���� ȸ�� �ڵ带 ����Ϸ��� ��� �����մϴ�.
		///
		/// ����: <see cref="orientation"/>
		/// <paramref name="maxDegrees"/>�� �� ������ ���� �ִ�� ȸ���� �� �ִ� �����Դϴ�.
		/// </summary>
		/// <param name="direction">���� �������� ȸ���� �����Դϴ�.</param>
		/// <param name="maxDegrees">�� �����ӿ��� �ִ� ȸ�� �����Դϴ�.</param>
		public Quaternion SimulateRotationTowards(Vector3 direction, float maxDegrees)
		{
			return SimulateRotationTowards(movementPlane.ToPlane(direction), maxDegrees);
		}

		/// <summary>
		/// ������ �������� ������Ʈ�� ȸ���� ��ó�� �ùķ��̼��ϰ� ���ο� ȸ���� ��ȯ�մϴ�.
		///
		/// �̰��� ���ο� ȸ���� ����ϴ� �͸� �����ϸ� ������Ʈ�� ���� ȸ���� �������� �ʽ��ϴ�.
		///
		/// ����: <see cref="orientation"/>
		/// ����: <see cref="movementPlane"/>
		/// </summary>
		/// <param name="direction">������ ��鿡�� ȸ���� �����Դϴ�.</param>
		/// <param name="maxDegrees">�� �����ӿ��� �ִ� ȸ�� �����Դϴ�.</param>
		protected Quaternion SimulateRotationTowards(Vector2 direction, float maxDegrees)
		{
			if (direction != Vector2.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation(movementPlane.ToWorld(direction, 0), movementPlane.ToWorld(Vector2.zero, 1));
				// �̰��� ĳ���Ͱ� Z �� �����θ� ȸ���ϵ��� ����ϴ�.
				if (orientation == OrientationMode.YAxisForward) targetRotation *= Quaternion.Euler(90, 0, 0);
				return Quaternion.RotateTowards(simulatedRotation, targetRotation, maxDegrees);
			}
			return simulatedRotation;
		}

		/// <summary>
		/// ������Ʈ�� ��ġ�� �̵��մϴ�.
		///
		/// �̰��� ������Ʈ�� �������� ��� ó������ �����Ϸ��� ��� ���˴ϴ�. ���� ���, Mecanim�� ����Ͽ� ��Ʈ ����� ����ϴ� ����Դϴ�.
		///
		/// �̰��� CharacterController, Rigidbody, Rigidbody2D �Ǵ� ������ �ɼǿ� ���� ���˴ϴ�.
		///
		/// ������ �� ������Ʈ�� �׺�޽ÿ� Ŭ�����˴ϴ� (�Ϲ������� RichAI ���� ��Ҹ��� �̸� �����մϴ�).
		///
		/// ����: <see cref="MovementUpdate"/>�� �� ���� ���� �ڵ尡 ���ԵǾ� �ֽ��ϴ�.
		/// ����: <see cref="controller"/>, <see cref="rigid"/>, <see cref="rigid2D"/>
		/// </summary>
		/// <param name="nextPosition">������Ʈ�� �� ��ġ�Դϴ�.</param>
		/// <param name="nextRotation">������Ʈ�� �� ȸ���Դϴ�. #enableRotation�� false�̸��� �Ű� ������ ���õ˴ϴ�.</param>
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
			// ���� ������ ����ϸ� ����� �����ϴ�.
			Vector3 currentPosition = simulatedPosition;
			bool positionDirty1 = false;

			if (controller != null && controller.enabled && updatePosition)
			{
				// CharacterController ���
				// Transform�� navmesh �ٱ��� �ְ� ���� ����� ��ȿ�� ��ġ�� �̵��ؾ� �ϴ� ��� ��ġ�� #position�� ���� ���� �� �ֽ��ϴ�.
				tr.position = currentPosition;
				controller.Move((nextPosition - currentPosition) + accumulatedMovementDelta);
				// �̵� �� ������ ����� �� �ִ� ��ġ�� ĸó�մϴ�.
				// RVO�� ������ �� �� �����ϵ��� clampedPosition ��꿡 �߰��ϼ���.
				currentPosition = tr.position;
				if (controller.isGrounded) verticalVelocity = 0;
			}
			else
			{
				// Transform, Rigidbody, Rigidbody2D �Ǵ� (updatePosition = false �� ���) �ƹ� �͵� ������� �ʽ��ϴ�.
				float lastElevation;
				movementPlane.ToPlane(currentPosition, out lastElevation);
				currentPosition = nextPosition + accumulatedMovementDelta;

				// ĳ���͸� ���� ��ġ��ŵ�ϴ�.
				if (usingGravity) currentPosition = RaycastPosition(currentPosition, lastElevation);
				positionDirty1 = true;
			}

			// �̵��� ���� �� ��ġ�� navmesh�� ����ϴ�.
			bool positionDirty2 = false;
			currentPosition = ClampToNavmesh(currentPosition, out positionDirty2);

			// ��ġ�� �̹� �������� ���� ��� ���� ��ġ�� ĳ���Ϳ� �Ҵ��մϴ� (�ַ� ������ ���� ��ġ ������ ���� �� �����Ƿ�).
			if ((positionDirty1 || positionDirty2) && updatePosition)
			{
				// rigid.MovePosition�� ĳ���͸� ��� �̵���Ű�ų� �׷��� ���� �� �ֽ��ϴ�.
				// Ư���� ��쿡 ���� Unity ������ Ȯ���ϼ���.
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
		/// ĳ������ ��ġ�� navmesh�� �µ��� �����մϴ�.
		/// ��� �̵� ��ũ��Ʈ�� �̸� �������� ���� �� �ֽ��ϴ�.
		///
		/// ��ȯ��: navmesh�� ���ѵ� ĳ������ �� ��ġ�Դϴ�.
		/// </summary>
		/// <param name="position">ĳ������ ���� ��ġ�Դϴ�.</param>
		/// <param name="positionChanged">�� �޼��忡 ���� ĳ������ ��ġ�� ����Ǿ����� �����Դϴ�.</param>
		protected virtual Vector3 ClampToNavmesh(Vector3 position, out bool positionChanged)
		{
			positionChanged = false;
			return position;
		}

		/// <summary>
		/// ĳ���Ͱ� ���� �پ� �ִ��� Ȯ���ϰ� ������ �浹�� �����մϴ�.
		///
		/// <see cref="verticalVelocity"/>�� 0���� �����ϸ� ĳ���Ͱ� ���� �پ� ���� ���Դϴ�.
		///
		/// ��ȯ��: ĳ������ �� ��ġ�Դϴ�.
		/// </summary>
		/// <param name="position">���迡�� ĳ������ ��ġ�Դϴ�.</param>
		/// <param name="lastElevation">ĳ���Ͱ� �̵��Ǳ� ���� ����� �� ��ǥ�Դϴ�. �̰��� #movementPlane�� 'up' ���� �����ϴ�.</param>
		protected Vector3 RaycastPosition(Vector3 position, float lastElevation)
		{
			RaycastHit hit;
			float elevation;

			movementPlane.ToPlane(position, out elevation);
			float rayLength = tr.localScale.y * height * 0.5f + Mathf.Max(0, lastElevation - elevation);
			Vector3 rayOffset = movementPlane.ToWorld(Vector2.zero, rayLength);

			if (Physics.Raycast(position + rayOffset, -rayOffset, out hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
			{
				// ���� �پ� ����
				// ���� �ӵ��� ���������� ���Դϴ�. �������� ���鿡�� �ո����Դϴ�. ĳ���ʹ� ������ �������� ������ ���鿡 ���� �� ��� ���� �ӵ��� ��� �������� �ʽ��ϴ�.
				// AI�� ������ �Ʒ��� �����̷��� �õ������� ���� ��� �����Ƿ� ���� �ӵ��� 0�Դϴ�. �̰��� ���� �Ʒ��� �̵��� �� �ſ� ������ �˴ϴ�.
				// ���鿡 ���� �� ĳ������ �ӵ��� ���η� �����Ǹ� ������ '�ٿ��' �ൿ�� �߻��մϴ�. (�׽�Ʈ�غ�����, �����ϱ� ����� �����Դϴ�).
				// �� ������ ���������� �� ��Ȯ�� ������ ����ؾ� ������ �̰��� ���� �ٻ簪�̸� ������ �� �����ϴ�. �Ʒ� ���� ��� '5'�� ���� �ӵ��� ���������� ���� ���� �ʹ� ���� ����� �ʷ��� �� �ֽ��ϴ�.
				verticalVelocity *= System.Math.Max(0, 1 - 5 * lastDeltaTime);
				return hit.point;
			}
			return position;
		}

		protected virtual void OnDrawGizmosSelected()
		{
			// Unity �ν����Ϳ��� ���õ� �� �ٸ� ���� ��Ұ� ����/�����ǰų� Ȱ��ȭ/��Ȱ��ȭ�Ǹ� ��� �����ϵ��� �մϴ�.
			// �����Ӹ��� �� �۾��� �������� �������� �մϴ�. (����� ��Դϴ�).
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
