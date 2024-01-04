using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room_Name_image : MonoBehaviour
{
    [SerializeField] private Sprite[] player_img;
    [SerializeField] private Image image;

    [SerializeField] private Button[] btn;
    [SerializeField] private InputField Nick_input;
    [SerializeField] private Room_Manager manager;
    public Room_PlayerSet[] room_;
    public int current_index = 0;

    public bool isReady = false;

    private void Awake()
    {
        manager = FindObjectOfType<Room_Manager>();
    }

    public void onClick_Front()
    {
        // 배열의 다음 sprite로 이동
        current_index = (current_index + 1) % player_img.Length;

        // Image에 새로운 sprite 할당
        Print_images(current_index);
    }

    public void onClick_Back()
    {
        // 배열의 이전 sprite로 이동
        current_index = (current_index - 1 + player_img.Length) % player_img.Length;

        // Image에 새로운 sprite 할당
        Print_images(current_index);
       
    }

    public void Print_images(int index)
    {
        image.sprite = player_img[current_index];
    }

    public void OnSave()
    {
        isReady = true;

        for(int i = 0; i < btn.Length; i++)
        {
            btn[i].interactable = false;
        }
        GameManager.instance.img_index = current_index;
        GameManager.instance.Nickname = Nick_input.text;

        room_ = FindObjectsOfType<Room_PlayerSet>();
        foreach (var player in room_)
        {
            player.Request_info(current_index, Nick_input.text);
        }
    }
}
