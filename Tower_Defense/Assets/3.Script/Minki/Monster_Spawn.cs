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

    [SerializeField] private GameObject[] Mon_Prefabs;
    [SerializeField] private GameObject[] test_Build;

    #region Unity Callback
    #endregion
    #region SyncVar
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
        GameObject monster = Instantiate(Mon_Prefabs[index], spawnpoint.position, Quaternion.identity);
        NetworkServer.Spawn(monster);

        // �±� �Ҵ�
        monster.tag = $"{player_num}P";

        Monster_Movement move = monster.GetComponent<Monster_Movement>();
        Transform finpoint = Get_FinPoint(player_num);
        move.Astar.target = finpoint;
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
