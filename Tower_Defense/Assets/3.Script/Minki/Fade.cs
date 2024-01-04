using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    public static Fade instance;

    [SerializeField] public float fadeDuration = 2f; // ���̵� ��/�ƿ��� �ɸ� �ð� (��)
    [SerializeField] private float startTime = 0f;    // ���� �ð� ��Ͽ� ����

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

    public IEnumerator fade(Image image, bool isFadeIn)
    {
        float startAlpha = isFadeIn ? 0f : 0.6f;
        float endAlpha = isFadeIn ? 0.6f : 0f;

        // ���� �ð��� ���
        startTime = Time.time;

        float t = 0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime / fadeDuration;
            Color newColor = image.color;
            newColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
            image.color = newColor;
            yield return null; // ���� �����ӱ��� ���
        }
    }

    public IEnumerator fade_out(Image image, bool isFadeIn)
    {
        float startAlpha = isFadeIn ? 0f : 1f;
        float endAlpha = isFadeIn ? 1f : 0f;

        // ���� �ð��� ���
        startTime = Time.time;

        float t = 0f;

        while (t < 2f)
        {
            t += Time.deltaTime / fadeDuration;
            Color newColor = image.color;
            newColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
            image.color = newColor;
            yield return null; // ���� �����ӱ��� ���
        }
    }
}
