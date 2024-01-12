using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
using FORGE3D;

public class Tower : NetworkBehaviour
{
    [SerializeField]
    public GameObject marker;
    public GameObject AttackRange;
    [SerializeField] private Material holoColor;
    [SerializeField] public Tower_Attack head;
    [SerializeField] public GameObject towerbase;
    [SerializeField] public Material Burnout;
    [SyncVar]
    public int towerNum;
    ////////////////////////////////타워 업그레이드 계수

    public ParticleSystem[] Heat;

    private bool heatShow;
    [SerializeField]private MeshRenderer[] _turretParts;
    private int _burnoutId;

    private float burnout;
    private float heatBias = 0f;
    float timer;
    bool BurnStart;



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
    [SyncVar]
    public bool isDestroy;
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
        _burnoutId = Shader.PropertyToID("_Burnout");
        Debug.Log(_burnoutId);
        for (var i = 0; i < Heat.Length; i++)
        {
            Heat[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        //너무많이담고있음
        _turretParts = new MeshRenderer[3]{ head.GetComponent<MeshRenderer>(), head.transform.parent.GetComponent<MeshRenderer>() , towerbase.GetComponent<MeshRenderer>()};
    }
    private void Update()
    {
        if (isActive &&isServer)
        {
            DestroyCheck();
          
        }

        if (isActive && isDestroy)
        {

            if (!BurnStart)
            {
                BurnStart = true;
                BuildManager.Instance.resourse.current_food -= 1;
                StartCoroutine(DestroyMotion());
            }
        }
    }

    public IEnumerator DestroyMotion() {

        timer += Time.deltaTime;
        burnout = Mathf.Lerp(0, 1f, timer / 2f);

        MeterialBurnout(gameObject);

        if (burnout <= 1)
        {
            for (var i = 0; i < Heat.Length; i++)
                Heat[i].Play(true);
        }


        yield return new WaitForSeconds(2f);


        for (var i = 0; i < Heat.Length; i++)
        {
            Heat[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        Debug.Log("들어오니");
        for (var i = 0; i < _turretParts.Length; i++)
            _turretParts[i].material.SetFloat(_burnoutId, 0);

        yield return new WaitForSeconds(0.2f);
        Debug.Log("죽음");
        if (isServer)
        {
            BuildManager.Instance.CMD_DestroyTower(gameObject);
        }





    }
    public void MeterialBurnout(GameObject tower)
    {
        MeshRenderer renderer = AttackRange.GetComponent<MeshRenderer>();
        Material temp = renderer.material;
        MeshRenderer meshRenderer = tower.GetComponent<MeshRenderer>();
        if (meshRenderer)
        {
            meshRenderer.materials = new Material[0];
            meshRenderer.material = Burnout;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        foreach (Transform child in tower.transform)
        {
            MeterialBurnout(child.gameObject);
        }
        renderer.material = temp;
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
        //현재 올린 업그레이드 횟수
        level = AttackLevel + RangeLevel + HPLevel;

        //hp업글계수에 따라 업데이트해주고 현재 체력그대로 올려줘야함
        float tempHp = maxHP - currentHP;
        maxHP = towerbase.GetComponent<BaseData>().baseData.Health + HPLevel * 100;
        currentHP = maxHP - tempHp;

        //업글로 인해 변화된 스탯 표기를 위한 변수들 up.
        upDamage = F3Round(head.head_Data.Damage * 0.1f * AttackLevel);
        upRange = 2 * RangeLevel;

        //업글계수로 인해 변화된 실제 데미지,범위,공속 값들
        RealDamage = damage + upDamage;
        RealRange = range + upRange;
        RealAS = F3Round(atkSpeed / (1 + (0.05f * AttackLevel)));
        upAS = atkSpeed - RealAS;
       
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
                //BuildManager.Instance.CMD_DestroyTower(gameObject);
            }
        }
    }
    [ClientRpc]
    private void SetSprite()
    {
        unitSprite = head.head_Data.towerImage;
    }
}
