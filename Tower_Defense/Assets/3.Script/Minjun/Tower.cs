using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class Tower : NetworkBehaviour
{
    [SerializeField]
    public GameObject marker;
    public GameObject AttackRange;
    [SerializeField] private Material holoColor;
    [SerializeField] public Tower_Attack head;
    [SerializeField] public GameObject towerbase;

    [SyncVar]
    public float maxHP;
    [SyncVar]
    public float currentHP;
    [SyncVar]
    public float damage;
    [SyncVar]
    public float range;
    [SyncVar]
    public float atkSpeed;
    [SyncVar]
    public string Speed;
    [SyncVar]
    public int level;
    [SyncVar]
    public bool isActive;
    
    public Sprite unitSprite;
    private bool isDestroy;

    private void Awake()
    {


    }
    private void Start()
    {
        if (isServer)
        {
            TowerIninit();
            SetSprite();
        }
    }
    private void Update()
    {
        if (isActive &&isServer)
        {
            DestroyCheck();
        }
    }



    private void TowerIninit()
    {
        maxHP = towerbase.GetComponent<BaseData>().baseData.Health;
        currentHP = maxHP;
        damage = head.head_Data.Damage;
        range = head.head_Data.ATK_Range;
        atkSpeed = head.head_Data.ATK_Speed;
        Speed = "-";
        level = 1;
        unitSprite = head.head_Data.towerImage;
    }
  
    public void Selectunit()
    {
        marker.SetActive(true);

    }
    public void SetRange()
    {
        AttackRange.SetActive(true);
        AttackRange.transform.localScale = new Vector3(head.H_ATK_Range * 2f, 0.01f, head.H_ATK_Range * 2f);
    }

    public void DeSelectunit()
    {
        marker.SetActive(false);
        AttackRange.SetActive(false);
    }
    public void HologramTower(GameObject gameObject)
    {
        MeshRenderer renderer = AttackRange.GetComponent<MeshRenderer>();
        Material temp = renderer.material;
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer )
        {
            meshRenderer.materials = new Material[0];
            meshRenderer.material = holoColor;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        foreach (Transform child in gameObject.transform)
        {
            HologramTower(child.gameObject);
        }
        renderer.material = temp;
    }
    private void DestroyCheck()
    {
        Debug.Log(currentHP);
        if (currentHP <= 0)
        {
            if (!isDestroy)
            {
                isDestroy = true;
                BuildManager.Instance.CMD_DestroyTower(gameObject);
            }
        }
    }
    //[Server]
    //private void DestroyTower()
    //{
    //    RPC_DestroyTower(gameObject);
    //    RTSControlSystem.Instance.Destroytower(this);
    //    Destroy(gameObject);
    //}
    //[ClientRpc]
    //private void RPC_DestroyTower(GameObject tower)
    //{
    //    if (RTSControlSystem.Instance.selectTowers.Contains(tower.GetComponent<Tower>()))
    //    {
    //        RTSControlSystem.Instance.selectTowers.Remove(tower.GetComponent<Tower>());
    //        //UI 세팅 다시해줘야함
    //    }
    //    Destroy(tower);
    //}
    [ClientRpc]
    private void SetSprite()
    {
        unitSprite = head.head_Data.towerImage;
    }
}
