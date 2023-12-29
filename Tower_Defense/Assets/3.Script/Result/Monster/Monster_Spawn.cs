using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Pathfinding;

public class Monster_Spawn : NetworkBehaviour
{
    /*
        ���� ���� �� ������Ʈ Ǯ�� ��ũ��Ʈ
        1. �÷��̾� �����տ� �ִ� ���� ����Ʈ�� ���� �������� ���� ����
        2. ����Ʈ�� ��Ƴ��� ��Ȱ��ȭ �Ǿ��ִ� ���Ͱ� ������ �ش� ���͸� Ȱ��ȭ�ϸ鼭 ����
    */

    public Transform[] SpawnPoint;
    public Transform[] FinPoint;

    [SerializeField] private Monster_Pooling pools;
    [SerializeField] private GameObject[] Mon_Prefabs;

    #region SyncVar
    
    public SyncList<Monster_Control> AllMonster = new SyncList<Monster_Control>();

    #endregion
    #region Unity Callback
    #endregion
    #region Client
    [Client]
    public void Onclick(int index)
    {
        CMD_SpawnMonster(index, (int)GameManager.instance.Player_Num);
    }
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    private void CMD_SpawnMonster(int index, int player_num)
    {
        // �ʱ�ȭ �޼ҵ� �����;ߵ�
        Transform spawnpoint = Get_SpawnPoint(player_num);
        int randIndexX = Random.Range(-10, 11);
        int randIndexZ = Random.Range(-1, 2);
        Vector3 spawnPos = new Vector3(spawnpoint.position.x + randIndexX, spawnpoint.position.y+ Mon_Prefabs[index].transform.position.y, spawnpoint.position.z + randIndexZ);
        GameObject monster = pools.GetMonster(index);
        monster.transform.SetPositionAndRotation(spawnPos, Mon_Prefabs[index].transform.rotation);
        //NetworkServer.Spawn(monster);
        // �±� �Ҵ�
        monster.tag = $"{player_num}P";

        Monster_Control monster_con = monster.GetComponent<Monster_Control>();
        AllMonster.Add(monster_con);
        Transform finpoint = Get_FinPoint(player_num);
        //�ö��̴� ���̽�Ÿ�Ⱦ�
        if (monster_con.state.type != MonsterState.monType.Fly)
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
            // �±� �Ҵ�
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
                ���߿� �÷��̾� ��� ���ϸ� ���� �÷��̾�� ��ȯ�ϴ� ���� �ʿ��� Todo 
            */
            case 1:
                return SpawnPoint[1];
            case 2:
                return SpawnPoint[2];
            case 3:
                return SpawnPoint[3];
            case 4:
                return SpawnPoint[0];
            default:
                return null;
        }
    }
    private Transform Get_FinPoint(int player_num)
    {
        switch (player_num)
        {
            /*
                ���߿� �ǴϽö��� �Ѿ�� �� �� ���� �÷��̾�� ���� ���� �ʿ��� Todo
            */
            case 1:
                return FinPoint[1];
            case 2:
                return FinPoint[2];
            case 3:
                return FinPoint[3];
            case 4:
                return FinPoint[0];
            default:
                return null;
        }
    }
}
