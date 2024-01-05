using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerPanel : MonoBehaviour
{
    public GameObject[] orderButtons;
    public Sprite NoneImage;
    private void Start()
    {
        orderButtons = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            orderButtons[i] = transform.GetChild(i).GetChild(2).gameObject;
        }

        for (int i = 6; i < orderButtons.Length; i++)
        {
            orderButtons[i].GetComponent<Image>().sprite = NoneImage;
        }
    }
}
