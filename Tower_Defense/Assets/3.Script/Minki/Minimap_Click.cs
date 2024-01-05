using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Minimap_Click : MonoBehaviour, IPointerClickHandler
{
    public RectTransform miniMapRect; // �̴ϸ��� RectTransform
    public Camera miniMapCamera; // �̴ϸ� ī�޶�
    public Image Render_img;

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Debug.Log(localPoint);
            Vector2 miniMapSize = miniMapRect.sizeDelta;
            Vector3 worldPosition = ConvertMiniMapPositionToWorld(localPoint, miniMapSize);
            MoveMainCamera(worldPosition);
        }
    }

    private Vector3 ConvertMiniMapPositionToWorld(Vector2 localPoint, Vector2 miniMapSize)
    {
        // �̴ϸ� ũ�� ��� Ŭ�� ��ġ�� ���� ���
        float xRatio = localPoint.x / 380;
        float yRatio = localPoint.y / miniMapSize.y;

        // ���� ��ǥ�� �߽��� �������� Ŭ���� ��ġ ���
        float worldX = 90 + (xRatio) * 300; // 300�� ������ ���� ����
        float worldZ = 35 + (yRatio) * 300; // 300�� ������ ���� ����

        return new Vector3(worldX, 30, worldZ); // Y �� ��ǥ�� 30���� ����
    }

    private void MoveMainCamera(Vector3 worldPosition)
    {
        // ī�޶� �̵� ���� ����
        float minX = 110 - 110; // �߾� ��ǥ���� x�� �ּ�
        float maxX = 110 + 110; // �߾� ��ǥ���� x�� �ִ�
        float minZ = 35 - 50;  // �߾� ��ǥ���� z�� �ּ�
        float maxZ = 35 + 40;  // �߾� ��ǥ���� z�� �ִ�

        // ī�޶� ��ġ ����
        float clampedX = Mathf.Clamp(worldPosition.x, minX, maxX);
        float clampedZ = Mathf.Clamp(worldPosition.z, minZ, maxZ);

        Camera.main.transform.position = new Vector3(clampedX, Camera.main.transform.position.y, clampedZ);

        // Render_img ��ġ ������Ʈ
        UpdateRenderImagePosition(new Vector2(clampedX, clampedZ));
    }

    private void UpdateRenderImagePosition(Vector2 worldPosition)
    {
        // �̴ϸ� ���� ���ο� ��ġ ���
        Vector2 newPosition = new Vector2(
            (worldPosition.x - 90) / 300 * miniMapRect.sizeDelta.x,
            (worldPosition.y - 35) / 300 * miniMapRect.sizeDelta.y
        );

        // Render_img�� RectTransform�� ������Ʈ
        Render_img.rectTransform.anchoredPosition = newPosition;
    }
}
