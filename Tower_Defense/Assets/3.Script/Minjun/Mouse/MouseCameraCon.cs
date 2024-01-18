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
    private Vector3 previousCameraPosition; // ���� �������� ī�޶� ��ġ
    public Image Render_img;
    public RectTransform miniMapRect; // �̴ϸ��� RectTransform

    private float cameraY = 30f; // �ʱ� ī�޶� Y ��ġ
    private float minCameraY = 26f; // �ּ� ī�޶� Y ��ġ
    private float maxCameraY = 34f; // �ִ� ī�޶� Y ��ġ

    private void Start()
    {
        // �ʱ� ī�޶� ��ġ ����
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

        // ī�޶� ���������� Ȯ��
        if (Camera.main.transform.position != previousCameraPosition)
        {
            // Render_img ��ġ ������Ʈ
            UpdateRenderImagePosition(Camera.main.transform.position);
            // ���� ī�޶� ��ġ ����
            previousCameraPosition = Camera.main.transform.position;
        }
    }

    void MoveCameraWithMouse()
    {

        Vector3 mousePosition = Input.mousePosition;
        // ȭ�� �����ڸ��� ��� ���� ����

        if (mousePosition.x < edgeThreshold)
        {
            // ���콺�� ���� �����ڸ��� ����� ��
            transform.Translate(Vector3.left * mouseMoveSpeed * Time.deltaTime);
        }
        else if (mousePosition.x > Screen.width - edgeThreshold)
        {
            // ���콺�� ������ �����ڸ��� ����� ��
            transform.Translate(Vector3.right * mouseMoveSpeed * Time.deltaTime);
        }

        if (mousePosition.y < edgeThreshold)
        {
            // ���콺�� �Ʒ� �����ڸ��� ����� ��
            transform.Translate(Vector3.down * mouseMoveSpeed * Time.deltaTime);
        }
        else if (mousePosition.y > Screen.height - edgeThreshold)
        {
            // ���콺�� �� �����ڸ��� ����� ��
            transform.Translate(Vector3.up * mouseMoveSpeed * Time.deltaTime);
        }
        float clampedX = Mathf.Clamp(Camera.main.transform.position.x, CameraZone.min.x, CameraZone.max.x);
        float clampedZ = Mathf.Clamp(Camera.main.transform.position.z, CameraZone.min.z, CameraZone.max.z);

        // ī�޶� ��ġ�� ����
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

        // ī�޶� ��ġ�� ����
        Camera.main.transform.position = new Vector3(clampedX, cameraY, clampedZ);
    }

    private void UpdateRenderImagePosition(Vector3 worldPosition)
    {
        // �̴ϸ� ���� ���ο� ��ġ ���
        Vector2 newPosition = new Vector2((worldPosition.x - 90) / 300 * miniMapRect.sizeDelta.x,
            (worldPosition.z - 35) / 300 * miniMapRect.sizeDelta.y);

        // Render_img�� RectTransform�� ������Ʈ
        Render_img.rectTransform.anchoredPosition = newPosition;
        // �̹����� ��ġ ������Ʈ
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
