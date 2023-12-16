using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Serialization;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	/// <summary>
	/// 네비게이션 메시 컷(Navmesh Cut) 객체의 도우미 클래스입니다.
	/// Navmesh Cut가 이동한 것을 추적하고 그러한 변경 사항을 고려하기 위한 그래프 업데이트를 조정하는 역할을 합니다.
	///
	/// 참조: 네비메쉬 컷팅 (온라인 문서에서 작동 링크 확인 가능)
	/// 참조: <see cref="AstarPath.navmeshUpdates"/>
	/// 참조: <see cref="Pathfinding.NavmeshBase.enableNavmeshCutting"/>
	/// </summary>
	[System.Serializable]
	public class NavmeshUpdates {
        /// <summary>
        /// 업데이트가 필요한지 확인하는 빈도 (실제 초 단위 간격).
        /// 매우 많은 NavmeshCut 객체가 있는 월드의 경우 이 프레임마다 확인하는 것은 성능에 악영향을 줄 수 있습니다.
        /// 성능 문제라고 생각되면 이 번호를 증가시켜 덜 자주 확인하도록 설정할 수 있습니다.
        ///
        /// 대부분의 게임에서 이 값을 0으로 유지해도 됩니다.
        ///
        /// 음수인 경우 자동 업데이트가 수행되지 않습니다. <see cref="ForceUpdate"/>를 사용하여 수동으로 업데이트해야 합니다.
        ///
        /// <code>
        /// // 매 프레임마다 확인 (기본값)
        /// AstarPath.active.navmeshUpdates.updateInterval = 0;
        ///
        /// // 0.1초마다 확인
        /// AstarPath.active.navmeshUpdates.updateInterval = 0.1f;
        ///
        /// // 변경 사항 확인하지 않음
        /// AstarPath.active.navmeshUpdates.updateInterval = -1;
        /// // 수동으로 업데이트를 예약해야 합니다.
        /// AstarPath.active.navmeshUpdates.ForceUpdate();
        /// </code>
        ///
        /// 이 설정은 AstarPath 인스펙터 설정에서도 찾을 수 있습니다.
        /// [온라인 문서에서 이미지 확인 가능]
        /// </summary>
        public float updateInterval;

		internal class NavmeshUpdateSettings {
			public NavmeshUpdateSettings(NavmeshBase graph) {}
			public void OnRecalculatedTiles (NavmeshTile[] tiles) {}
		}
		internal void Update () {}
		internal void OnEnable () {}
		internal void OnDisable () {}
	}
}
