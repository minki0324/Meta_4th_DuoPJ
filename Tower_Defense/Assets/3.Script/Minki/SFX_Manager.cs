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
        ��ĭ ��Ʈ         : 0 ~ 5
        ��ĭ ��           : 6
        �������� ��Ʈ     : 7 ~ 12
        �������� ��       : 13
        ������ ����       : 14
        ������ ����       : 15
        ������ Ŭ����     : 16
        ��Ŀ ��Ʈ         : 17 ~ 19
        ��Ŀ ��           : 20
        ���� ��Ʈ         : 21 ~ 24
        ���� ��           : 25
        �÷��� ����       : 26
        �÷��� ����       : 27
        �÷��� Ŭ����     : 28
        �ö�� ����     : 29
        �ö�� ����     : 30
        �ö�� Ŭ����   : 31
        ���������޽� ��Ʈ : 32 ~ 38
        ���������޽� ��   : 39
    */
    [Header("Clip Array")]
    public AudioClip[] clip_Array;

    [Header("Vulcan")] // ��ĭ
    public AudioClip[] vulcanHit; // Impact prefabs array  
    public AudioClip vulcanShot; // Shot prefab
    public float vulcanDelay; // Shot delay in ms
    public float vulcanHitDelay; // Hit delay in ms
    
    [Header("Sniper")] // ��������
    public AudioClip[] sniperHit;
    public AudioClip sniperShot;
    public float sniperDelay;
    public float sniperHitDelay;

    [Header("Laser")] // ������
    public AudioClip plasmabeamOpen; // Open audio clip prefab
    public AudioClip plasmabeamLoop; // Loop audio clip prefab
    public AudioClip plasmabeamClose; // Close audio clip prefab
    
    [Header("Seeker")] // ��Ŀ
    public AudioClip[] seekerHit;
    public AudioClip seekerShot;
    public float seekerDelay;
    public float seekerHitDelay;
    
    [Header("Air")] // ����
    public AudioClip[] soloGunHit;
    public AudioClip soloGunShot;
    public float soloGunDelay;
    public float soloGunHitDelay;

    [Header("Flame gun")] // �÷���
    public AudioClip flameGunOpen;
    public AudioClip flameGunLoop;
    public AudioClip flameGunClose;

    [Header("Plasma Laser")] // �ö�� ������
    public AudioClip plasmabeamHeavyOpen;
    public AudioClip plasmabeamHeavyLoop;
    public AudioClip plasmabeamHeavyClose;
    
    [Header("Laser impulse")] // ������ ���޽�
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
