using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room_FinalData : MonoBehaviour
{
    [SerializeField] private Room_SelectPart head_script;
    [SerializeField] private Room_SelectPart mount_script;
    [SerializeField] private Room_SelectPart base_script;
    [SerializeField] private Room_Prefabs prefabs_script;

    [SerializeField] private Text name_;
    [SerializeField] private Text data_1;
    [SerializeField] private Text data_2;

    [SerializeField] private Tower_Index index;

    [SerializeField] private GameObject[] Empty_Btn;
    [SerializeField] private GameObject[] Ready_Btn;
    [SerializeField] private RenderTexture[] Prefabs_Image;
    [SerializeField] private RawImage image;

    public int Cost;
    public float Head_Rot_Speed;
    public float Mount_Rot_Speed;
    public float Damage;
    public float ATK_Speed;
    public float ATK_Range;
    public int Reload;
    public int Health;
    public string BuildArea;

    public int[] Prefab_1_index;
    public int[] Prefab_2_index;
    public int[] Prefab_3_index;

    public bool Slot_1 = false;
    public bool Slot_2 = false;
    public bool Slot_3 = false;

    private void Awake()
    {
        Set_Data();
        Print_Data();
        Prefab_1_index = new int[] { 1, 1, 1 };
        Prefab_2_index = new int[] { 2, 2, 2 };
        Prefab_3_index = new int[] { 3, 3, 3 };
    }

    public void Set_Data()
    {
        Cost = head_script.head[head_script.current_index].Cost + mount_script.mount[mount_script.current_index].Cost + base_script.base_[base_script.current_index].Cost;
        Head_Rot_Speed = head_script.head[head_script.current_index].Rot_Speed;
        Mount_Rot_Speed = mount_script.mount[mount_script.current_index].Rot_Speed;
        Damage = head_script.head[head_script.current_index].Damage;
        ATK_Speed = head_script.head[head_script.current_index].ATK_Speed;
        ATK_Range = head_script.head[head_script.current_index].ATK_Range;
        Reload = head_script.head[head_script.current_index].Reload;
        Health = base_script.base_[base_script.current_index].Health;
        BuildArea = base_script.base_[base_script.current_index].BuildArea;
    }

    public void Print_Data()
    {
        data_1.text = $"{Cost}\n{Damage}\n{ATK_Range}\n{ATK_Speed}\n{Health}";
        data_2.text = $"{Mount_Rot_Speed}\n{Head_Rot_Speed}\n{Reload}\n{BuildArea}";
    }

    public void onSave()
    {
        if (index.tower_index == 0)
        {
            Prefab_1_index = index.Return_index();
            Slot_1 = true;
            Empty_Btn[0].SetActive(false);
            Ready_Btn[0].SetActive(true);
        }
        else if(index.tower_index == 1)
        {
            Prefab_2_index = index.Return_index();
            Slot_2 = true;
            Empty_Btn[1].SetActive(false);
            Ready_Btn[1].SetActive(true);
        }
        else
        {
            Prefab_3_index = index.Return_index();
            Slot_3 = true;
            Empty_Btn[2].SetActive(false);
            Ready_Btn[2].SetActive(true);
        }
    }

    public void Change_Prefabs(int index)
    {
        this.index.tower_index = index;
        if(index == 0)
        {
            head_script.current_index = Prefab_1_index[0];
            mount_script.current_index = Prefab_1_index[1];
            base_script.current_index = Prefab_1_index[2];
            image.texture = Prefabs_Image[0];
        }
        else if(index == 1)
        {
            head_script.current_index = Prefab_2_index[0];
            mount_script.current_index = Prefab_2_index[1];
            base_script.current_index = Prefab_2_index[2];
            image.texture = Prefabs_Image[1];
        }
        else
        {
            head_script.current_index = Prefab_3_index[0];
            mount_script.current_index = Prefab_3_index[1];
            base_script.current_index = Prefab_3_index[2];
            image.texture = Prefabs_Image[2];
        }
        Set_Data();
        Print_Data();
        this.index.Get_Index();
        this.index.Print_Tower();
        head_script.Print_Parts(head_script.current_index);
        head_script.Set_data();
        mount_script.Print_Parts(mount_script.current_index);
        mount_script.Set_data();
        base_script.Print_Parts(base_script.current_index);
        base_script.Set_data();

    }
}
