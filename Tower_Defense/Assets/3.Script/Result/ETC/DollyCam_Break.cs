using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class DollyCam_Break : MonoBehaviour
{
    public CinemachineVirtualCamera dollyCam;
    public CinemachineSmoothPath smoothPath;
    public CinemachineDollyCart dollyCart;
    [SerializeField] private GameObject warp;
    [SerializeField] private Image blur;
    [SerializeField] private GameObject dolly_obj;
    [SerializeField] private GameObject MainCamera;
    [SerializeField] private GameObject MainUI;

    public IEnumerator AdjustDollySpeed()
    {
        dollyCam.m_LookAt = warp.transform;

        // 초기 속도 설정
        float initialSpeed = 10000f;

        // 카트의 속도를 초기 속도로 설정
        dollyCart.m_Speed = initialSpeed;

        // 카트의 현재 위치
        float currentDistance = 0f;

        while (currentDistance < 35920)
        {
            // 카트의 현재 위치를 업데이트
            currentDistance += dollyCart.m_Speed * Time.deltaTime;
          
            dollyCart.m_Speed = Mathf.Lerp(initialSpeed, 0f, currentDistance / 50000f);

            yield return null;
        }

        dollyCart.m_Speed = 0f;
        yield return new WaitForSeconds(3f);

        StartCoroutine(Fade.instance.fade_out(blur, true, 2));
        while (currentDistance < 36100)
        {
            float accelerationSpeed = 100f;
            dollyCart.m_Speed = Mathf.Lerp(0f, 130f, Time.deltaTime * accelerationSpeed);

            if (dollyCart.m_Position >= 36000)
            {
                break;
            }
            yield return null;
        }

        // 최종 목적지에 도착했을 때 속도를 0으로 만듦
        dollyCart.m_Speed = 0f;

        // 게임 셋팅 초기화 지점
        dolly_obj.SetActive(false);
        // 카메라 위치 플레이어별로 따로따로 줘야함
        MainCamera.transform.position = new Vector3(0, 20, 0);
        MainCamera.transform.rotation = Quaternion.Euler(75, 0, 0);

        yield return new WaitForSeconds(1f);
        StartCoroutine(Fade.instance.fade_out(blur, false, 2));
        MainUI.SetActive(true);

        // 게임매니저에 불값 켜주기
    }
}