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

    [SerializeField] private Transform[] SpawnPoint;

    [SerializeField] private GameObject[] Mon_Prefabs;
    [SerializeField] private Transform[] FinPoint;
    [SerializeField] private GameObject[] test_Build;

    #region Unity Callback
    private void Start()
    {
        
    }
    #endregion
    #region SyncVar
    #endregion
    #region Client
    [Client]
    public void Onclick(int index)
    {
        CMD_SpawnMonster(index, (int)GameManager.instance.Player_Num);
    }

    [Client]
    public void testClick()
    {
        test_Build[0].SetActive(true);
        test_Build[1].SetActive(true);
        CMD_TestBuild();
    }
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    private void CMD_SpawnMonster(int index, int player_num)
    {
        GameObject monster = Instantiate(Mon_Prefabs[index], SpawnPoint[0].position, Quaternion.identity);
        NetworkServer.Spawn(monster);

        // �±� �Ҵ�
        monster.tag = $"{player_num}P";

        Monster_Movement move = monster.GetComponent<Monster_Movement>();
        move.Astar.target = FinPoint[0];
        Rpc_SpawnMonster(monster, player_num);
    }

    [Command(requiresAuthority = false)]
    private void CMD_TestBuild()
    {
        test_Build[0].SetActive(true);
        test_Build[1].SetActive(true);
        AstarPath.active.Scan();
        Test_MonSetup();
        RPC_TestBuild();
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

    [ClientRpc]
    private void RPC_TestBuild()
    {
        if(isLocalPlayer)
        {
            Test_MonSetup();
            test_Build[0].SetActive(true);
            test_Build[1].SetActive(true);
        }
    }
    #endregion
    #region Hook Method
    #endregion
    private void Test_MonSetup()
    {
        AIDestinationSetter[] monsters = FindObjectsOfType<AIDestinationSetter>();
        foreach (var monster in monsters)
        {
            monster.target = FinPoint[0];
        }
    }
}
