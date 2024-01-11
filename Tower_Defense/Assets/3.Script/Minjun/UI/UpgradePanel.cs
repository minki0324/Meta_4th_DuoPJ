using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    public GameObject[] orderButtons;
    public GameObject heads;
    public Base_Data ScanTower_health;
    public Sprite NoneImage;

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
        headindex = GameManager.instance.Tower_4_index[0];
        orderButtons[3].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        datas[3] = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data;

        for (int i = 4; i < orderButtons.Length; i++)
        {
            if (i != 6) orderButtons[i].GetComponent<Image>().sprite = NoneImage;
        }

    }
}
