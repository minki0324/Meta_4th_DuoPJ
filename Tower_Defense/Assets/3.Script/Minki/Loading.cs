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

        float elapsed = 0; // 경과 시간을 추적하는 변수

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // 경과 시간 업데이트
            loadingSlider.value = elapsed / duration; // 슬라이더 값 설정

            yield return null; // 다음 프레임까지 대기
        }

        loadingSlider.value = 1; // 완료되면 슬라이더를 최대값으로 설정
        yield return new WaitForSeconds(1f);
        StartCoroutine(Fade.instance.fade_out(gameObject.GetComponent<Image>(), false, 1.5f, gameObject));
        StartCoroutine(Fade.instance.fade_out(Logo, false, 1.5f));
    }
}
