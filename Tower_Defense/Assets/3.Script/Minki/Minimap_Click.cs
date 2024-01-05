using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Minimap_Click : MonoBehaviour, IPointerClickHandler
{
    public RectTransform miniMapRect; // 미니맵의 RectTransform
    public Camera miniMapCamera; // 미니맵 카메라
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
        // 미니맵 크기 대비 클릭 위치의 비율 계산
        float xRatio = localPoint.x / 380;
        float yRatio = localPoint.y / miniMapSize.y;

        // 월드 좌표의 중심을 기준으로 클릭된 위치 계산
        float worldX = 90 + (xRatio) * 300; // 300은 월드의 가로 길이
        float worldZ = 35 + (yRatio) * 300; // 300은 월드의 세로 길이

        return new Vector3(worldX, 30, worldZ); // Y 축 좌표는 30으로 고정
    }

    private void MoveMainCamera(Vector3 worldPosition)
    {
        // 카메라 이동 범위 제한
        float minX = 110 - 110; // 중앙 좌표에서 x축 최소
        float maxX = 110 + 110; // 중앙 좌표에서 x축 최대
        float minZ = 35 - 50;  // 중앙 좌표에서 z축 최소
        float maxZ = 35 + 40;  // 중앙 좌표에서 z축 최대

        // 카메라 위치 제한
        float clampedX = Mathf.Clamp(worldPosition.x, minX, maxX);
        float clampedZ = Mathf.Clamp(worldPosition.z, minZ, maxZ);

        Camera.main.transform.position = new Vector3(clampedX, Camera.main.transform.position.y, clampedZ);

        // Render_img 위치 업데이트
        UpdateRenderImagePosition(new Vector2(clampedX, clampedZ));
    }

    private void UpdateRenderImagePosition(Vector2 worldPosition)
    {
        // 미니맵 상의 새로운 위치 계산
        Vector2 newPosition = new Vector2(
            (worldPosition.x - 90) / 300 * miniMapRect.sizeDelta.x,
            (worldPosition.y - 35) / 300 * miniMapRect.sizeDelta.y
        );

        // Render_img의 RectTransform을 업데이트
        Render_img.rectTransform.anchoredPosition = newPosition;
    }
}
