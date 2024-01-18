using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseCameraCon : MonoBehaviour
{
    public float mouseMoveSpeed = 35f;
    public float KeyMoveSpeed = 35f;
    private float edgeThreshold = 3f;
    public float AddValue =0.5f;
    [SerializeField] private BoxCollider CameraZoneCol;
    [SerializeField] private Slider YvalueSlider;
    private Bounds CameraZone;
    private bool isScreenLock;
    private Vector3 previousCameraPosition; // 이전 프레임의 카메라 위치
    public Image Render_img;
    public RectTransform miniMapRect; // 미니맵의 RectTransform

    private float cameraY = 30f; // 초기 카메라 Y 위치
    private float minCameraY = 26f; // 최소 카메라 Y 위치
    private float maxCameraY = 34f; // 최대 카메라 Y 위치

    private void Start()
    {
        // 초기 카메라 위치 저장
        previousCameraPosition = Camera.main.transform.position;
        CameraZone = CameraZoneCol.bounds;
        UpdateRenderImagePosition(Camera.main.transform.position);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            isScreenLock = !isScreenLock;
        }

        if (!isScreenLock)
        {
            KeybordMove();
            MoveCameraWithMouse();
        }

        // 카메라가 움직였는지 확인
        if (Camera.main.transform.position != previousCameraPosition)
        {
            // Render_img 위치 업데이트
            UpdateRenderImagePosition(Camera.main.transform.position);
            // 현재 카메라 위치 저장
            previousCameraPosition = Camera.main.transform.position;
        }
    }

    void MoveCameraWithMouse()
    {

        Vector3 mousePosition = Input.mousePosition;
        // 화면 가장자리에 닿기 위한 간격

        if (mousePosition.x < edgeThreshold)
        {
            // 마우스가 왼쪽 가장자리에 닿았을 때
            transform.Translate(Vector3.left * mouseMoveSpeed * Time.deltaTime);
        }
        else if (mousePosition.x > Screen.width - edgeThreshold)
        {
            // 마우스가 오른쪽 가장자리에 닿았을 때
            transform.Translate(Vector3.right * mouseMoveSpeed * Time.deltaTime);
        }

        if (mousePosition.y < edgeThreshold)
        {
            // 마우스가 아래 가장자리에 닿았을 때
            transform.Translate(Vector3.down * mouseMoveSpeed * Time.deltaTime);
        }
        else if (mousePosition.y > Screen.height - edgeThreshold)
        {
            // 마우스가 위 가장자리에 닿았을 때
            transform.Translate(Vector3.up * mouseMoveSpeed * Time.deltaTime);
        }
        float clampedX = Mathf.Clamp(Camera.main.transform.position.x, CameraZone.min.x, CameraZone.max.x);
        float clampedZ = Mathf.Clamp(Camera.main.transform.position.z, CameraZone.min.z, CameraZone.max.z);

        // 카메라 위치를 설정
        Camera.main.transform.position = new Vector3(clampedX, cameraY, clampedZ);
    }

    private void KeybordMove()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(Vector3.up * KeyMoveSpeed * Time.deltaTime);
        }
         if (Input.GetKey(KeyCode.DownArrow))
        {

            transform.Translate(Vector3.down * KeyMoveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(Vector3.right * KeyMoveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.left * KeyMoveSpeed * Time.deltaTime);
        }
        float clampedX = Mathf.Clamp(Camera.main.transform.position.x, CameraZone.min.x, CameraZone.max.x);
        float clampedZ = Mathf.Clamp(Camera.main.transform.position.z, CameraZone.min.z, CameraZone.max.z);

        // 카메라 위치를 설정
        Camera.main.transform.position = new Vector3(clampedX, cameraY, clampedZ);
    }

    private void UpdateRenderImagePosition(Vector3 worldPosition)
    {
        // 미니맵 상의 새로운 위치 계산
        Vector2 newPosition = new Vector2((worldPosition.x - 90) / 300 * miniMapRect.sizeDelta.x,
            (worldPosition.z - 35) / 300 * miniMapRect.sizeDelta.y);

        // Render_img의 RectTransform을 업데이트
        Render_img.rectTransform.anchoredPosition = newPosition;
        // 이미지의 위치 업데이트
    }

    public void ViewPos(bool zoomIn)
    {
        if (zoomIn)
        {
            cameraY = Mathf.Clamp(cameraY + 1f, minCameraY, maxCameraY);
        }
        else
        {
            cameraY = Mathf.Clamp(cameraY - 1f, minCameraY, maxCameraY);
        }
    }
}
