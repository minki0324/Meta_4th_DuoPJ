using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private Sound[] sfx = null;
    [SerializeField] private Sound[] bgm = null;

    [SerializeField] private AudioSource bgmPlayer = null;
    [SerializeField] private AudioSource[] sfxPlayer = null;

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

        // SFX용 AudioSource를 초기화
        foreach (Sound sound in sfx)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.clip = sound.clip;
            sfxPlayers.Add(sound.name, sfxSource);
        }
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
            Debug.Log(p_sfxName + " 이름의 효과음이 없습니다.");
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
}
