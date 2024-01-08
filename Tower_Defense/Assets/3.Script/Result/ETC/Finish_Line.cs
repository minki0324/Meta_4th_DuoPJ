using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Pathfinding;

public class Finish_Line : NetworkBehaviour
{
    [SerializeField] private Monster_Spawn point;

    #region Unity Callback
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 && isServer)
        {
            GameObject mon = other.gameObject;
            Monster_Control mon_con = other.GetComponent<Monster_Control>();
            AIDestinationSetter monAI = mon.GetComponent<AIDestinationSetter>();
            mon_con.goalCount++;
            int player_num = Convert_num(other);
            int col_num = Convert_num(gameObject.GetComponent<Collider>());
            int index = (mon_con.goalCount % 3);

            Life_Manager.instance.Life_Set(col_num, player_num);

            switch (col_num)
            {
                // 나중에 플레이어 죽으면 건너뛰는 로직 넣어야됨
                case 1: // 1p 피니시라인
                    if (player_num == 2) // 2p 몬스터일때
                    {
                        mon.transform.position = point.SpawnPoint[2].position + NextRandomSpawnPos(mon);
                        if(mon_con.state.type != MonsterState.monType.Fly)
                        monAI.target = point.FinPoint[2];
                    }
                    else
                    {
                        mon.transform.position = point.SpawnPoint[1].position + NextRandomSpawnPos(mon);
                        if (mon_con.state.type != MonsterState.monType.Fly)
                            monAI.target = point.FinPoint[1];
                    }
                    break;
                case 2: // 2p 피니시라인
                    if (player_num == 3) // 3p 몬스터일때
                    {
                        mon.transform.position = point.SpawnPoint[3].position + NextRandomSpawnPos(mon);
                        if (mon_con.state.type != MonsterState.monType.Fly)
                            monAI.target = point.FinPoint[3];
                    }
                    else
                    {
                        mon.transform.position = point.SpawnPoint[2].position + NextRandomSpawnPos(mon);
                        if (mon_con.state.type != MonsterState.monType.Fly)
                            monAI.target = point.FinPoint[2];
                    }
                    break;
                case 3: // 3p 피니시라인
                    if (player_num == 4) // 4p 몬스터일때
                    {
                        mon.transform.position = point.SpawnPoint[0].position + NextRandomSpawnPos(mon);
                        if (mon_con.state.type != MonsterState.monType.Fly)
                            monAI.target = point.FinPoint[0];
                    }
                    else
                    {
                        mon.transform.position = point.SpawnPoint[3].position + NextRandomSpawnPos(mon);
                        if (mon_con.state.type != MonsterState.monType.Fly)
                            monAI.target = point.FinPoint[3];
                    }
                    break;
                case 4: // 4p 피니시라인
                    if (player_num == 1) // 1p 몬스터일때
                    {
                        mon.transform.position = point.SpawnPoint[1].position + NextRandomSpawnPos(mon);
                        if (mon_con.state.type != MonsterState.monType.Fly)
                            monAI.target = point.FinPoint[1];
                    }
                    else
                    {
                        mon.transform.position = point.SpawnPoint[0].position + NextRandomSpawnPos(mon);
                        if (mon_con.state.type != MonsterState.monType.Fly)
                            monAI.target = point.FinPoint[0];
                    }
                    break;
            }
        }
    }
    #endregion
    #region SyncVar
    #endregion
    #region Client
    [Client]
    private void Life_Set(int Player_Num, int Target_Num, bool isdead)
    {
        Debug.Log("플레이어 넘버 : " + Player_Num);
        Debug.Log("몬스터 넘버 : " + Target_Num);
        Debug.Log("죽었는지 살았는지 : " + isdead);
    }

    #endregion
    #region Command
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_UpdateLifeOnClients(int newLife)
    {
        // 클라이언트에서 라이프 UI 등을 업데이트합니다.
    }
    #endregion
    #region Hook Method
    #endregion
    private Vector3 NextRandomSpawnPos(GameObject mon )
{
    int randIndexX = Random.Range(-10, 11);
    int randIndexZ = Random.Range(-1, 2);
    Vector3 spawnPos = new Vector3(randIndexX,  mon.transform.position.y, randIndexZ);

        return spawnPos;
}

    private int Convert_num(Collider col)
    {
        if(col.CompareTag("1P"))
        {
            return 1;
        }
        else if (col.CompareTag("2P"))
        {
            return 2;
        }
        else if (col.CompareTag("3P"))
        {
            return 3;
        }
        else if (col.CompareTag("4P"))
        {
            return 4;
        }
        else
        {
            return -1;
        }
    }

   
    private GameManager FindPlayerGameManager(int playerNum)
    {
        // 이 메소드는 네트워크 상의 플레이어를 찾아 해당하는 GameManager 인스턴스를 반환합니다.
        // playerNum에 해당하는 플레이어를 찾고, 그 플레이어의 GameManager 인스턴스를 반환합니다.
        // 찾는 방법은 게임의 구현에 따라 다를 수 있습니다.
        return null; // 예시를 위한 더미 반환값
    }
}
