using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] private GameObject Tooltip_panel;
    [SerializeField] private GameObject Builder_tooltip;
    [SerializeField] private GameObject Spawner_tooltip;
    [SerializeField] private GameObject Upgrade_tooltip;

    [Header("타워")]
    [SerializeField] private BuilderPanel tower_data;
    [SerializeField] private Text T_name_txt;
    [SerializeField] private Text T_Cost_txt;
    [SerializeField] private Text T_Des_txt;
    [SerializeField] private Text T_Data_txt;
    [SerializeField] private Text T_Quick_txt;


    [Header("몬스터")]
    [SerializeField] private MonsterState[] monster_data;
    [SerializeField] private Text M_name_txt;
    [SerializeField] private Text M_Cost_txt;
    [SerializeField] private Text M_Des_txt;
    [SerializeField] private Text M_Data_txt;
    [SerializeField] private Text M_Quick_txt;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject info = eventData.pointerCurrentRaycast.gameObject;
        if (info.GetComponent<Tooltip_Data>() != null)
        {
            Tooltip_panel.SetActive(true);
            Debug.Log(InfoConecttoUI.Instance.type);
            switch (InfoConecttoUI.Instance.type)
            {
                case InfoConecttoUI.Type.Builder:
                    Builder_tooltip.SetActive(true);
                    Print_Builder(info);
                    break;
                case InfoConecttoUI.Type.Spawner:
                    Spawner_tooltip.SetActive(true);
                    Print_Spawner(info);
                    break;
                case InfoConecttoUI.Type.Upgrade:
                    Upgrade_tooltip.SetActive(true);
                    Print_Upgrade(info);
                    break;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Builder_tooltip.SetActive(false);
        Spawner_tooltip.SetActive(false);
        Upgrade_tooltip.SetActive(false);
        Tooltip_panel.SetActive(false);
    }

    private void Print_Builder(GameObject info)
    {
        int index = info.GetComponent<Tooltip_Data>().index;

        if (index < 3)
        {
            T_name_txt.text = tower_data.datas[index].name_;
            T_Cost_txt.text = GameManager.instance.Cost[index].ToString();
            T_Des_txt.text = tower_data.datas[index].descirption;
            T_Data_txt.text = $"{GameManager.instance.Health[index].ToString()}\n" +
                $"{GameManager.instance.Damage[index].ToString()}\n" +
                $"{GameManager.instance.Range[index].ToString()}\n" +
                $"{GameManager.instance.ATK_Speed[index].ToString()}s\n" +
                $"{GameManager.instance.Head_Rot_speed[index].ToString()}\n" +
                $"{tower_data.datas[index].weapon_Type}\n" +
                $"{tower_data.datas[index].atk_Area}";
        }
        else
        {
            T_name_txt.text = tower_data.datas[index].name_;
            T_Cost_txt.text = "100";
            T_Des_txt.text = tower_data.datas[index].descirption;
            T_Data_txt.text = $"{tower_data.ScanTower_health.Health}\n" +
                $"-\n" +
                $"{tower_data.datas[index].ATK_Range}\n" +
                $"-\n" +
                $"-\n" +
                $"-\n" +
                $"{tower_data.datas[index].atk_Area}";
        }

        switch(index)
        {
            case 0:
                T_Quick_txt.text = "Q";
                break;
            case 1:
                T_Quick_txt.text = "W";
                break;
            case 2:
                T_Quick_txt.text = "E";
                break;
            case 3:
                T_Quick_txt.text = "R";
                break;
        }
    }

    private void Print_Spawner(GameObject info)
    {
        int index = info.GetComponent<Tooltip_Data>().index;
        M_name_txt.text = monster_data[index].monsterName;
        M_Cost_txt.text = monster_data[index].cost.ToString();
        M_Des_txt.text = monster_data[index].Description;
        M_Data_txt.text = $"{monster_data[index].maxHp}\n" +
            $"{monster_data[index].damage}\n" +
            $"{monster_data[index].attackSpeed}\n" +
            $"{monster_data[index].speed}\n" +
            $"{monster_data[index].monsterType}\n" +
            $"{monster_data[index].income}";

        Debug.Log(index);
        switch(index)
        {
            case 0:
                M_Quick_txt.text = "Q";
                break;
            case 1:
                M_Quick_txt.text = "W";
                break;
            case 2:
                M_Quick_txt.text = "E";
                break;
            case 3:
                M_Quick_txt.text = "R";
                break;
            case 4:
                M_Quick_txt.text = "A";
                break;
            case 5:
                M_Quick_txt.text = "S";
                break;
        }
    }

    private void Print_Upgrade(GameObject info)
    {

    }
}
