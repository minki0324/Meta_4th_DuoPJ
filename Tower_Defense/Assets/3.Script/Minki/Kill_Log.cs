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

        // �޽����� �߰��� ������ Ÿ�̸� �����
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
                //�̴� �۾�
            }
            message_Box[message_Box.Length - 1].text = me;
        }
    }

    private IEnumerator MessageTimeoutCoroutine()
    {
        // 5�� ���
        yield return new WaitForSeconds(5);

        // 5�� �Ŀ� �޽��� �ڽ� ��Ȱ��ȭ
        foreach (Text message in message_Box)
        {
            message.gameObject.SetActive(false);
        }
    }
}
