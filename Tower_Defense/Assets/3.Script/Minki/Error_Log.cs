using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Error_Log : MonoBehaviour
{
    public static Error_Log instance;
    public GameObject Log_Panel;
    public GameObject mineral;
    public GameObject crystal;
    public GameObject food;
    public GameObject land;
    public GameObject gilmak;
    public GameObject life;

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

    public void Print_Log(GameObject log)
    {
        StartCoroutine(Fade.instance.fade_out(Log_Panel.GetComponent<Image>(), false, 2f));
        StartCoroutine(Fade.instance.fade_out(log.GetComponent<Text>(), false, 2f));
    }
}
