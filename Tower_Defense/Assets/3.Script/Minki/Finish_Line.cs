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

            switch (col_num)
            {
                // 나중에 플레이어 죽으면 건너뛰는 로직 넣어야됨
                case 1:
                    if (player_num == 2)
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
                case 2:
                    if (player_num == 3)
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
                case 3:
                    if (player_num == 4)
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
                case 4:
                    if (player_num == 1)
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
    #endregion
    #region Command
    #endregion
    #region ClientRPC
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
}
