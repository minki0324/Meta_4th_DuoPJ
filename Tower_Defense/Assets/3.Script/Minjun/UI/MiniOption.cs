using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniOption : MonoBehaviour
{
    [SerializeField] private GameObject Option_Panel;
    [SerializeField] private GameObject ViewPos_Panel;

    [SerializeField] private Slider master;
    [SerializeField] private Slider bgm;
    [SerializeField] private Slider sfx;

    [SerializeField] private Slider mouse;

    [SerializeField] private GameObject camera_;
    

    private void Start()
    {

        master.value = AudioManager.instance.tempMaster;
        bgm.value = AudioManager.instance.tempBGM;
        sfx.value = AudioManager.instance.tempSFX;

        AudioManager.instance.BGMVolume = bgm;
        AudioManager.instance.SFXVolume = sfx;
        AudioManager.instance.MasterVolume = master;

    }

    

    public void Open_Option()
    {
        bool active = Option_Panel.activeSelf;
        ViewPos_Panel.SetActive(false);
        Option_Panel.SetActive(!active);
    }

    public void Open_ViewPos()
    {
        bool active = ViewPos_Panel.activeSelf;
        Option_Panel.SetActive(false);
        ViewPos_Panel.SetActive(!active);
    }
}
