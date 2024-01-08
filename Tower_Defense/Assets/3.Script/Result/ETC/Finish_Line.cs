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
                // ���߿� �÷��̾� ������ �ǳʶٴ� ���� �־�ߵ�
                case 1: // 1p �ǴϽö���
                    if (player_num == 2) // 2p �����϶�
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
                case 2: // 2p �ǴϽö���
                    if (player_num == 3) // 3p �����϶�
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
                case 3: // 3p �ǴϽö���
                    if (player_num == 4) // 4p �����϶�
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
                case 4: // 4p �ǴϽö���
                    if (player_num == 1) // 1p �����϶�
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
        Debug.Log("�÷��̾� �ѹ� : " + Player_Num);
        Debug.Log("���� �ѹ� : " + Target_Num);
        Debug.Log("�׾����� ��Ҵ��� : " + isdead);
    }

    #endregion
    #region Command
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_UpdateLifeOnClients(int newLife)
    {
        // Ŭ���̾�Ʈ���� ������ UI ���� ������Ʈ�մϴ�.
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
        // �� �޼ҵ�� ��Ʈ��ũ ���� �÷��̾ ã�� �ش��ϴ� GameManager �ν��Ͻ��� ��ȯ�մϴ�.
        // playerNum�� �ش��ϴ� �÷��̾ ã��, �� �÷��̾��� GameManager �ν��Ͻ��� ��ȯ�մϴ�.
        // ã�� ����� ������ ������ ���� �ٸ� �� �ֽ��ϴ�.
        return null; // ���ø� ���� ���� ��ȯ��
    }
}
