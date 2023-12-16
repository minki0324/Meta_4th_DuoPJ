using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Serialization;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	/// <summary>
	/// �׺���̼� �޽� ��(Navmesh Cut) ��ü�� ����� Ŭ�����Դϴ�.
	/// Navmesh Cut�� �̵��� ���� �����ϰ� �׷��� ���� ������ ����ϱ� ���� �׷��� ������Ʈ�� �����ϴ� ������ �մϴ�.
	///
	/// ����: �׺�޽� ���� (�¶��� �������� �۵� ��ũ Ȯ�� ����)
	/// ����: <see cref="AstarPath.navmeshUpdates"/>
	/// ����: <see cref="Pathfinding.NavmeshBase.enableNavmeshCutting"/>
	/// </summary>
	[System.Serializable]
	public class NavmeshUpdates {
        /// <summary>
        /// ������Ʈ�� �ʿ����� Ȯ���ϴ� �� (���� �� ���� ����).
        /// �ſ� ���� NavmeshCut ��ü�� �ִ� ������ ��� �� �����Ӹ��� Ȯ���ϴ� ���� ���ɿ� �ǿ����� �� �� �ֽ��ϴ�.
        /// ���� ������� �����Ǹ� �� ��ȣ�� �������� �� ���� Ȯ���ϵ��� ������ �� �ֽ��ϴ�.
        ///
        /// ��κ��� ���ӿ��� �� ���� 0���� �����ص� �˴ϴ�.
        ///
        /// ������ ��� �ڵ� ������Ʈ�� ������� �ʽ��ϴ�. <see cref="ForceUpdate"/>�� ����Ͽ� �������� ������Ʈ�ؾ� �մϴ�.
        ///
        /// <code>
        /// // �� �����Ӹ��� Ȯ�� (�⺻��)
        /// AstarPath.active.navmeshUpdates.updateInterval = 0;
        ///
        /// // 0.1�ʸ��� Ȯ��
        /// AstarPath.active.navmeshUpdates.updateInterval = 0.1f;
        ///
        /// // ���� ���� Ȯ������ ����
        /// AstarPath.active.navmeshUpdates.updateInterval = -1;
        /// // �������� ������Ʈ�� �����ؾ� �մϴ�.
        /// AstarPath.active.navmeshUpdates.ForceUpdate();
        /// </code>
        ///
        /// �� ������ AstarPath �ν����� ���������� ã�� �� �ֽ��ϴ�.
        /// [�¶��� �������� �̹��� Ȯ�� ����]
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
