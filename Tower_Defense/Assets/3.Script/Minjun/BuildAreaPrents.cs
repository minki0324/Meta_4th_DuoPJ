using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildAreaPrents : MonoBehaviour
{
    public enum Type
    {
        TwoTwo,
        ThreeTwo,
        ThreeThree
        
    }
    [SerializeField] private Bounds box;
    private Vector3 RealPosition;
    private Type type;
    private void Start()
    {
        SetType();

       
    }

   

    private void Update()
    {
        MouseMove();
    }
    private void SetType()
    {
        if (transform.childCount == 4)
        {
            type = Type.TwoTwo;
        }
        else if (transform.childCount == 6)
        {
            type = Type.ThreeTwo;
        }
        else if (transform.childCount == 9)
        {
            type = Type.ThreeThree;
        }
    }
    private void MouseMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity ,1<< LayerMask.NameToLayer("Ground")))
        {   
            RealPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        }
        // 가장 가까운 1.5 간격의 격자로 좌표 보정
        Vector3 snappedPos = SnapToNearestGrid(RealPosition, 1.5f , 1.5f);

        // 보정된 좌표를 적용
        transform.position = snappedPos;

        //transform.position = RealPosition;
    }

    private Vector3 SnapToNearestGrid(Vector3 originalPos, float gridSize , float gridSize1)
    {
        // 기존 좌표를 가장 가까운 격자 좌표로 보정
        float snappedX = Mathf.Round(originalPos.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(originalPos.y / gridSize) * gridSize;
        float snappedZ = Mathf.Round(originalPos.z / gridSize1) * gridSize1;
        Vector3 result = Vector3.zero;
        //타입에따라 그리드 위치조정
        switch (type)
        {
            case Type.TwoTwo:
                result = new Vector3(snappedX , snappedY, snappedZ);
                break;
            case Type.ThreeTwo:
                result = new Vector3(snappedX - 0.75f, snappedY, snappedZ);
                break;
            case Type.ThreeThree:
                result = new Vector3(snappedX - 0.75f, snappedY, snappedZ - 0.75f);
                break;
        }

        return result;
    }
}
