using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room_PrintPlayerInfo : MonoBehaviour
{
    public enum Player
    {
        P1 = 1,
        P2,
        P3,
        P4
    }

    public Player num;
    public Sprite[] Image_Set;
    public Image Player_Image;
    public Text Player_Name;
    public Text Ready_txt;
    public Text Wait_txt;



}
