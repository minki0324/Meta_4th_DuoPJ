using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Monster_Animation : NetworkBehaviour
{
    /*
        1. 몬스터 애니메이션 관련 스크립트
        2. 기본 디폴트는 Walk // Bool값에 따라 Fly 몬스터 컨트롤 
        3. 죽었을 때 모션은 Trigger로 Dead
        4. 죽었을 때 모션 출력전에 Apply Root Motion 활성화, 풀링으로 다시 생성할때 꺼주기
    */

    [SerializeField] private Monster_Animation monster_Animation;
    [SerializeField] private NetworkAnimator ani;


    #region Unity Callback
    private void Update()
    {
        /*
            몬스터 죽는 조건에 맞춰서 Client 메소드 호출하기
        */
    }

    #endregion
    #region SyncVar
    #endregion
    #region Client
    [Client]
    private void OnDead()
    {
        // 초기화 조건들 하나씩 넣어줘야함 (콜라이더, 레이어, 등등)
        bool currentRootMotionState = ani.animator.applyRootMotion; // 죽을 때 루트모션 꺼야 자연스럽게 죽음
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
