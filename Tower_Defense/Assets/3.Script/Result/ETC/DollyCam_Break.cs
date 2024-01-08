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

        // �ʱ� �ӵ� ����
        float initialSpeed = 10000f;

        // īƮ�� �ӵ��� �ʱ� �ӵ��� ����
        dollyCart.m_Speed = initialSpeed;

        // īƮ�� ���� ��ġ
        float currentDistance = 0f;

        while (currentDistance < 35920)
        {
            // īƮ�� ���� ��ġ�� ������Ʈ
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

        // ���� �������� �������� �� �ӵ��� 0���� ����
        dollyCart.m_Speed = 0f;

        // ���� ���� �ʱ�ȭ ����
        dolly_obj.SetActive(false);
        // ī�޶� ��ġ �÷��̾�� ���ε��� �����
        MainCamera.transform.position = new Vector3(0, 20, 0);
        MainCamera.transform.rotation = Quaternion.Euler(75, 0, 0);

        yield return new WaitForSeconds(1f);
        StartCoroutine(Fade.instance.fade_out(blur, false, 2));
        MainUI.SetActive(true);

        // ���ӸŴ����� �Ұ� ���ֱ�
    }
}