using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pathfinding {
	/// <summary>
	/// 특정한 객체의 위치로 AI의 목적지를 설정합니다.
	/// 이 컴포넌트는 AIPath, RichAI 또는 AILerp와 같은 이동 스크립트와 함께 GameObject에 부착되어야 합니다.
	/// 그런 다음 이 컴포넌트에 설정된 <see cref="target"/>으로 AI를 이동시킵니다.
	///
	/// 참조: <see cref="Pathfinding.IAstarAI.destination"/>
	///
	/// [이미지를 보려면 온라인 문서를 열어보세요]
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
			// 경로를 검색하기 직전에 목적지를 업데이트합니다.
			// 이론적으로 이것으로 충분하지만, 이 스크립트는 디버깅을 위해 사용되며
			// 다른 스크립트에서 다른 용도로 사용될 수도 있으므로 매 프레임 목적지를 업데이트하는 것이
			// 의미가 있습니다. 따라서 매 프레임마다 최신 상태를 유지하는 것이 좋습니다.
			if (ai != null) ai.onSearchPath += Update;
		}

		void OnDisable () {
			if (ai != null) ai.onSearchPath -= Update;
		}

		/// <summary>매 프레임 AI의 목적지를 업데이트합니다</summary>
		void Update () {
            if (target != null && ai != null) ai.destination = target.position;
        }
	}
}
