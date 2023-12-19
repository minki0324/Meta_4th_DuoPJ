using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildAreaPrents : MonoBehaviour
{
    [SerializeField] private Bounds box;
    private Vector3 RealPosition;

   
    private void Update()
    {
        MouseMove();
    }
    private void MouseMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {

            RealPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        }
        // 가장 가까운 1.5 간격의 격자로 좌표 보정
        Vector3 snappedPos = SnapToNearestGrid(RealPosition, 1.5f);

        // 보정된 좌표를 적용
        transform.position = snappedPos;
        //transform.position = RealPosition;
    }

    private Vector3 SnapToNearestGrid(Vector3 originalPos, float gridSize)
    {
        // 기존 좌표를 가장 가까운 격자 좌표로 보정
        float snappedX = Mathf.Round(originalPos.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(originalPos.y / gridSize) * gridSize;
        float snappedZ = Mathf.Round(originalPos.z / gridSize) * gridSize;

        return new Vector3(snappedX, snappedY, snappedZ);
    }
}
