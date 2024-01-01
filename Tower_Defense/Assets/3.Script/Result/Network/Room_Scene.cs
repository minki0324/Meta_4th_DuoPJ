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

    [SerializeField] private Image P1_Character;
    [SerializeField] private Image P2_Character;

    [SerializeField] private Sprite[] Cha_img;

    [SerializeField] private Room_Player P1_Component;
    [SerializeField] private Room_Player P2_Component;
    [SerializeField] private Room_Manager manager;

    [SerializeField] private GameObject Prefabs_Panel;
    [SerializeField] private GameObject info_Panel;

    #region Unity Callback
    private void Start()
    {
        manager = FindObjectOfType<Room_Manager>();
    }

    private void Update()
    {
        Ready_Check();
    }
    #endregion

    public void Switch_Panel()
    {
        bool switch_ = Prefabs_Panel.activeSelf;
        Prefabs_Panel.SetActive(!switch_);
        info_Panel.SetActive(switch_);
    }

    private void Find_Player()
    {
        if (manager.roomSlots.Count == 0)
        {
            return;
        }
        else if (manager.roomSlots.Count == 2)
        {
            P1_Component = manager.roomSlots[0].GetComponent<Room_Player>();
            P1_Character.sprite = Cha_img[0];
            P2_Component = manager.roomSlots[1].GetComponent<Room_Player>();
            P2_Character.sprite = Cha_img[1];
        }
        else if (manager.roomSlots.Count == 1)
        {
            P1_Component = manager.roomSlots[0].GetComponent<Room_Player>();
            P1_Character.sprite = Cha_img[0];
            P2_Character.sprite = null;
        }
    }

    private void Ready_Check()
    {
        if (manager.roomSlots.Count == 1)
        {
            P1_Component = manager.roomSlots[0].GetComponent<Room_Player>();
            if (P1_Component.readyToBegin)
            {
                P1_Wait.SetActive(false);
                P1_Ready.SetActive(true);
            }
            else
            {
                P1_Ready.SetActive(false);
                P1_Wait.SetActive(true);
            }
        }
        else if (manager.roomSlots.Count == 2)
        {
            P1_Component = manager.roomSlots[0].GetComponent<Room_Player>();
            if (P1_Component.readyToBegin)
            {
                P1_Wait.SetActive(false);
                P1_Ready.SetActive(true);
            }
            else
            {
                P1_Ready.SetActive(false);
                P1_Wait.SetActive(true);
            }
            P2_Component = manager.roomSlots[1].GetComponent<Room_Player>();
            if (P2_Component.readyToBegin)
            {
                P2_Wait.SetActive(false);
                P2_Ready.SetActive(true);
            }
            else
            {
                P2_Ready.SetActive(false);
                P2_Wait.SetActive(true);
            }
        }
    }

    public void OnReadyBtn_Click()
    {
        if (manager.roomSlots.Count == 1)
        {
            if (P1_Component.isLocalPlayer)
            {
                if (P1_Component.readyToBegin)
                {
                    P1_Component.CmdChangeReadyState(false);
                }
                else
                {
                    P1_Component.CmdChangeReadyState(true);
                }
            }
            else
            {
                return;
            }
        }
        else if (manager.roomSlots.Count == 2)
        {
            if (P1_Component.isLocalPlayer)
            {
                if (P1_Component.readyToBegin)
                {
                    P1_Component.CmdChangeReadyState(false);
                }
                else
                {
                    P1_Component.CmdChangeReadyState(true);
                }
            }
            else if (P2_Component.isLocalPlayer)
            {
                if (P2_Component.readyToBegin)
                {
                    P2_Component.CmdChangeReadyState(false);
                }
                else
                {
                    P2_Component.CmdChangeReadyState(true);
                }
            }
            else
            {
                return;
            }
        }
    }
}
