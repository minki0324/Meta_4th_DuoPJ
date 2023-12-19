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
        // ���� ����� 1.5 ������ ���ڷ� ��ǥ ����
        Vector3 snappedPos = SnapToNearestGrid(RealPosition, 1.5f);

        // ������ ��ǥ�� ����
        transform.position = snappedPos;
        //transform.position = RealPosition;
    }

    private Vector3 SnapToNearestGrid(Vector3 originalPos, float gridSize)
    {
        // ���� ��ǥ�� ���� ����� ���� ��ǥ�� ����
        float snappedX = Mathf.Round(originalPos.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(originalPos.y / gridSize) * gridSize;
        float snappedZ = Mathf.Round(originalPos.z / gridSize) * gridSize;

        return new Vector3(snappedX, snappedY, snappedZ);
    }
}
