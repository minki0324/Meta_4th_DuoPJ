using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniOption : MonoBehaviour
{
    [SerializeField] private Slider master;
    [SerializeField] private Slider bgm;
    [SerializeField] private Slider sfx;


    private void Start()
    {

        master.value = AudioManager.instance.tempMaster;
        bgm.value = AudioManager.instance.tempBGM;
        sfx.value = AudioManager.instance.tempSFX;

        AudioManager.instance.BGMVolume = bgm;
        AudioManager.instance.SFXVolume = sfx;
        AudioManager.instance.MasterVolume = master;

    }
}
