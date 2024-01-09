using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Pathfinding;

public class Monster_Spawn : NetworkBehaviour
{
    /*
        몬스터 스폰 및 오브젝트 풀링 스크립트
        1. 플레이어 프리팹에 있는 스폰 포인트의 랜덤 구역에서 몬스터 스폰
        2. 리스트에 담아놓고 비활성화 되어있는 몬스터가 있으면 해당 몬스터를 활성화하면서 스폰
    */

    public Transform[] SpawnPoint;
    public Transform[] FinPoint;

    [SerializeField] private Monster_Pooling pools;
    [SerializeField] private GameObject[] Mon_Prefabs;
    [SerializeField] private Income income;
    [SerializeField] private Resourse resourse;
    #region SyncVar
    
    public SyncList<Monster_Control> AllMonster = new SyncList<Monster_Control>();

    #endregion
    #region Unity Callback
    #endregion
    #region Client
    [Client]
    public void Onclick(int index)
    {

        //소지금확인
        CheckCost(index);
       
    }

    private void CheckCost(int index)
    {
        float Cost = Mon_Prefabs[index].GetComponent<Monster_Control>().state.cost;
        if (Cost <= resourse.current_mineral) //소지금이 더많으면 스폰
        {
            resourse.current_mineral -= (int)Cost ;
            CMD_SpawnMonster(index, (int)GameManager.instance.Player_Num);
        }
        else //없으면 리턴
        {
            //에러메세지

            return;
        }

    }
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    private void CMD_SpawnMonster(int index, int player_num)
    {
        GameObject monster = pools.GetMonster(index);
        Monster_Control monster_con = monster.GetComponent<Monster_Control>();
        monster_con.currentLineNum = player_num + 1;
        while (Life_Manager.instance.IsPlayerDead(monster_con.currentLineNum) || monster_con.currentLineNum == player_num)
        {
            monster_con.currentLineNum++;
            if (monster_con.currentLineNum > 4)
            {
                monster_con.currentLineNum = 1;
            }
            //적들의 isDie 불값을 모두가져와서 모두 죽었을때 break걸고 승리조건 만들어야함
            if (Life_Manager.instance.isVectoryCheck(player_num))
            {
                Debug.Log(player_num + "게임종료로 인해 스폰이 불가능합니다.");
                return;
            }

        }
        // 초기화 메소드 가져와야됨
        Transform spawnpoint = Get_SpawnPoint(monster_con.currentLineNum);
        Transform finpoint = Get_FinPoint(monster_con.currentLineNum);
        int randIndexX = Random.Range(-10, 11);
        int randIndexZ = Random.Range(-1, 2);
        Vector3 spawnPos = new Vector3(spawnpoint.position.x + randIndexX, spawnpoint.position.y+ Mon_Prefabs[index].transform.position.y, spawnpoint.position.z + randIndexZ);
        monster.transform.SetPositionAndRotation(spawnPos, Mon_Prefabs[index].transform.rotation);
        //NetworkServer.Spawn(monster);
        // 태그 할당
        monster.tag = $"{player_num}P";
        Plus_Income(index, player_num);



        AllMonster.Add(monster_con);
        //플라이는 에이스타안씀
        if (monster_con.state.type != MonsterState.monType.Fly &&
            monster_con.state.type != MonsterState.monType.Attack
            )
        {
            if (monster_con.aiPath == null)
            {
                Debug.Log(monster_con.aiPath);
                monster_con.GetComponent<AIPath>().isStopped = false;
            }
            else
            {
                monster_con.aiPath.isStopped = false;
            }
            monster_con.Astar.target = finpoint;
        }


        Rpc_SpawnMonster(monster, player_num);
    }
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void Rpc_SpawnMonster(GameObject mon, int player_num)
    {
        if (!isServer)
        {
            // 태그 할당
            mon.tag = $"{player_num}P";
        }
    }
    #endregion
    #region Hook Method
    #endregion

    private Transform Get_SpawnPoint(int player_num)
    {
        switch(player_num)
        {
            /*
                나중에 플레이어 목숨 다하면 다음 플레이어로 소환하는 로직 필요함 Todo 
            */
            case 1:
                return SpawnPoint[0];
            case 2:
                return SpawnPoint[1];
            case 3:
                return SpawnPoint[2];
            case 4:
                return SpawnPoint[3];
            default:
                return null;
        }
    }
    private Transform Get_FinPoint(int player_num)
    {
        switch (player_num)
        {
            /*
                나중에 피니시라인 넘어갔을 때 그 다음 플레이어에게 가는 로직 필요함 Todo
            */
            case 1:
                return FinPoint[0];
            case 2:
                return FinPoint[1];
            case 3:
                return FinPoint[2];
            case 4:
                return FinPoint[3];
            default:
                return null;
        }
    }

    private void Plus_Income(int index, int player_num)
    {
        int incomeIncrease = 1;

        switch (player_num)
        {
            case 1:
                income.P1_income += incomeIncrease;
                break;
            case 2:
                income.P2_income += incomeIncrease;
                break;
            case 3:
                income.P3_income += incomeIncrease;
                break;
            case 4:
                income.P4_income += incomeIncrease;
                break;
        }
    }
}
