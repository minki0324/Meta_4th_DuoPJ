using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pathfinding {
	/// <summary>
	/// Ư���� ��ü�� ��ġ�� AI�� �������� �����մϴ�.
	/// �� ������Ʈ�� AIPath, RichAI �Ǵ� AILerp�� ���� �̵� ��ũ��Ʈ�� �Բ� GameObject�� �����Ǿ�� �մϴ�.
	/// �׷� ���� �� ������Ʈ�� ������ <see cref="target"/>���� AI�� �̵���ŵ�ϴ�.
	///
	/// ����: <see cref="Pathfinding.IAstarAI.destination"/>
	///
	/// [�̹����� ������ �¶��� ������ �������]
	/// </summary>
	[UniqueComponent(tag = "ai.destination")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_a_i_destination_setter.php")]
	public class AIDestinationSetter : MonoBehaviour {
		/// <summary>The object that the AI should move to</summary>
		public Transform target;
		IAstarAI ai;
		

		void OnEnable () {
			ai = GetComponent<IAstarAI>();
			GameObject player = GameObject.Find("Player");
			// ��θ� �˻��ϱ� ������ �������� ������Ʈ�մϴ�.
			// �̷������� �̰����� ���������, �� ��ũ��Ʈ�� ������� ���� ���Ǹ�
			// �ٸ� ��ũ��Ʈ���� �ٸ� �뵵�� ���� ���� �����Ƿ� �� ������ �������� ������Ʈ�ϴ� ����
			// �ǹ̰� �ֽ��ϴ�. ���� �� �����Ӹ��� �ֽ� ���¸� �����ϴ� ���� �����ϴ�.
			if (ai != null) ai.onSearchPath += Update;
		}

		void OnDisable () {
			if (ai != null) ai.onSearchPath -= Update;
		}

		/// <summary>�� ������ AI�� �������� ������Ʈ�մϴ�</summary>
		void Update () {
            if (target != null && ai != null) ai.destination = target.position;
        }
	}
}
