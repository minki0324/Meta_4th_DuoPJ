using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Monster_Pooling : NetworkBehaviour
{
    public SyncList<GameObject> Fly_List = new SyncList<GameObject>();
    public SyncList<GameObject> Fast_List = new SyncList<GameObject>();
    public SyncList<GameObject> Attack_List = new SyncList<GameObject>();
    public SyncList<GameObject> Slow_List = new SyncList<GameObject>();
    public SyncList<GameObject> Basic_List = new SyncList<GameObject>();
    public SyncList<GameObject> Invi_List = new SyncList<GameObject>();
    public GameObject[] MonsterPrefab;
    public GameObject[] Prefabs_Parents;
    public int PoolSize;
    #region Unity Callback
    private void Start()
    {
        if (isServer)
        {
            StartCoroutine(DelayedInit());
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
    [ClientRpc]
    private void RPC_DeactivateAllChildren()
    {
        for(int i  = 0; i < Prefabs_Parents.Length; i++)
        {
            for(int j = 0; j < Prefabs_Parents[i].transform.childCount; j++)
            {
                Prefabs_Parents[i].transform.GetChild(j).gameObject.SetActive(false);
            }
        }
    }

    [ClientRpc]
    private void RpcSyncMonsterInfo(GameObject monster, int parentIndex)
    {
        if (parentIndex >= 0 && parentIndex < Prefabs_Parents.Length)
        {
            // 클라이언트에서는 몬스터 정보를 받아와서 생성 및 부모 설정
            monster.transform.SetParent(Prefabs_Parents[parentIndex].transform);
        }
        else
        {
            Debug.LogError("Invalid parent index in RpcSyncMonsterInfo");
        }
    }

    #endregion
    #region Hook Method
    #endregion
    #region Server
    [Server]
    private IEnumerator DelayedInit()
    {
        // 서버 시작 후 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 풀 초기화
        Init_Pool();
    }

    [Server]
    private void Init_Pool()
    {
        for(int i = 0; i < PoolSize; i++)
        {
            for(int j = 0; j < MonsterPrefab.Length; j++)
            {
                GetMonster(j);
            }
        }

        for (int i = 0; i < Prefabs_Parents.Length; i++)
        {
            DeactivateAllChildren(Prefabs_Parents[i]);
        }
        RPC_DeactivateAllChildren();
    }

    [Server]
    private void DeactivateAllChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    [Server]
    public GameObject GetMonster(int index)
    {
        SyncList<GameObject> targetList = GetTargetList(index);

        foreach (var monster in targetList)
        {
            if (!monster.activeInHierarchy)
            {
                monster.SetActive(true);
                GameManager.instance.RPC_ActiveSet(true, monster);
                return monster;
            }
        }

        int randomSpawnX = Random.Range(-110, -90);
        int randomSpawnZ = Random.Range(-110, -90);

        Vector3 newSpawnPoint = new Vector3(randomSpawnX, 100.5f, randomSpawnZ);
        GameObject newMonster = Instantiate(MonsterPrefab[index], newSpawnPoint, MonsterPrefab[index].transform.rotation);
        NetworkServer.Spawn(newMonster);
        targetList.Add(newMonster);

        int parentIndex = GetParentIndex(Prefabs_Parents[index]);
        newMonster.transform.SetParent(Prefabs_Parents[index].transform);
        RpcSyncMonsterInfo(newMonster, parentIndex);

        return newMonster;
    }

    [Server]
    private SyncList<GameObject> GetTargetList(int index)
    {
        switch (index)
        {
            case 0: return Fly_List;
            case 1: return Fast_List;
            case 2: return Attack_List;
            case 3: return Slow_List;
            case 4: return Basic_List;
            case 5: return Invi_List;
            default: return null;
        }
    }

    [Server]
    private int GetParentIndex(GameObject parent)
    {
        for (int i = 0; i < Prefabs_Parents.Length; i++)
        {
            if (Prefabs_Parents[i] == parent)
            {
                return i;
            }
        }
        return -1; // Invalid index
    }
    #endregion
}
