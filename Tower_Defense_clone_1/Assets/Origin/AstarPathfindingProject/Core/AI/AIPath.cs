using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.RVO;
	using Pathfinding.Util;

	/// <summary>
	/// �н��� ���� �̵��ϴ� AI�Դϴ�.
	/// �� AI�� A* Pathfinding Project�� �Բ� �����Ǵ� �⺻ �̵� ��ũ��Ʈ��, ���� �� ĳ������ �̵��� �����ϱ� ���� �����ִ� ��ũ��Ʈ�Դϴ�.
	/// �� ��ũ��Ʈ�� �پ��� ���� ������ �� �۵������� ���� ���� ������ �ʿ��� ��� (��: ���� ���� ĳ���͸� �̵���Ű�� ���) 
	/// �� ��ũ��Ʈ�� ����� �����ϰų� ���ӿ� Ư���� ����ȭ�� ����� ���� �̵� ��ũ��Ʈ�� �ۼ��� �� �ֽ��ϴ�.
	///
	/// �� ��ũ��Ʈ�� �־��� <see cref="destination"/>�� �̵��Ϸ��� �õ��մϴ�. <see cref="repathRate"/> �������� �������� ��ΰ� �ٽ� ���˴ϴ�.
	/// ����� ���󰡱⸦ ���Ѵٸ� <see cref="Pathfinding.AIDestinationSetter"/> ���� ��Ҹ� ������ �� �ֽ��ϴ�.
	/// �ڼ��� ������ [getstarted(�¶��� �������� ��ũ�� Ȯ���ϼ���)] Ʃ�丮���� �����Ͽ� �� ��ũ��Ʈ�� �����ϴ� ����� ���� ��ħ�� ���� �� �ֽ��ϴ�.
	///
	/// �� ��ũ��Ʈ�� ����Ͽ� ������Ʈ�� �̵���Ű�� ����� ���� ������ ������ �����ϴ�
	/// (��������δ� �� ��ũ��Ʈ�� ����ϵ� ���� ���� ���� �ణ�� �ִϸ��̼� ������ �߰��� <see cref="Pathfinding.Examples.MineBotAI"/> ��ũ��Ʈ�� ����մϴ�):
	/// [�¶��� �������� ������ ������ ���⸦ Ŭ���ϼ���]
	///
	/// \section ���� ������ ������ ����
	/// Unity�� �ν����Ϳ��� ���� ������ �� �� �ֽ��ϴ�. �ڼ��� ������ �Ʒ����� Ȯ���� �� ������ ������ ����� ������ �����ϴ�.
	///
	/// <see cref="repathRate"/>�� ���ο� ��θ� �˻��ϴ� �󵵸� �����մϴ�. ���� �̵� ����� �ִ� ��� ���� ���� ������ �� �ֽ��ϴ�.
	/// <see cref="destination"/> �ʵ�� AI�� �̵��Ϸ��� �õ��� ��ġ��, ���� ��� RTS���� �÷��̾ Ŭ���� ������ �� �ֽ��ϴ�.
	/// �Ǵ� ���� ���ӿ��� �÷��̾� ��ü�� �� �� �ֽ��ϴ�.
	/// <see cref="maxSpeed"/>�� �ڸ��� �����̸�, <see cref="rotationSpeed"/>�� �׷����ϴ�. �׷��� <see cref="slowdownDistance"/>�� �ణ�� ������ �ʿ��մϴ�.
	/// �̰��� �뷫������ ��󿡼� AI�� ������ �����ϴ� �Ÿ��Դϴ�. ū ���� �����ϸ� AI�� �ſ� ���������� �����ϰ� �˴ϴ�.
	/// <see cref="pickNextWaypointDist"/>�� AI�� �̵��� ���������� �Ÿ��� �����մϴ� (�Ʒ� �̹��� ����).
	///
	/// �Ʒ� �̹����� �� Ŭ�������� ����� ���� ������ �����ϴ� �� ���˴ϴ� (<see cref="pickNextWaypointDist"/>, <see cref="steeringTarget"/>, <see cref="desiredVelocity)"/>.
	/// [�¶��� �������� �̹����� ������ ���⸦ Ŭ���ϼ���]
	///
	/// �� ��ũ��Ʈ���� ���� �̵� ��ü ����� �ֽ��ϴ�.
	/// �� ��ũ��Ʈ�� ���� ���� ������Ʈ�� ����� RVOController�� ã���� �ش� ��ü ����� ����մϴ�. ĳ���� ��Ʈ�ѷ��� ã���� �װ͵� ����մϴ�.
	/// ������ٵ� ã���� �װ͵� ����մϴ�. ���������� ��ü�� �̵��� Transform.position�� �����ϴ� ��� ���Ǹ� �׻� �۵��ϸ� ���� ȿ������ �ɼ��Դϴ�.
	///
	/// \section how-aipath-works �۵� ���
	/// �� ���ǿ����� �� ��ũ��Ʈ�� ���� �� ���� �帧�� ���� �����ϰ� �ֽ��ϴ�.
	/// �� ��ũ��Ʈ�� �����Ϸ��ų� �۵� ����� �ڼ��� �����Ϸ��� ��쿡 �����մϴ�.
	/// �׷��� ��ũ��Ʈ�� �״�� ����� ��ȹ�̶�� �� ������ ���� �ʿ䰡 �����ϴ�.
	///
	/// �� ��ũ��Ʈ�� <see cref="AIBase"/> Ŭ�������� ����մϴ�. �̵��� Unity�� ǥ�� <see cref="Update"/> �Ǵ� <see cref="FixedUpdate"/> �޼��� �� �ϳ����� ����˴ϴ�.
	/// �̷��� �޼���� AIBase Ŭ�������� ���ǵǾ� �ֽ��ϴ�. � �޼��尡 ������ ���Ǵ����� �����ӿ� ������ٵ� ���Ǵ��� ���ο� ���� �ٸ��ϴ�.
	/// Rigidbody �̵��� FixedUpdate �޼��忡�� �����ؾ� ������, ������ٵ� ������ �ʴ� ��쿡�� Update���� �����ϴ� ���� �� ���� �� �ֽ��ϴ�.
	///
	/// ���⼭ <see cref="MovementUpdate"/> �޼���(�� �޼���� <see cref="MovementUpdateInternal)"/>�� ȣ���մϴ�)�� ȣ���� �̷������, 
	/// �� �޼��忡�� �ֵ� �ڵ尡 ���ԵǾ� ������ AI�� �̵��� *���ϴ�* ����� ����մϴ�.
	/// �׷��� ���� �̵��� �������� �ʽ��ϴ�. ��� �� �������� ���� AI�� �̵��Ϸ��� ��ġ�� ȸ���� ��ȯ�մϴ�.
	/// <see cref="Update"/> (�Ǵ� <see cref="FixedUpdate)"/> �޼���� �̷��� ���� <see cref="FinalizeMovement"/> �޼��忡 �����ϸ�, ���� ĳ���� �̵��� ����մϴ�.
	/// �� �޼���� ���� ����ĳ������ ����Ͽ� AI�� ���鿡�� �������� �ʵ��� �ϴ� �۾��� ���� �۾��� ó���մϴ�.
	///
	/// AI�� ���������� ��θ� �ٽ� ����մϴ�. �̰��� Update �޼��忡�� <see cref="shouldRecalculatePath"/>�� Ȯ���ϰ� true�� ��ȯ�ϸ� <see cref="SearchPath"/>�� ȣ���մϴ�.
	/// <see cref="SearchPath"/> �޼���� ��� ��û�� �غ��ϰ� �� ��ũ��Ʈ�� ������ GameObject�� ����� <see cref="Pathfinding.Seeker"/> ���� ��ҿ��� �����ϴ�.
	/// �� ��ũ��Ʈ�� ��󺰷� <see cref="Pathfinding.Seeker.pathCallback"/> �븮�ڿ� ����ϹǷ� �� ��ũ��Ʈ�� ��ΰ� <see cref="OnPathComplete"/> �޼��尡 ȣ��� ������ ���� ������ �����ް� �˴ϴ�.
	/// ��ΰ� ���Ǵ� �� �� ������ �Ǵ� ���� �������� �ɸ� �� ������, ���������� <see cref="OnPathComplete"/> �޼��尡 ȣ��ǰ� AI�� ������ ���� ��ΰ� ��ü�˴ϴ�.
	/// </summary>
	[AddComponentMenu("Pathfinding/AI/AIPath (2D,3D)")]
	public partial class AIPath : AIBase, IAstarAI {
		/// <summary>
		/// ������Ʈ�� ���ӵ��Դϴ�.
		/// ��� ���� �ʴ� ���� ������ ������ ��Ÿ���ϴ�.
		/// ���� ���� ������Ʈ�� �ִ� �ӵ��� �����ϴ� �� �ɸ��� �ð��� ������ �ؼ��˴ϴ�.
		/// ���� ��� ������Ʈ�� �ִ� �ӵ��� �����ϴ� �� �� 0.4�ʰ� �ɸ��ٸ� �� �ʵ�� -1/0.4 = -2.5�� �����ؾ� �մϴ�.
		/// ���� ���� ��� ���� ���ӵ��� ������ ���� ���˴ϴ�: -���ӵ�*�ִ� �ӵ�.
		/// �� ������ �ַ� ȣȯ���� ���� �����մϴ�.
		///
		/// Unity �ν����Ϳ����� �� ���� ��尡 �ֽ��ϴ�. Default �� Custom ����Դϴ�. Default ��忡���� �� �ʵ尡 -2.5�� �����Ǿ� ������Ʈ�� �ִ� �ӵ��� �����ϴ� �� �� 0.4�ʰ� �ɸ��ϴ�.
		/// Custom ��忡���� ���ӵ��� ���� ������ ������ �� �ֽ��ϴ�.
		/// </summary>
		public float maxAcceleration = -2.5f;

		/// <summary>
		/// �ʴ� ȸ�� �ӵ�(�� ����).
		/// ȸ���� Quaternion.RotateTowards�� ����Ͽ� ���˴ϴ�. �� ������ �ʴ� ȸ�� �ӵ��� ��Ÿ���ϴ�.
		/// ���� �������� ĳ���Ͱ� ������ ȸ���� �� �ֽ��ϴ�.
		/// </summary>
		[UnityEngine.Serialization.FormerlySerializedAs("turningSpeed")]
		public float rotationSpeed = 360;

		/// <summary>��� ���������� ������Ʈ�� ������ ������ �Ÿ�</summary>
		public float slowdownDistance = 0.6F;

		/// <summary>
		/// AI�� ��� �󿡼� �̵� ������ �����ϴ� �� �󸶳� �ָ� ���� �ٶ󺸴��� ��Ÿ���� ���Դϴ�.
		/// ���� ������ ǥ�õ˴ϴ�.
		/// ���� <see cref="alwaysDrawGizmos"/> ����� Ȱ��ȭ�ϸ��� ���� ������Ʈ �ֺ��� �Ķ� ������ �ð�ȭ�˴ϴ�.
		/// [�¶��� �������� �̹����� ������ ����]
		///
		/// ���⿡�� �� ���� �ʹ� ���ų� �ʹ� ���� ���� �Ϲ����� ����� �Բ� �� ���� ���� ������ ���� �ֽ��ϴ�.
		/// <table>
		/// <tr><td>[�¶��� �������� ������ ������ ����]</td><td>\xmlonly <verbatim><span class="label label-danger">�ʹ� ����</span><br/></verbatim>\endxmlonly> �ʹ� ���� ���� �ʹ� ���� ���ӵ��� ������Ʈ�� ���� overshooting�ϰ� ��θ� ������ ���ϴ� ����� ���� �� �ֽ��ϴ�.</td></tr>
		/// <tr><td>[�¶��� �������� ������ ������ ����]</td><td>\xmlonly <verbatim><span class="label label-warning">������</span><br/></verbatim>\endxmlonly> ���� �������� ���� ���ӵ��� AI�� ��θ� �� ������ �������� �۵��Ͽ� ���� ����� ������ �� �ֽ��ϴ�. ��θ� ������ ���� ���ϴ� ��� <see cref="Pathfinding.AILerp"/> ������Ʈ�� �� �����մϴ�.</td></tr>
		/// <tr><td>[�¶��� �������� ������ ������ ����]</td><td>\xmlonly <verbatim><span class="label label-success">������</span><br/></verbatim></span> �� �������� �ո����� ���Դϴ�.</td></tr>
		/// <tr><td>[�¶��� �������� ������ ������ ����]</td><td>\xmlonly <verbatim><span class="label label-success">������</span><br/></verbatim>\endxmlonly> �� ���������� �ո����� �������� ��ΰ� ���� �������� �ణ �� �����ϰ� ���󰩴ϴ�.</td></tr>
		/// <tr><td>[�¶��� �������� ������ ������ ����]</td><td>\xmlonly <verbatim><span class="label label-danger">�ʹ� ����</span><br/></verbatim>\endxmlonly> �ʹ� ���� ���� ������Ʈ�� ��θ� �ʹ� �����ϰ� ������ ��ֹ��� ����Ϸ� �õ��� �� �ֽ��ϴ�.</td></tr>
		/// </table>
		/// </summary>
		public float pickNextWaypointDist = 2;

		/// <summary>
		/// ����� �������� ���޷� ���ֵǴ� �Ÿ��Դϴ�.
		/// ������ �� �Ÿ� ���� ������ <see cref="OnTargetReached"/>�� ȣ��ǰ� <see cref="reachedEndOfPath"/>�� true�� ��ȯ�մϴ�.
		/// </summary>
		public float endReachedDistance = 0.2F;

		/// <summary>���õ� ������Ʈ�� ������ ���� ���� ������ �� �信�� �ڼ��� gizmo�� ��� �׸��ϴ�.</summary>
		public bool alwaysDrawGizmos;

		/// <summary>
		/// ��� ������ ������ �ʾ��� �� �����մϴ�.
		/// �ణ�� ���� ������尡 �߻��մϴ�.
		/// </summary>
		public bool slowWhenNotFacingTarget = true;

		/// <summary>
		/// ��󿡼� <see cref="endReachedDistance"/> ���� ���� �� ������ ���� �����մϴ�.
		/// ĳ���ʹ� �� �Ÿ� ���� ������ �� ��� ���� �� ������, Ȱ�� ��ų� ��� �����ϴ� Ȱ�� ����ϴ� �û�� ���� ��� �����մϴ�.
		/// �Ǵ� ĳ���ʹ� ��Ȯ�� ��� ������ �����Ϸ��� �õ��ϰ� �ű⿡�� ������ ���� �� �ֽ��ϴ�. �̰��� ������ ��� ������ �����Ϸ��� ��� �����մϴ�.
		///
		/// ����: �� �ʵ尡 �����Ǿ� ���� �ʴ��� <see cref="reachedEndOfPath"/>�� ��󿡼� <see cref="endReachedDistance"/> ���� ���� ���� �� true�� �˴ϴ�.
		/// </summary>
		public CloseToDestinationMode whenCloseToDestination = CloseToDestinationMode.Stop;


		/// <summary>
		/// ĳ���Ͱ� �׻� �׺���̼� �޽��� Ž�� ������ ǥ�鿡 �ִ��� Ȯ���մϴ�.
		/// �� �ɼ��� Ȱ��ȭ�ϸ� �� �����Ӹ��� <see cref="AstarPath.GetNearest"/> ������ ����Ǿ� ������Ʈ�� ���� �� �ִ� ���� ����� ��带 ã���ϴ�.
		/// �׸��� ������Ʈ�� �� ��� �ȿ� ���� ������ ������Ʈ�� �ش� ��ġ�� �̵��մϴ�.
		///
		/// Ư�� ���� ȸ�ǿ� �Բ� ����ϸ� ������Ʈ�� ���θ� ���� �о�ִ� ���� �����ϴ� �� �����մϴ�.
		/// �� ���� ������ ������: ���� ���ϴ� (�¶��� �������� �۵� ��ũ ����)�� �����ϼ���.
		///
		/// �� �ɼ��� ���� ȸ�ǿ� ���յǾ� �־� �ٸ� ������Ʈ�� ���� ���� ������ �з����� ���� ȸ�� �ý����� �ش� ���� ����� �� �ֽ��ϴ�.
		///
		/// �� �ɼ��� Ȱ��ȭ�ϸ� �׷��� ������ ���� ���� ������ �ֽ��ϴ� (�׸��� �׷����� ��� �� ������, �׺�޽�/��ĳ��Ʈ �׷����� ��� �ణ �����ϴ�).
		/// �׺�޽�/��ĳ��Ʈ �׷����� ����ϴ� ��� �׺�޽�/��ĳ��Ʈ �׷��� �������� �ۼ��� <see cref="Pathfinding.RichAI"/> �̵� ��ũ��Ʈ�� ��ȯ�ϴ� ���� �����ϴ�.
		/// ���� ��쿡 ��� ������ ��θ� �� �ε巴�� ���� �� �ֽ��ϴ�.
		///
		/// �׸��� �׷����� ��� �� �ɼ��� ��� ������̾�� �Բ� ������� �ʴ� ���� �����ϴ�. ��� ������̾�� ��θ� �׷����� �����ڸ��� �ſ� ������ ����� ������
		/// �� ��ũ��Ʈ�� �ణ �𼭸��� �߶� ������ �õ��� ���ɼ��� �ֽ��ϴ�. �̷��� �ϸ� �𼭸� ��ó�� Ž�� ������ ǥ�� ������ �������� �õ��ϴ� ��ó�� ���� ���Դϴ�.
		/// �� �ɼ��� Ȱ��ȭ�ϸ� �̷��� �ϸ� �ȵ˴ϴ�.
		///
		/// ���: �� �ɼ��� ����Ʈ �׷������� ����� �� �ǹ̰� �����ϴ�. ����Ʈ �׷������� ǥ���� �����ϴ�.
		/// ����Ʈ �׷����� ����� �� �� �ɼ��� Ȱ��ȭ�ϸ� ������Ʈ�� �� �����Ӹ��� ���� ����� ���� �̵��Ǹ�, �̴� �Ϲ������� ���ϴ� ������ �ƴ� ���Դϴ�.
		///
		/// �Ʒ� �̹������� ���� ȸ�Ǹ� ����ϴ� ���� ������Ʈ�� ��� �𼭸����ִ� ������ �������� �̵��ϵ��� ���õǾ����ϴ�.
		/// ������Ʈ�� �׷����� ������ ���� ������ ���� ��ֹ� ������ �з����ϴ�.
		/// [�¶��� �������� �̹��� ����]
		/// </summary>
		public bool constrainInsideGraph = false;

		/// <summary>���� ������ ���</summary>
		protected Path path;

		/// <summary>���� ��θ� ���� ���� ����ϴ� �����</summary>
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

				// 2D ��忡���� ���� �˻縦 �������� �ʽ��ϴ�.
				if (orientation != OrientationMode.YAxisForward) {
					// ����� ĳ������ �Ӹ� �� �Ǵ� �� �Ʒ��� �ſ� �ָ� �ִ��� Ȯ���մϴ�.
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

			// ���� ��θ� �����Ͽ� Ǯ�� ��ȯ�մϴ�.
			if (path != null) path.Release(this);
			path = null;
			interpolator.SetPath(null);
			reachedEndOfPath = false;
		}

		/// <summary>
		/// ����� ���� �����߽��ϴ�.
		/// AI�� �������� �������� �� ����� ���� ������ �߰��Ϸ��� �̰��� �߰��մϴ�. ���� �̸� ����� �� ��ũ��Ʈ�� ����� �ش� ��ũ��Ʈ���� �� �Լ��� �������� ���� �ֽ��ϴ�.
		///
		/// �� �޼���� �� ��ΰ� ���Ǹ� ȣ��˴ϴ�. �������� ����� �� �ֱ� �����Դϴ�.
		/// ���� ������Ʈ�� �������� ����� �� �� �޼���� �Ϲ������� �� <see cref="repathRate"/> �ʸ��� ȣ��˴ϴ�.
		/// </summary>
		public virtual void OnTargetReached () {
		}

		/// <summary>
		/// ��û�� ��ΰ� ���Ǿ��� �� ȣ��˴ϴ�.
		/// ��δ� ���� <see cref="UpdatePath"/>�� ���� ��û�ǰ�, �Ƹ��� ������ �������̳� ���� �����ӿ��� ���˴ϴ�.
		/// ���������� �� �Լ��� ���޵˴ϴ�.
		/// </summary>
		protected override void OnPathComplete(Path newPath)
		{
			ABPath p = newPath as ABPath;

			if (p == null) throw new System.Exception("�� �Լ��� ABPaths�� ó���ϸ� Ư�� ��� ������ ������� ���ʽÿ�.");

			waitingForPathCalculation = false;

			// �� ����� ���� Ƚ���� ������ŵ�ϴ�.
			// �̰��� �Ҵ緮�� ���̱� ���� ��ü Ǯ���� ���˴ϴ�.
			p.Claim(this);

			// ��ΰ� � �����ε� ������ ���߽��ϴ�.
			// �ڼ��� ������ p.errorLog(����� ���ڿ�)�� �ֽ��ϴ�.
			if (p.error)
			{ 
				p.Release(this);
				SetPath(null);
				return;
			}

			// ���� ��� ����
			if (path != null) path.Release(this);

			// ���� ��� ��ü
			path = p;

			// ��ο� �ּ��� 2���� ������ ���Եǵ��� �մϴ�.
			if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
			interpolator.SetPath(path.vectorPath);

			var graph = path.path.Count > 0 ? AstarData.GetGraph(path.path[0]) as ITransformedGraph : null;
			movementPlane = graph != null ? graph.transform : (orientation == OrientationMode.YAxisForward ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 270, 90), Vector3.one)) : GraphTransform.identityTransform);

			// �Ϻ� ���� �缳��
			reachedEndOfPath = false;

			// ��θ� ��û�� �������� ���� ��ġ���� �̵��� �ùķ���Ʈ�մϴ�.
			// �̷��� �ϸ� ������Ʈ�� ����� ù ��° ������ ���� ��ġ���� �ָ� ������ ��쿡 ȥ������������ ������ �پ��ϴ�.
			interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint) * 0.5f);
			interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

			// �̵��� ������ ������Ʈ�մϴ�.
			// ����� �̰��� ���⼭ �����ؾ��մϴ�. �׷��� ������ remainingDistance �ʵ尡 1������ ���� �߸��� �� �ֽ��ϴ�.
			// (interpolator.remainingDistance�� ��Ȯ���� �ʱ� ����).
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

		/// <summary>Update �Ǵ� FixedUpdate �߿� ȣ��˴ϴ�. ��ü �̵��� ������ �ٵ� ����ϴ��� ���ο� ���� �ٸ��ϴ�.</summary>
		protected override void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
		{
			float currentAcceleration = maxAcceleration;

			// ������ ��� �ִ� �ӵ����� ������ ����մϴ�.
			if (currentAcceleration < 0) currentAcceleration *= -maxSpeed;

			if (updatePosition)
			{
				// ������ �� ���� transform.position���� �б� ������ ���� �������ٴ� transform.position���� �н��ϴ�.
				// (��� ���� ������ ���� ��������� �����ϴ�)
				simulatedPosition = tr.position;
			}
			if (updateRotation) simulatedRotation = tr.rotation;

			var currentPosition = simulatedPosition;

			// �̵��� ������ ������Ʈ�մϴ�.
			interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
			var dir = movementPlane.ToPlane(steeringTarget - currentPosition);

			// ��� �������� �Ÿ��� ����մϴ�.
			float distanceToEnd = dir.magnitude + Mathf.Max(0, interpolator.remainingDistance);

			// ��ǥ�� �����ߴ��� Ȯ���մϴ�.
			var prevTargetReached = reachedEndOfPath;
			reachedEndOfPath = distanceToEnd <= endReachedDistance && interpolator.valid;
			if (!prevTargetReached && reachedEndOfPath) OnTargetReached();
			float slowdown;

			// ������Ʈ�� �����ִ� ������ ����ȭ�� ����
			var forwards = movementPlane.ToPlane(simulatedRotation * (orientation == OrientationMode.YAxisForward ? Vector3.up : Vector3.forward));

			// ��ȿ�� ��θ� ����� �ϰ� �ٸ� ��ũ��Ʈ�� ĳ���͸� �������� �ʾҴ��� Ȯ���մϴ�.
			bool stopped = isStopped || (reachedDestination && whenCloseToDestination == CloseToDestinationMode.Stop);
			if (interpolator.valid && !stopped)
			{
				// �������� ��������� ���� �����̴� �ӵ��� ����մϴ�.
				// ĳ���Ͱ� �������� ����������� �� ������ �̵��մϴ�.
				// �׻� 0�� 1 ������ ���Դϴ�.
				slowdown = distanceToEnd < slowdownDistance? Mathf.Sqrt(distanceToEnd / slowdownDistance) : 1;

				if (reachedEndOfPath && whenCloseToDestination == CloseToDestinationMode.Stop) {
					// Slow down as quickly as possible
					velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
				} else {
					velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir, dir.normalized*maxSpeed, velocity2D, currentAcceleration, rotationSpeed, maxSpeed, forwards) * deltaTime;
				}
			} else {
				slowdown = 1;
				// ������ ���� �����մϴ�.
				velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
			}

			velocity2D = MovementUtilities.ClampVelocity(velocity2D, maxSpeed, slowdown, slowWhenNotFacingTarget && enableRotation, forwards);

			ApplyGravity(deltaTime);


			// �� ������ ���� ������Ʈ�� �̵��Ϸ��� ���� �����մϴ�.
			var delta2D = lastDeltaPosition = CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);
			nextPosition = currentPosition + movementPlane.ToWorld(delta2D, verticalVelocity * lastDeltaTime);
			CalculateNextRotation(slowdown, out nextRotation);
		}

		protected virtual void CalculateNextRotation (float slowdown, out Quaternion nextRotation) {
			if (lastDeltaTime > 0.00001f && enableRotation) {
				Vector2 desiredRotationDirection;
				desiredRotationDirection = velocity2D;

				// �츮�� �����̴� ������ ���� ȸ���մϴ�.
				// ��ǥ ������ �ſ� ����� ���� ȸ������ �ʽ��ϴ�.
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

				// �츮�� �ܼ��� ��ȣ�� Ȯ���� �� �����ϴ�. ��� ���е��� �սǵ� �� �ֱ� �����Դϴ�.
				var difference = movementPlane.ToPlane(clampedPosition - position);
				float sqrDifference = difference.sqrMagnitude;
				if (sqrDifference > 0.001f*0.001f) {
					// ������Ʈ�� �׺�޽� �ٱ��� �־����ϴ�. ���� �������θ� �̵� �ӵ��� �̵��ϵ��� �Ͽ�
					// �ӵ��� ���� ���� �̵����� �ʵ��� �մϴ�.
					velocity2D -= difference * Vector2.Dot(difference, velocity2D) / sqrDifference;

					positionChanged = true;
					// �� ��ġ�� ��ȯ�մϴ�. �׷��� ClampToNavmesh �޼��忡�� y ��ǥ�� ���� ������ �����մϴ�. navmesh�� y ��ǥ�� �밳 �ſ� ��Ȯ���� �ʱ� �����Դϴ�.
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
				// �׻� Gizmo�� ǥ�õ� �� Scene �䰡 �ٽ� �׷������� �մϴ�.
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
			// �ణ�� ����: ���� ���� �ʴ� ȸ�� ������ �뷫 ��ȯ�մϴ�.
			if (version < 1) rotationSpeed *= 90;
			return base.OnUpgradeSerializedData(version, unityThread);
		}

        public void Move(Vector3 deltaPosition)
        {
            throw new System.NotImplementedException();
        }
    }
}
