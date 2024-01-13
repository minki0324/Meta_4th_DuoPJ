using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room_Scene : MonoBehaviour
{
    [SerializeField] private GameObject P1_Ready;
    [SerializeField] private GameObject P1_Wait;
    [SerializeField] private GameObject P2_Ready;
    [SerializeField] private GameObject P2_Wait;
    [SerializeField] private GameObject P3_Wait;
    [SerializeField] private GameObject P3_Ready;
    [SerializeField] private GameObject P4_Wait;
    [SerializeField] private GameObject P4_Ready;

    [SerializeField] private Image P1_Character;
    [SerializeField] private Image P2_Character;

    [SerializeField] private Sprite[] Cha_img;

    [SerializeField] private Room_Player[] players;
    [SerializeField] private Room_Player P1_Component;
    [SerializeField] private Room_Player P2_Component;
    [SerializeField] private Room_Manager manager;

    [SerializeField] private GameObject Prefabs_Panel;
    [SerializeField] private GameObject info_Panel;

    public bool Tower_Ready = false;
    public bool Image_Ready = false;
    [SerializeField] private GameObject Ready_Btn;

    #region Unity Callback
    private void Start()
    {
        manager = FindObjectOfType<Room_Manager>();
    }

    private void Update()
    {
        Ready_Check();
        if (Tower_Ready && Image_Ready)
        {
            Ready_Btn.SetActive(true);
        }
    }
    #endregion

    public void Switch_Panel()
    {
        bool switch_ = Prefabs_Panel.activeSelf;
        Prefabs_Panel.SetActive(!switch_);
        info_Panel.SetActive(switch_);
    }

    private void Ready_Check()
    {
        Room_Player currentPlayer;

        for (int i = 0; i < manager.roomSlots.Count; i++)
        {
            currentPlayer = manager.roomSlots[i].GetComponent<Room_Player>();

            if (currentPlayer != null && currentPlayer.readyToBegin)
            {
                if (i == 0)
                {
                    P1_Wait.SetActive(false);
                    P1_Ready.SetActive(true);
                }
                else if (i == 1)
                {
                    P2_Wait.SetActive(false);
                    P2_Ready.SetActive(true);
                }
                else if (i == 2)
                {
                    P3_Wait.SetActive(false);
                    P3_Ready.SetActive(true);
                }
                else if (i == 3)
                {
                    P4_Wait.SetActive(false);
                    P4_Ready.SetActive(true);
                }
            }
            else
            {
                if (i == 0)
                {
                    P1_Ready.SetActive(false);
                    P1_Wait.SetActive(true);
                }
                else if (i == 1)
                {
                    P2_Ready.SetActive(false);
                    P2_Wait.SetActive(true);
                }
                else if (i == 2)
                {
                    P3_Ready.SetActive(false);
                    P3_Wait.SetActive(true);
                }
                else if (i == 3)
                {
                    P4_Ready.SetActive(false);
                    P4_Wait.SetActive(true);
                }
            }
        }
    }

    public void OnReadyBtn_Click()
    {
        for (int i = 0; i < manager.roomSlots.Count; i++)
        {
            Room_Player currentPlayer = manager.roomSlots[i].GetComponent<Room_Player>();

            // 현재 로컬 플레이어인 경우에만 상태 변경
            if (currentPlayer != null && currentPlayer.isLocalPlayer)
            {
                currentPlayer.CmdChangeReadyState(!currentPlayer.readyToBegin);
                break; // 상태를 변경한 후 반복문 종료
            }
        }
    }
}
