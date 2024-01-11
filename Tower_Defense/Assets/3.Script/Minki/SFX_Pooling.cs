using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SFX_Pooling : NetworkBehaviour
{
    public SyncList<GameObject> SFX_List = new SyncList<GameObject>();
    public GameObject SFX_Obj;
    public GameObject Pool_Parent;
    public int PoolSize;
    #region Unity Callback
    private void Start()
    {
        if (isServer)
        {
            StartCoroutine(DelayedInit());
        }

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
    private void RPC_DeactivateAllChildren()
    {
        for (int i = 0; i < Pool_Parent.transform.childCount; i++)
        {
            Pool_Parent.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void RpcAudioInfo(GameObject Audio)
    {
        Audio.transform.SetParent(Pool_Parent.transform);
    }

    #endregion
    #region Hook Method
    #endregion
    #region Server
    [Server]
    private IEnumerator DelayedInit()
    {
        // 서버 시작 후 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 풀 초기화
        Init_Pool();
    }

    [Server]
    private void Init_Pool()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            GetAudio(Vector3.zero);
        }

        DeactivateAllChildren(Pool_Parent);
        RPC_DeactivateAllChildren();
    }

    [Server]
    private void DeactivateAllChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    [Server]
    public GameObject GetAudio(Vector3 pos, AudioClip clip = null)
    {
        foreach (GameObject audio in SFX_List)
        {
            if (!audio.activeInHierarchy)
            {
                audio.SetActive(true);
                GameManager.instance.RPC_ActiveSet(true, audio);
                if(clip != null)
                {
                    AudioSource source = audio.GetComponent<AudioSource>();
                    source.clip = clip;
                }
                audio.transform.position = pos;
                return audio;
            }
        }

        GameObject newAudio = Instantiate(SFX_Obj, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(newAudio);
        SFX_List.Add(newAudio);

        newAudio.transform.SetParent(Pool_Parent.transform);
        RpcAudioInfo(newAudio);
        if (clip != null)
        {
            AudioSource source = newAudio.gameObject.GetComponent<AudioSource>();
            source.clip = clip;
        }
        newAudio.transform.position = pos;

        return newAudio;
    }
    #endregion
}
