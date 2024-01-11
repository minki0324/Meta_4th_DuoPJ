using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SFX_Manager : NetworkBehaviour
{
    // Audio timers
    float timer_01, timer_02;
    public Transform audioSource;

    /*
        불칸 히트         : 0 ~ 5
        불칸 샷           : 6
        스나이퍼 히트     : 7 ~ 12
        스나이퍼 샷       : 13
        레이저 오픈       : 14
        레이저 루프       : 15
        레이저 클로즈     : 16
        시커 히트         : 17 ~ 19
        시커 샷           : 20
        에어 히트         : 21 ~ 24
        에어 샷           : 25
        플레임 오픈       : 26
        플레임 루프       : 27
        플레임 클로즈     : 28
        플라즈마 오픈     : 29
        플라즈마 루프     : 30
        플라즈마 클로즈   : 31
        레이저임펄스 히트 : 32 ~ 38
        레이저임펄스 샷   : 39
    */
    [Header("Clip Array")]
    public AudioClip[] clip_Array;

    [Header("Vulcan")] // 불칸
    public AudioClip[] vulcanHit; // Impact prefabs array  
    public AudioClip vulcanShot; // Shot prefab
    public float vulcanDelay; // Shot delay in ms
    public float vulcanHitDelay; // Hit delay in ms
    
    [Header("Sniper")] // 스나이퍼
    public AudioClip[] sniperHit;
    public AudioClip sniperShot;
    public float sniperDelay;
    public float sniperHitDelay;

    [Header("Laser")] // 레이저
    public AudioClip plasmabeamOpen; // Open audio clip prefab
    public AudioClip plasmabeamLoop; // Loop audio clip prefab
    public AudioClip plasmabeamClose; // Close audio clip prefab
    
    [Header("Seeker")] // 시커
    public AudioClip[] seekerHit;
    public AudioClip seekerShot;
    public float seekerDelay;
    public float seekerHitDelay;
    
    [Header("Air")] // 에어
    public AudioClip[] soloGunHit;
    public AudioClip soloGunShot;
    public float soloGunDelay;
    public float soloGunHitDelay;

    [Header("Flame gun")] // 플레임
    public AudioClip flameGunOpen;
    public AudioClip flameGunLoop;
    public AudioClip flameGunClose;

    [Header("Plasma Laser")] // 플라즈마 레이저
    public AudioClip plasmabeamHeavyOpen;
    public AudioClip plasmabeamHeavyLoop;
    public AudioClip plasmabeamHeavyClose;
    
    [Header("Laser impulse")] // 레이저 임펄스
    public AudioClip[] laserImpulseHit;
    public AudioClip laserImpulseShot;
    public float laserImpulseDelay;
    public float laserImpulseHitDelay;

    private SFX_Pooling pool;

    #region Unity Callback
    private void Awake()
    {
        pool = FindObjectOfType<SFX_Pooling>();
    }
    private void Update()
    {
        timer_01 += Time.deltaTime;
        timer_02 += Time.deltaTime;
    }
    #endregion
    #region SyncVar
    #endregion
    #region Client
    #endregion
    #region Command
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_SFX_Play(GameObject Obj, float pitch, float volume, bool loop, int index)
    {
        AudioSource audio = Obj.GetComponent<AudioSource>();

        if (audio != null)
        {
            audio.clip = clip_Array[index];
            audio.pitch = Random.Range(pitch, 1f);
            audio.volume = Random.Range(volume, 1f);
            audio.loop = loop;
            audio.Play();
        }
    }
    #endregion
    #region Hook Method
    #endregion
    #region Server
    [Server]
    public void SFX_VulcanShot(Vector3 pos)
    {
        if (timer_01 >= vulcanHitDelay)
        {
            GameObject audio = pool.GetAudio(pos, vulcanShot);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.95f, 0.8f, false, 5);
            timer_01 = 0f;
        }
    }

    [Server]
    public void SFX_VulcanHit(Vector3 pos)
    {
        if (timer_02 >= vulcanHitDelay)
        {
            GameObject audio = pool.GetAudio(pos, vulcanHit[Random.Range(0, vulcanHit.Length)]);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.95f, 0.6f, false, Random.Range(0, 6));
            timer_02 = 0f;
        }
    }

    [Server]
    public void SFX_SniperShot(Vector3 pos)
    {
        if (timer_01 >= sniperDelay)
        {
            GameObject audio = pool.GetAudio(pos, sniperShot);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.9f, 0.8f, false, 13);

            timer_01 = 0f;
        }
    }

    [Server]
    public void SFX_SniperHit(Vector3 pos)
    {
        if (timer_02 >= sniperHitDelay)
        {
            GameObject audio = pool.GetAudio(pos, sniperHit[Random.Range(0, sniperHit.Length)]);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.9f, 0.8f, false, Random.Range(7, 13));

            timer_02 = 0f;
        }
    }

    [Server]
    public void SFX_Laser(Vector3 pos, float delay_)
    {
        GameObject audio = pool.GetAudio(pos, plasmabeamLoop);
        Despawn_audio delay = audio.GetComponent<Despawn_audio>();
        delay.delay = delay_;
        GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
        RPC_SFX_Play(audio, 0.7f, 0.55f, false, 15);
    }

    [Server]
    public void SFX_SeekerShot(Vector3 pos)
    {
        if (timer_01 >= seekerDelay)
        {
            GameObject audio = pool.GetAudio(pos, seekerShot);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.8f, 0.8f, false, 20);

            timer_01 = 0f;
        }
    }

    [Server]
    public void SFX_SeekerHit(Vector3 pos)
    {
        if (timer_02 >= sniperHitDelay)
        {
            GameObject audio = pool.GetAudio(pos, seekerHit[Random.Range(0, seekerHit.Length)]);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.8f, 0.8f, false, Random.Range(17, 20));

            timer_02 = 0f;
        }
    }

    [Server]
    public void SFX_AirShot(Vector3 pos)
    {
        if (timer_01 >= soloGunDelay)
        {
            GameObject audio = pool.GetAudio(pos, soloGunShot);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.95f, 0.8f, false, 25);

            timer_01 = 0f;
        }
    }

    [Server]
    public void SFX_AirHit(Vector3 pos)
    {
        if (timer_02 >= sniperHitDelay)
        {
            GameObject audio = pool.GetAudio(pos, soloGunHit[Random.Range(0, soloGunHit.Length)]);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.95f, 0.8f, false, Random.Range(21, 24));

            timer_02 = 0f;
        }
    }

    [Server]
    public void SFX_Flame(Vector3 pos, float delay_)
    {
        GameObject audio = pool.GetAudio(pos, flameGunLoop);
        Despawn_audio delay = audio.GetComponent<Despawn_audio>();
        delay.delay = delay_;
        GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
        RPC_SFX_Play(audio, 0.7f, 0.55f, false, 27);
    }

    [Server]
    public void SFX_Plasma(Vector3 pos)
    {
        if (timer_01 >= soloGunDelay)
        {
            GameObject audio = pool.GetAudio(pos, plasmabeamHeavyLoop);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.95f, 0.95f, false, 30);

            timer_01 = 0f;
        }
    }

    [Server]
    public void SFX_LaserImpulseShot(Vector3 pos)
    {
        if (timer_01 >= soloGunDelay)
        {
            GameObject audio = pool.GetAudio(pos, laserImpulseShot);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.9f, 0.8f, false, 39);

            timer_01 = 0f;
        }
    }

    [Server]
    public void SFX_LaserImpulseHit(Vector3 pos)
    {
        if (timer_02 >= sniperHitDelay)
        {
            GameObject audio = pool.GetAudio(pos, laserImpulseHit[Random.Range(0, laserImpulseHit.Length)]);
            GameManager.instance.RPC_TransformSet(audio, pos, Quaternion.identity);
            RPC_SFX_Play(audio, 0.8f, 0.8f, false, Random.Range(32, 39));

            timer_02 = 0f;
        }
    }
    #endregion
}
