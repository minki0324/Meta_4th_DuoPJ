using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCameraCon : MonoBehaviour
{
    public float mouseMoveSpeed = 35f;
    public float KeyMoveSpeed = 35f;
    private float edgeThreshold = 3f;
    [SerializeField] private BoxCollider CameraZoneCol;
    private Bounds CameraZone;
    private bool isScreenLock;
    private void Start()
    {
        CameraZone = CameraZoneCol.bounds;
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
        Camera.main.transform.position = new Vector3(clampedX, 30 ,clampedZ);
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
        Camera.main.transform.position = new Vector3(clampedX, 30, clampedZ);
    }
}
