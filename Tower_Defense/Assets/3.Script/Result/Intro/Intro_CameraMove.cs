using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro_CameraMove : MonoBehaviour
{
    private bool isMoving = false;

    // 초기 위치 및 회전 저장
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [SerializeField] private GameObject Option_Panel_;

    void Start()
    {
        // 초기 위치 및 회전 설정
        initialPosition = new Vector3(170f, 1180f, -4510f);
        initialRotation = Quaternion.Euler(new Vector3(-30f, 130f, 0f));

        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    public void Option_Panel()
    {
        StartCoroutine(MoveAndRotateCamera(new Vector3(-650f, 1180f, -5500f), new Vector3(0f, 220f, -30f), 1f, true));
    }

    public void Return_Menu()
    {
        StartCoroutine(MoveAndRotateCamera(initialPosition, new Vector3(-30f, 130f, 0f), 1f, false));
    }

    private IEnumerator MoveAndRotateCamera(Vector3 targetPosition, Vector3 targetRotation, float duration, bool active)
    {
        if(!active)
        {
            Option_Panel_.SetActive(active);
        }
        
        isMoving = true;

        Vector3 initialPosition = transform.position;
        Quaternion initialRotation = transform.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsed / duration);
            transform.rotation = Quaternion.Lerp(initialRotation, Quaternion.Euler(targetRotation), elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and rotation are exactly as specified
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(targetRotation);

        isMoving = false;
        Option_Panel_.SetActive(active);
    }
}
