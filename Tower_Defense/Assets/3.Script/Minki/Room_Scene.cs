using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room_Scene : MonoBehaviour
{
    [SerializeField] private Text P1_Ready;
    [SerializeField] private Text P2_Ready;

    [SerializeField] private Image P1_Character;
    [SerializeField] private Image P2_Character;

    [SerializeField] private Sprite[] Cha_img;

    [SerializeField] private Room_Player P1_Component;
    [SerializeField] private Room_Player P2_Component;
    [SerializeField] private Room_Manager manager;

    #region Unity Callback
    private void Start()
    {
        manager = FindObjectOfType<Room_Manager>();
    }

    private void Update()
    {
        Find_Player();
        Ready_Check();
    }
    #endregion

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
            if (P1_Component.readyToBegin)
            {
                P1_Ready.text = "Ready";
            }
            else
            {
                P1_Ready.text = "Wait For Player";
            }
            P2_Ready.text = "";
        }
        else if (manager.roomSlots.Count == 2)
        {
            if (P1_Component.readyToBegin)
            {
                P1_Ready.text = "Ready";
            }
            else
            {
                P1_Ready.text = "Wait For Player";
            }

            if (P2_Component.readyToBegin)
            {
                P2_Ready.text = "Ready";
            }
            else
            {
                P2_Ready.text = "Wait For Player";
            }
        }
    }

    private void Set_Name()
    {

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
