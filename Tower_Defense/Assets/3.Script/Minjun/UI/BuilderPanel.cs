using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuilderPanel : MonoBehaviour
{
    public GameObject[] orderButtons;
    public GameObject heads;
    public GameObject ScanTower;
    public Sprite NoneImage;
    private void Start()
    {
        orderButtons = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            orderButtons[i] = transform.GetChild(i).GetChild(2).gameObject;
        }
        int headindex = GameManager.instance.Tower_1_index[0];
        orderButtons[0].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        headindex = GameManager.instance.Tower_2_index[0];
        orderButtons[1].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        headindex = GameManager.instance.Tower_3_index[0];
        orderButtons[2].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;
        headindex = GameManager.instance.Tower_4_index[0];
        orderButtons[3].GetComponent<Image>().sprite = heads.transform.GetChild(headindex).GetComponent<Tower_Attack>().head_Data.towerImage;

        for (int i = 4; i < orderButtons.Length; i++)
        {
            orderButtons[i].GetComponent<Image>().sprite = NoneImage;
        }
    }
}
