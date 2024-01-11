using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class UpgradeManager : NetworkBehaviour
{
    public static UpgradeManager Instance;
    public string mytag;

    public int AttackLevel_Tower1;
    public int RangeLevel_Tower1;
    public int HPLevel_Tower1;

    public int AttackLevel_Tower2;
    public int RangeLevel_Tower2;
    public int HPLevel_Tower2;

    public int AttackLevel_Tower3;
    public int RangeLevel_Tower3;
    public int HPLevel_Tower3;

    public int AttackLevel_Tower4;
    public int RangeLevel_Tower4;
    public int HPLevel_Tower4;

    public int[,] TowerUpgradesArray = new int[4, 3];
  

    private void Awake()
    {
        if(Instance == null)
        {   
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mytag = ((int)GameManager.instance.Player_Num).ToString() + 'P';

        TowerUpgradesArray[0, 0] = AttackLevel_Tower1;
        TowerUpgradesArray[0, 1] = RangeLevel_Tower1;
        TowerUpgradesArray[0, 2] = HPLevel_Tower1;

        TowerUpgradesArray[1, 0] = AttackLevel_Tower2;
        TowerUpgradesArray[1, 1] = RangeLevel_Tower2;
        TowerUpgradesArray[1, 2] = HPLevel_Tower2;

        TowerUpgradesArray[2, 0] = AttackLevel_Tower3;
        TowerUpgradesArray[2, 1] = RangeLevel_Tower3;
        TowerUpgradesArray[2, 2] = HPLevel_Tower3;

        TowerUpgradesArray[3, 0] = AttackLevel_Tower4;
        TowerUpgradesArray[3, 1] = RangeLevel_Tower4;
        TowerUpgradesArray[3, 2] = HPLevel_Tower4;




    }
 
    public void UpdateLevel()
    {
        AttackLevel_Tower1 = TowerUpgradesArray[0, 0];
        RangeLevel_Tower1 = TowerUpgradesArray[0, 1];
        HPLevel_Tower1 = TowerUpgradesArray[0, 2];

        AttackLevel_Tower2 = TowerUpgradesArray[1, 0];
        RangeLevel_Tower2 = TowerUpgradesArray[1, 1];
        HPLevel_Tower2 = TowerUpgradesArray[1, 2];

        AttackLevel_Tower3 = TowerUpgradesArray[2, 0];
        RangeLevel_Tower3 = TowerUpgradesArray[2, 1];
        HPLevel_Tower3 = TowerUpgradesArray[2, 2];

        AttackLevel_Tower4 = TowerUpgradesArray[3, 0];
        RangeLevel_Tower4 = TowerUpgradesArray[3, 1];
        HPLevel_Tower4 = TowerUpgradesArray[3, 2];
    }
    public void onClickUp(int TowerNum, int upgradeTarget)
    {
        //if()
        CliUpgrade(TowerNum, upgradeTarget , mytag);
    }


    [Client]
    public void CliUpgrade(int TowerNum , int upgradeTarget, string mytag)
    {
        TowerUpgradesArray[TowerNum, upgradeTarget]++;
        UpdateLevel();
        CMD_Upgrade(TowerNum , upgradeTarget, mytag , TowerUpgradesArray[TowerNum, upgradeTarget]);
    }
    [Command(requiresAuthority = false)]
    private void CMD_Upgrade(int towerNum ,int upgradeTarget, string mytag ,int value)
    {
        
            Debug.Log("들어오 널");
        Debug.Log(mytag);
        foreach (Tower tower in BuildManager.Instance.AllTower)
        {
            if(tower.towerNum == towerNum && tower.gameObject.tag == mytag)
            {
                if (upgradeTarget == 0)
                {
                    Debug.Log("필드 타워 공격강화");
                    tower.AttackLevel = value;
                    tower.UpgradeUpdate();
                }
                else if (upgradeTarget == 1)
                {
                    Debug.Log("필드 타워 범위강화");
                    tower.RangeLevel = value;
                    tower.UpgradeUpdate();
                }
                else if (upgradeTarget == 2)
                {
                    Debug.Log("필드 타워 체력강화");
                    tower.HPLevel = value;
                    tower.UpgradeUpdate();
                }
            }
        }
    }
    //public void onClickAllTowerStateUpdate()
    //{
    //    foreach (Tower tower in BuildManager.Instance.AllTower)
    //    {
    //        tower.head.StateUpgradeUpdate();
    //    }
    //}
}
