using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room_Prefabs : MonoBehaviour
{
    [SerializeField] private GameObject[] Mounts;
    [SerializeField] private GameObject[] Bases;

    public void Print_Tower(int head_index, int mount_index, int base_index)
    {
        for(int i = 0; i < Bases.Length; i++)
        {
            Bases[i].SetActive(false);
        }
        Bases[base_index].SetActive(true);

        for(int i = 0; i < Mounts.Length; i++)
        {
            Mounts[i].SetActive(false);
        }
        Mounts[mount_index].SetActive(true);

        Room_Mount head = Mounts[mount_index].GetComponent<Room_Mount>();
        Set_Mount_pos(base_index, head, Mounts[mount_index]);
        for (int i = 0; i < head.Heads.Length; i++)
        {
            head.Heads[i].SetActive(false);
        }
        head.Heads[head_index].SetActive(true);
    }

    private void Set_Mount_pos(int base_index, Room_Mount mount_pos, GameObject mount)
    {
        switch(base_index)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
                mount.transform.localPosition = mount_pos.Mount_Pos[base_index];
                break;
        }
    }
}
