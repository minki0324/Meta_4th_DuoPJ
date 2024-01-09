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
            int col_num = Convert_num(gameObject.GetComponent<Collider>());
            Monster_Control mon_con = other.GetComponent<Monster_Control>();
            AIDestinationSetter monAI = mon.GetComponent<AIDestinationSetter>();
            int player_num = Convert_num(other);
            Life_Manager.instance.Life_Set(col_num, player_num);
            int lineNum = mon_con.currentLineNum;
            lineNum++;
            while (Life_Manager.instance.IsPlayerDead(lineNum) || lineNum == player_num)
            {
                lineNum++;
                if (lineNum > 4)
                {
                    lineNum = 1;
                }
                //������ isDie �Ұ��� ��ΰ����ͼ� ��� �׾����� break�ɰ� �¸����� ��������
                if (Life_Manager.instance.isVectoryCheck(player_num))
                {
                    Debug.Log(player_num + "�÷��̾ �¸��Ͽ����ϴ�!");
                    return;
                }
            }
            mon_con.currentLineNum = lineNum;
            mon.transform.position = point.SpawnPoint[lineNum - 1].position + NextRandomSpawnPos(mon);
            if (mon_con.state.type != MonsterState.monType.Fly && mon_con.state.type != MonsterState.monType.Attack)
            {
                monAI.target = point.FinPoint[lineNum - 1];
            }




            /*
                        mon.transform.position = point.SpawnPoint[point_].position + NextRandomSpawnPos(mon);
                        if (mon_con.state.type != MonsterState.monType.Fly || mon_con.state.type != MonsterState.monType.Attack)
                            monAI.target = point.FinPoint[point_];*/
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
    private Vector3 NextRandomSpawnPos(GameObject mon)
    {
        int randIndexX = Random.Range(-10, 11);
        int randIndexZ = Random.Range(-1, 2);
        Vector3 spawnPos = new Vector3(randIndexX, mon.transform.position.y, randIndexZ);

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

    private int Die_Check(int num, int goalcount)
    {
        // �� col_num�� �����ϴ� ��ȸ ����
        int[][] spawnOrder = new int[][]
        {
            new int[] { 1, 2, 3 }, // 1P�� ��ȸ ����
            new int[] { 2, 3, 0 }, // 2P�� ��ȸ ����
            new int[] { 3, 0, 1 }, // 3P�� ��ȸ ����
            new int[] { 0, 1, 2 }  // 4P�� ��ȸ ����
        };

        int[] order = spawnOrder[num]; // col_num�� �ش��ϴ� ��ȸ ���� �迭

        foreach (int point in order)
        {
            // ���� �÷��̾��� ���� ����Ʈ�� �ǳʶٱ�
            if (!IsPlayerDead(point))
            {
                return point;
            }
        }

        return -1;
    }

    private bool IsPlayerDead(int playerNum)
    {
        switch (playerNum)
        {
            case 1:
                return Life_Manager.instance.P1_isDead;
            case 2:
                return Life_Manager.instance.P2_isDead;
            case 3:
                return Life_Manager.instance.P3_isDead;
            case 4:
                return Life_Manager.instance.P4_isDead;
            default:
                return true; // �߸��� �÷��̾� ��ȣ
        }
    }
}
