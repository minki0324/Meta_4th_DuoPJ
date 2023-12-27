using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Effect_Pooling : NetworkBehaviour
{
    [Header("Vulcan")]
    public SyncList<GameObject> Vulcan_Projectile_List = new SyncList<GameObject>();    // 0
    public SyncList<GameObject> Vulcan_Muzzle_List = new SyncList<GameObject>();        // 1
    public SyncList<GameObject> Vulcan_Impact_List = new SyncList<GameObject>();        // 2

    [Header("Sniper")]
    public SyncList<GameObject> Sniper_Beam_List = new SyncList<GameObject>();          // 3
    public SyncList<GameObject> Sniper_Muzzle_List = new SyncList<GameObject>();        // 4
    public SyncList<GameObject> Sniper_Impact_List = new SyncList<GameObject>();        // 5

    [Header("Laser")]
    public SyncList<GameObject> Laser_List = new SyncList<GameObject>();                // 6

    [Header("Missle")]
    public SyncList<GameObject> Missle_List = new SyncList<GameObject>();               // 7
    public SyncList<GameObject> Missle_Flame_List = new SyncList<GameObject>();         // 8
    public SyncList<GameObject> Missle_Smoke_List = new SyncList<GameObject>();         // 9

    [Header("Seeker")]
    public SyncList<GameObject> Seeker_Projectile_List = new SyncList<GameObject>();    // 10
    public SyncList<GameObject> Seeker_Muzzle_List = new SyncList<GameObject>();        // 11
    public SyncList<GameObject> Seeker_Impact_List = new SyncList<GameObject>();        // 12

    [Header("Solo Gun(Air)")]
    public SyncList<GameObject> Air_Projectile_List = new SyncList<GameObject>();       // 13
    public SyncList<GameObject> Air_Muzzle_List = new SyncList<GameObject>();           // 14
    public SyncList<GameObject> Air_Gun_List = new SyncList<GameObject>();              // 15

    [Header("Flame")]
    public SyncList<GameObject> FlameRed_List = new SyncList<GameObject>();             // 16

    [Header("PlasmaBeam")]
    public SyncList<GameObject> PlasmaBeamHeavy_List = new SyncList<GameObject>();      // 17

    [Header("LaserImpulse")]
    public SyncList<GameObject> LaserImpulse_Projectile_List = new SyncList<GameObject>();// 18
    public SyncList<GameObject> LaserImpulse_Muzzle_List = new SyncList<GameObject>();  // 19
    public SyncList<GameObject> LaserImpulse_Impact_List = new SyncList<GameObject>();  // 20


    public GameObject[] EffectPrefab;
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
        for (int i = 0; i < Prefabs_Parents.Length; i++)
        {
            for (int j = 0; j < Prefabs_Parents[i].transform.childCount; j++)
            {
                Prefabs_Parents[i].transform.GetChild(j).gameObject.SetActive(false);
            }
        }
    }

    [ClientRpc]
    private void RpcSyncMonsterInfo(GameObject Effect, int parentIndex)
    {
        if (parentIndex >= 0 && parentIndex < Prefabs_Parents.Length)
        {
            // 클라이언트에서는 몬스터 정보를 받아와서 생성 및 부모 설정
            Effect.transform.SetParent(Prefabs_Parents[parentIndex].transform);
        }
        else
        {
            Debug.LogError("Invalid parent index in RpcSyncMonsterInfo");
        }
    }

    [ClientRpc]
    public void RPC_ActiveSet(bool isActive, GameObject monster)
    {
        if (isActive)
        {
            Debug.Log(monster.name);
            monster.SetActive(isActive);
        }
        else
        {
            monster.SetActive(!isActive);
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
        for (int i = 0; i < PoolSize; i++)
        {
            for (int j = 0; j < EffectPrefab.Length; j++)
            {
                GetEffect(j);
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
    public GameObject GetEffect(int index)
    {
        SyncList<GameObject> targetList = GetTargetList(index);

        foreach (var effect in targetList)
        {
            if (!effect.activeInHierarchy)
            {
                effect.SetActive(true);
                RPC_ActiveSet(true, effect);
                return effect;
            }
        }

        GameObject newEffect = Instantiate(EffectPrefab[index], Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(newEffect);
        RPC_ActiveSet(true, newEffect);
        targetList.Add(newEffect);

        int parentIndex = GetParentIndex(Prefabs_Parents[index]);
        newEffect.transform.SetParent(Prefabs_Parents[index].transform);
        RpcSyncMonsterInfo(newEffect, parentIndex);

        return newEffect;
    }

    [Server]
    private SyncList<GameObject> GetTargetList(int index)
    {
        switch (index)
        {
            case 0: return Vulcan_Projectile_List;
            case 1: return Vulcan_Muzzle_List;
            case 2: return Vulcan_Impact_List;
            case 3: return Sniper_Beam_List;
            case 4: return Sniper_Muzzle_List;
            case 5: return Sniper_Impact_List;
            case 6: return Laser_List;
            case 7: return Missle_List;
            case 8: return Missle_Flame_List;
            case 9: return Missle_Smoke_List;
            case 10: return Seeker_Projectile_List;
            case 11: return Seeker_Muzzle_List;
            case 12: return Seeker_Impact_List;
            case 13: return Air_Projectile_List;
            case 14: return Air_Muzzle_List;
            case 15: return Air_Gun_List;
            case 16: return FlameRed_List;
            case 17: return PlasmaBeamHeavy_List;
            case 18: return LaserImpulse_Projectile_List;
            case 19: return LaserImpulse_Muzzle_List;
            case 20: return LaserImpulse_Impact_List;
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
