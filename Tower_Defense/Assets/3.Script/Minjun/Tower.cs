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
    [SerializeField] public Material holo;
    [SyncVar]
    public int towerNum;
    ////////////////////////////////타워 업그레이드 계수
 

    [SyncVar]
    public float maxHP ;
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
    public int level =0;
    [SyncVar]
    public bool isActive;
    [SyncVar]
    public string towerName;
    [SyncVar]
    public string towerType;
    public Sprite unitSprite;
    private bool isDestroy;
    [SyncVar]
    public int AttackLevel;
    [SyncVar]
    public int RangeLevel;
    [SyncVar]
    public int HPLevel;
    [SyncVar]
    public float upDamage =0;
    [SyncVar]
    public float upAS = 0;
    [SyncVar]
    public float upRange = 0;

    [SyncVar]
    public float RealDamage;
    [SyncVar]
    public float RealRange;
    [SyncVar]
    public float RealAS;

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



    public void TowerIninit()
    {

        maxHP = towerbase.GetComponent<BaseData>().baseData.Health + (HPLevel * 100);
        currentHP = maxHP;
        damage = head.head_Data.Damage ;
        range = head.head_Data.ATK_Range;
        atkSpeed = head.head_Data.ATK_Speed;
        Speed = "-";
        unitSprite = head.head_Data.towerImage;
        towerName = head.head_Data.name_;
        towerType = head.head_Data.weapon_Type.ToString();
        UpgradeUpdate();
        //
        //
    }
    public void HpUpgrade()
    {

    }
    public void UpgradeUpdate()
    {

        //AttackLevel = upgradeArray[towerNum, 0];
        //RangeLevel = upgradeArray[towerNum, 1];
        //HPLevel = upgradeArray[towerNum, 2];

        level = AttackLevel + RangeLevel + HPLevel;
        float tempHp = maxHP - currentHP;
        maxHP = towerbase.GetComponent<BaseData>().baseData.Health + HPLevel * 100;
        currentHP = maxHP - tempHp;

        upDamage = F3Round(head.head_Data.Damage * 0.1f * AttackLevel);
        upRange = 2 * RangeLevel;

        RealDamage = damage + upDamage;
        RealRange = range + upRange;
        Debug.Log("기본공속 : " + atkSpeed);
        RealAS = F3Round(atkSpeed / (1 + (0.05f * AttackLevel)));
        upAS = atkSpeed - RealAS;
        Debug.Log($"실제공속 : {RealAS} , 감소된공속 : {upAS}");
       
    }

    private float F3Round(float temp)
    {
        
        return Mathf.Round(temp * 1000) / 1000f;
    }

    public void Selectunit()
    {
        if (!GameManager.instance.CompareEnumWithTag(tag))
        {
            marker.GetComponent<SpriteRenderer>().color = new Color(1f, 0.2039f, 0.2235f);
        }
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
        if (currentHP <= 0)
        {
            if (!isDestroy)
            {
                isDestroy = true;
                BuildManager.Instance.CMD_DestroyTower(gameObject);
            }
        }
    }
    [ClientRpc]
    private void SetSprite()
    {
        unitSprite = head.head_Data.towerImage;
    }
}
