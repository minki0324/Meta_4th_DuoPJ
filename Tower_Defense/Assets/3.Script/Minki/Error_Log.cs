using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Error_Log : NetworkBehaviour
{
    public static Error_Log instance;
    public GameObject Log_Panel;
    public Text Log_text;

    private Coroutine panel;
    private Coroutine txt;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    [ClientRpc]
    public void RPC_LogTarget(string log, Player_Num player)
    {
        if(player == GameManager.instance.Player_Num)
        {
            Print_Log(log);
        }
    }

    [ClientRpc]
    public void RPC_Log(string log)
    {
        Print_Log(log);
    }

    public void Print_Log(string log)
    {
        if(panel != null || txt != null)
        {
            StopCoroutine(panel);
            StopCoroutine(txt);
        }

        panel = StartCoroutine(Fade.instance.fade_out(Log_Panel.GetComponent<Image>(), false, 3f));
        Log_text.text = log;
        txt = StartCoroutine(Fade.instance.fade_out(Log_text.GetComponent<Text>(), false, 3f));
    }
}
