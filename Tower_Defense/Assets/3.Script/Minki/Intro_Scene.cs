using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro_Scene : MonoBehaviour
{
    [SerializeField] private GameObject Loading_Screen;

    public void Next_Scene()
    {
        Loading_Screen.SetActive(true);
    }
}
