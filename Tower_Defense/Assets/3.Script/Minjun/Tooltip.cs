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
    [SerializeField] private Text name_txt;
    [SerializeField] private Text Cost_txt;
    [SerializeField] private Text Des_txt;
    [SerializeField] private Text Data_txt;
    [SerializeField] private Text Q_txt;


    [Header("몬스터")]
    [SerializeField] private MonsterState[] monster_data;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject info = eventData.pointerCurrentRaycast.gameObject;
        if (info.GetComponent<Tooltip_Data>() != null)
        {
            Tooltip_panel.SetActive(true);
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
            name_txt.text = tower_data.datas[index].name_;
            Cost_txt.text = GameManager.instance.Cost[index].ToString();
            Des_txt.text = tower_data.datas[index].descirption;
            Data_txt.text = $"{GameManager.instance.Health[index].ToString()}\n" +
                $"{GameManager.instance.Damage[index].ToString()}\n" +
                $"{GameManager.instance.Range[index].ToString()}\n" +
                $"{GameManager.instance.ATK_Speed[index].ToString()}s\n" +
                $"{GameManager.instance.Head_Rot_speed[index].ToString()}\n" +
                $"{tower_data.datas[index].weapon_Type}\n" +
                $"{tower_data.datas[index].atk_Area}";
        }
        else
        {
            name_txt.text = tower_data.datas[index].name_;
            Cost_txt.text = "100";
            Des_txt.text = tower_data.datas[index].descirption;
            Data_txt.text = $"{tower_data.ScanTower_health.Health}\n" +
                $"-\n" +
                $"{tower_data.datas[index].ATK_Range}\n" +
                $"-\n" +
                $"-\n" +
                $"-\n" +
                $"{tower_data.datas[index].atk_Area}";
        }
    }

    private void Print_Spawner(GameObject info)
    {

    }

    private void Print_Upgrade(GameObject info)
    {

    }
}
