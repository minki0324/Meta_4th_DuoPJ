using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class Room_SelectPart : MonoBehaviour
{
    public enum Type
    {
        Head,
        Mount,
        Base
    }

    public Type type;

    [SerializeField] public Head_Data[] head;
    [SerializeField] public Mount_Data[] mount;
    [SerializeField] public Base_Data[] base_;
    [SerializeField] private RenderTexture[] images;
    [SerializeField] private RawImage image;

    [SerializeField] private VideoClip[] preview_Clip;
    [SerializeField] private VideoPlayer Player;

    [SerializeField] private Text name_;
    [SerializeField] private Text info_1;
    [SerializeField] private Text info_2;
    [SerializeField] private Text ATK_type;
    [SerializeField] private Text ATK_Area;

    [SerializeField] private Room_FinalData final;

    public int current_index = 0;

    private void Awake()
    {
        Set_data();
    }

    public void onClick_Front()
    {
        // 배열의 다음 텍스처로 이동
        current_index = (current_index + 1) % images.Length;

        // RawImage에 새로운 텍스처 할당
        Print_Parts(current_index);
        Set_data();
        final.Set_Data();
        final.Print_Data();

    }

    public void onClick_Back()
    {
        // 배열의 이전 텍스처로 이동
        current_index = (current_index - 1 + images.Length) % images.Length;

        // RawImage에 새로운 텍스처 할당
        Print_Parts(current_index);
        Set_data();
        final.Set_Data();
        final.Print_Data();
    }

    public void Set_data()
    {
        if(type == Type.Head)
        {
            name_.text = $"{head[current_index].name_}";
            info_1.text = $"{head[current_index].Cost}\n{head[current_index].Damage}\n{head[current_index].ATK_Range}\n{head[current_index].Rot_Speed}";
            info_2.text = $"{head[current_index].ATK_Speed}\n{head[current_index].Reload}";
            if(head[current_index].weapon_Type == Head_Data.Weapon_Type.Targeting)
            {
                ATK_type.text = "Target";
            }
            else
            {
                ATK_type.text = "Splash";
            }

            if(head[current_index].atk_Area ==  Head_Data.Atk_Area.Ground)
            {
                ATK_Area.text = "Ground";
            }
            else if(head[current_index].atk_Area == Head_Data.Atk_Area.Air)
            {
                ATK_Area.text = "Air Only";
            }
            else
            {
                ATK_Area.text = "All";
            }
        }
        else if(type == Type.Mount)
        {
            name_.text = $"{mount[current_index].name_}";
            info_1.text = $"{mount[current_index].Cost}\n{mount[current_index].Rot_Speed}";
        }
        else if(type == Type.Base)
        {
            name_.text = $"{base_[current_index].name_}";
            info_1.text = $"{base_[current_index].Cost}\n{base_[current_index].Health}\n{base_[current_index].BuildArea}";
        }
    }

    public void Print_Parts(int index)
    {
        image.texture = images[index];
        if(type == Type.Head)
        {
            Player.clip = preview_Clip[index];
        }
    }
}
