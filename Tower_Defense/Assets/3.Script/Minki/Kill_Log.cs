using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Kill_Log : NetworkBehaviour
{
    public static Kill_Log instance;

    [SerializeField] private Text[] message_Box;

    public Action<string> Message;

    string current_me = string.Empty;
    string past_me;

    private Coroutine messageTimeoutCoroutine;

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

    private void Start()
    {
        message_Box = transform.GetComponentsInChildren<Text>();
        Message = Adding_Message;
        past_me = current_me;
    }
    
    [ClientRpc]
    public void Adding_Message(string m)
    {
        current_me = m;
        ReadText(current_me);

        // 메시지가 추가될 때마다 타이머 재시작
        if (messageTimeoutCoroutine != null)
        {
            StopCoroutine(messageTimeoutCoroutine);
        }
        messageTimeoutCoroutine = StartCoroutine(MessageTimeoutCoroutine());
    }

    public void ReadText(string me)
    {
        bool isinput = false;

        foreach (Text message in message_Box)
        {
            message.gameObject.SetActive(true);
        }

        for (int i = 0; i < message_Box.Length; i++)
        {
            if (message_Box[i].text.Equals(""))
            {
                message_Box[i].text = me;
                isinput = true;
                break;
            }
        }
        if (!isinput)
        {
            for (int i = 1; i < message_Box.Length; i++)
            {
                message_Box[i - 1].text = message_Box[i].text;
                //미는 작업
            }
            message_Box[message_Box.Length - 1].text = me;
        }
    }

    private IEnumerator MessageTimeoutCoroutine()
    {
        // 5초 대기
        yield return new WaitForSeconds(5);

        // 5초 후에 메시지 박스 비활성화
        foreach (Text message in message_Box)
        {
            message.gameObject.SetActive(false);
        }
    }
}
