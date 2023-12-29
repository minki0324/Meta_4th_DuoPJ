using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Room_PlayerSet : NetworkBehaviour
{
    public Image Player_image;
    public Text Player_name;
    public Sprite[] Image_Set;

    #region Unity Callback
    #endregion
    #region SyncVar
    [SyncVar(hook = nameof(Change_Index))]
    public int img_index;
    [SyncVar(hook = nameof(Change_Name))]
    public string Nickname;
    #endregion
    #region Client
    [Client]
    public void Request_info(int index, string name)
    {
        if(isOwned)
        {
            img_index = index;
            Nickname = name;
            CMD_Set_Playerinfo(img_index, Nickname, (int)netId - 1);
            Debug.Log("클라");
        }
    }
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    private void CMD_Set_Playerinfo(int index, string name, int player_index)
    {
        Debug.Log("커맨드");
        Debug.Log("플레이어 인덱스 : " + player_index);
        Room_PrintPlayerInfo[] infos = FindObjectsOfType<Room_PrintPlayerInfo>();
        foreach (var info in infos)
        {
            switch((int)info.num)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    Debug.Log("이넘 넘버 : " + (int)info.num);
                    if (player_index != (int)info.num) continue;
                    info.Player_Image.sprite = info.Image_Set[index];
                    info.Player_Name.text = name;
                    break;
            }
        }
        RPC_Print_info(index, name, player_index);
    }
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_Print_info(int index, string name, int player_index)
    {
        Room_PrintPlayerInfo[] infos = FindObjectsOfType<Room_PrintPlayerInfo>();
        Debug.Log("인포 랭쓰 : " + infos.Length);
        Debug.Log("플레이어 인덱스 : " + player_index);
        foreach (var info in infos)
        {
            switch ((int)info.num)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    if (player_index != (int)info.num) continue;
                    info.Player_Image.sprite = info.Image_Set[index];
                    info.Player_Name.text = name;
                    break;
            }
        }
    }
    #endregion
    #region Hook Method
    public void Change_Index(int old_, int new_)
    {
        img_index = new_;
    }

    public void Change_Name(string old_, string new_)
    {
        Nickname = new_;
    }
    #endregion
}
