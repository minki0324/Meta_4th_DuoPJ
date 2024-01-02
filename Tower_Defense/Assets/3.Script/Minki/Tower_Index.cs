using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Index : MonoBehaviour
{
    [SerializeField] private Room_SelectPart[] Indexs;
    [SerializeField] private Room_Prefabs[] Final_Print;

    private int head_index = 0;
    private int mount_index = 0;
    private int base_index = 0;

    public int tower_index = 0;

    public void Get_Index()
    {
        for(int i = 0; i < Indexs.Length; i++)
        {
            if(Indexs[i].type == Room_SelectPart.Type.Head)
            {
                head_index = Indexs[i].current_index;
            }
            else if(Indexs[i].type == Room_SelectPart.Type.Mount)
            {
                mount_index = Indexs[i].current_index;
            }
            else
            {
                base_index = Indexs[i].current_index;
            }
        }
    }

    public void Print_Tower()
    {
        switch(tower_index)
        {
            case 0:
                Final_Print[0].Print_Tower(head_index, mount_index, base_index);
                break;
            case 1:
                Final_Print[1].Print_Tower(head_index, mount_index, base_index);
                break;
            case 2:
                Final_Print[2].Print_Tower(head_index, mount_index, base_index);
                break;
        }
    }

    public int[] Return_index()
    {
        return new int[] { head_index, mount_index, base_index };
    }
}
