using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UpgradePanel : MonoBehaviour
{
    public GameObject[] orderButtons;
    public GameObject heads;

    public Head_Data[] datas;

    private void Start()
    {
        orderButtons = new GameObject[transform.childCount];
        datas = new Head_Data[4];
        for (int i = 0; i < transform.childCount; i++)
        {
            orderButtons[i] = transform.GetChild(i).GetChild(2).gameObject;
        }
        int headindex = GameManager.instance.Tower_1_index[0];
        orderButtons[0].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[0] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        headindex = GameManager.instance.Tower_2_index[0];
        orderButtons[1].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[1] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        headindex = GameManager.instance.Tower_3_index[0];
        orderButtons[2].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[2] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        //스캔
        headindex = GameManager.instance.Tower_4_index[0];
        orderButtons[3].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[3] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        headindex = GameManager.instance.Tower_1_index[0];
        orderButtons[4].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[0] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        headindex = GameManager.instance.Tower_2_index[0];
        orderButtons[5].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[1] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        headindex = GameManager.instance.Tower_3_index[0];
        orderButtons[6].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[2] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        //라이프, 인구수

        //headindex = GameManager.instance.Tower_4_index[0];
        //orderButtons[3].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        //datas[3] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        headindex = GameManager.instance.Tower_1_index[0];
        orderButtons[8].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[0] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        headindex = GameManager.instance.Tower_2_index[0];
        orderButtons[9].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[1] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        headindex = GameManager.instance.Tower_3_index[0];
        orderButtons[10].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[2] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        //라이프, 인구수
        //headindex = GameManager.instance.Tower_4_index[0];
        //orderButtons[3].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        //datas[3] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;


    }
    public void buttonClick(int index)
    {
       
        if (!InfoConecttoUI.Instance.UpgradeCostCheck(100)) return; 
        switch (index)
        {
            case 0:
                UpgradeManager.Instance.onClickUp(0, 0);
                break;
            case 1:
                UpgradeManager.Instance.onClickUp(1, 0);
                break;
            case 2:
                UpgradeManager.Instance.onClickUp(2, 0);
                break;
            case 3:
                UpgradeManager.Instance.onClickUp(4, 1);
                break;
            case 4:
                UpgradeManager.Instance.onClickUp(0, 1);
                break;
            case 5:
                UpgradeManager.Instance.onClickUp(1, 1);
                break;
            case 6:
                UpgradeManager.Instance.onClickUp(2, 1);
                break;
            case 7:
                break;
            case 8:
                UpgradeManager.Instance.onClickUp(0, 2);
                break;
            case 9:
                UpgradeManager.Instance.onClickUp(1, 2);
                break;
            case 10:
                UpgradeManager.Instance.onClickUp(2, 2);
                break;
            case 11:
                break;

        }

    }
}
