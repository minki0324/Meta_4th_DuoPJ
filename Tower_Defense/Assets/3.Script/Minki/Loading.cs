using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    private Room_Manager manager;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Image Logo;

    private void Start()
    {
        manager = FindObjectOfType<Room_Manager>();

        if(manager == null)
        {
            return;
        }
        else
        {
            manager.loadingScreen = this.gameObject;
        }
        StartCoroutine(Loading_co(2));
    }

    private IEnumerator Loading_co(int duration)
    {
        Color newColor = gameObject.GetComponent<Image>().color;
        newColor.a = 255;
        gameObject.GetComponent<Image>().color = newColor;

        Color newColor_ = Logo.color;
        newColor.a = 255;
        Logo.color = newColor_;

        float elapsed = 0; // ��� �ð��� �����ϴ� ����

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // ��� �ð� ������Ʈ
            loadingSlider.value = elapsed / duration; // �����̴� �� ����

            yield return null; // ���� �����ӱ��� ���
        }

        loadingSlider.value = 1; // �Ϸ�Ǹ� �����̴��� �ִ밪���� ����
        yield return new WaitForSeconds(1f);
        StartCoroutine(Fade.instance.fade_out(gameObject.GetComponent<Image>(), false, 1.5f, gameObject));
        StartCoroutine(Fade.instance.fade_out(Logo, false, 1.5f));
    }
}
