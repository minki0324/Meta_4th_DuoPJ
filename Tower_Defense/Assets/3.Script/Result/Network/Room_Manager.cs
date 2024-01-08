using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Room_Manager : NetworkRoomManager
{
    public GameObject loadingScreen; // 로딩 화면 객체에 대한 참조

    public Sprite[] Player_images;
    public string[] Player_name;

    public Image[] Print_images;
    public Text[] Print_names;

    public int[] image_indexs;

    public override void OnGUI()
    {
        
    }



    public override void OnRoomServerPlayersReady()
    {
        if(loadingScreen == null)
        {
            loadingScreen = GameObject.FindGameObjectWithTag("Loading");
        }

        // 모든 플레이어가 준비 상태일 때 호출됩니다.
        // 로딩 화면 활성화
        loadingScreen.SetActive(true);

        // 게임 씬으로 전환
        ServerChangeScene(GameplayScene);
    }
}
