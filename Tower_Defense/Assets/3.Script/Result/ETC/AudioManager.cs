using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private int randNum;
   [SerializeField] private Sound[] sfx = null;
    [SerializeField] private Sound[] bgm = null;

    [SerializeField] public AudioSource bgmPlayer = null;
    [SerializeField] public AudioSource[] sfxPlayer = null;
    //[SerializeField] private GameObject Option;
    [SerializeField] public Slider MasterVolume;
    [SerializeField] public Slider SFXVolume;
    [SerializeField] public Slider BGMVolume;
    public float tempBGM;
    public float tempSFX;
    public float tempMaster;
    [SerializeField] private Text Label;
    [SerializeField] private Dropdown dropdown;
    private Dictionary<string, AudioSource> sfxPlayers = new Dictionary<string, AudioSource>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // SFX�� AudioSource�� �ʱ�ȭ
        foreach (Sound sound in sfx)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.clip = sound.clip;
            sfxPlayers.Add(sound.name, sfxSource);
        }
    }
    private void Start()
    {
        RandomPlay();
        dropdown.value = randNum;
        MasterVolume.value = 0.5f;
        SFXVolume.value = 0.5f;
        BGMVolume.value = 0.0f;
        
    }
    private void Update()
    {
      
            if (!bgmPlayer.isPlaying)
            {
                RandomPlay();
            }
      
      
        if(BGMVolume !=null && SFXVolume != null)
        {
            bgmPlayer.volume = BGMVolume.value * MasterVolume.value;
            SFXVolumeSet();

            tempSFX = SFXVolume.value;
            tempBGM = BGMVolume.value;
            tempMaster = MasterVolume.value;
        }


    }

    private void SFXVolumeSet()
    {
          foreach (AudioSource sound in sfxPlayer)
        {
            sound.volume = SFXVolume.value * MasterVolume.value;
        }
    }

    private void RandomPlay()
    {
        //�����ϸ� �����ѹ� �̰�
        //�����ѹ��� BGM ���
        //�ٷ� ���� ������ BGM�� �ȳ����� ����
        int temp = randNum;
        randNum = Random.Range(1, 8);
         
        while (randNum == temp)
        {
            temp = randNum;
            randNum = Random.Range(1, 9);
        }
     
        PlayBGM(bgm[randNum].name);
    }
    public void LabelChange()
    {
        PlayBGM(Label.text);
    }
    public void PlayBGM(string p_bgmName)
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            if (p_bgmName == bgm[i].name)
            {
                bgmPlayer.clip = bgm[i].clip;
                bgmPlayer.Play();
            }
        }
    }

    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    public AudioSource PlaySFX(string p_sfxName)
    {
        if (sfxPlayers.ContainsKey(p_sfxName))
        {
            AudioSource sfxPlayer = sfxPlayers[p_sfxName];
            sfxPlayer.Play();
            return sfxPlayer;
        }
        else
        {
            Debug.Log(p_sfxName + " �̸��� ȿ������ �����ϴ�.");
            return null;
        }
    }


    public void StopSFX(AudioSource sfxSource)
    {
        if (sfxSource != null && sfxSource.isPlaying)
        {
            sfxSource.Stop();
        }
    }
    public void ScreenFullScreenMode()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }
    public void WindowSCreenMode()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
    }
}
