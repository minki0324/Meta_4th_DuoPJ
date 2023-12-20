using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Player_Num
{
    P1 = 1,
    P2,
    P3,
    P4
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player_Num Player_Num;

    #region Unity Callback
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    public void Set_Player_Num()
    {
        Room_Player[] players = FindObjectsOfType<Room_Player>();

        foreach (var player in players)
        {
            if (player.isLocalPlayer)
            {
                Player_Num = (Player_Num)player.netId;
            }
        }
        Debug.Log(Player_Num);
    }
}
