using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Room_Manager : NetworkRoomManager
{
    public Sprite[] Player_images;
    public string[] Player_name;

    public Image[] Print_images;
    public Text[] Print_names;

    public int[] image_indexs;

    public override void OnGUI()
    {
        
    }
}
