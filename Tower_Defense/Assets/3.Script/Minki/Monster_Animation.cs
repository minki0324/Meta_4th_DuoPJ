using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Monster_Animation : NetworkBehaviour
{
    /*
        1. ���� �ִϸ��̼� ���� ��ũ��Ʈ
        2. �⺻ ����Ʈ�� Walk // Bool���� ���� Fly ���� ��Ʈ�� 
        3. �׾��� �� ����� Trigger�� Dead
        4. �׾��� �� ��� ������� Apply Root Motion Ȱ��ȭ, Ǯ������ �ٽ� �����Ҷ� ���ֱ�
    */

    [SerializeField] private Monster_Animation monster_Animation;
    [SerializeField] private NetworkAnimator ani;


    #region Unity Callback
    private void Update()
    {
        /*
            ���� �״� ���ǿ� ���缭 Client �޼ҵ� ȣ���ϱ�
        */
    }

    #endregion
    #region SyncVar
    #endregion
    #region Client
    [Client]
    private void OnDead()
    {
        // �ʱ�ȭ ���ǵ� �ϳ��� �־������ (�ݶ��̴�, ���̾�, ���)
        bool currentRootMotionState = ani.animator.applyRootMotion; // ���� �� ��Ʈ��� ���� �ڿ������� ����
        ani.animator.applyRootMotion = !currentRootMotionState;
        ani.animator.SetTrigger("Dead");
    }
    #endregion
    #region Command
    #endregion
    #region ClientRPC
    #endregion
    #region Hook Method
    #endregion

}

/*
    #region Unity Callback
    #endregion
    #region SyncVar
    #endregion
    #region Client
    #endregion
    #region Command
    #endregion
    #region ClientRPC
    #endregion
    #region Hook Method
    #endregion
 */
