using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag : MonoBehaviour
{
    [SerializeField] RectTransform dragRectangle;

    private Rect dragRect;
    private Vector2 start = Vector2.zero;
    private Vector2 end = Vector2.zero;

    private Camera maincamera;
    private RTSControlSystem rts;

    private void Awake()
    {
        maincamera = Camera.main;
        rts = GetComponent<RTSControlSystem>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            start = Input.mousePosition;
            dragRect = new Rect();
        }
        if (Input.GetMouseButton(0))
        {
            end = Input.mousePosition;

            DrawDragRectangle();
        }

        if (Input.GetMouseButtonUp(0)){

            CalculateDragRact();
            SelectUnits();

            start = end = Vector2.zero;
            DrawDragRectangle();
        }

    }

    private void SelectUnits()
    {
        //foreach (var item in collection)
        //{

        //    if(dragRect.Contains(maincamera.WorldToScreenPoint()))
        //}
    }

    private void CalculateDragRact()
    {
       if(Input.mousePosition.x < start.x)
        {
            dragRect.xMin = Input.mousePosition.x;
            dragRect.xMax = start.x;
        }
        else
        {
            dragRect.xMin = start.x;
            dragRect.xMax = Input.mousePosition.x;
        }
        if (Input.mousePosition.y < start.y)
        {
            dragRect.yMin = Input.mousePosition.y;
            dragRect.yMax = start.y;
        }
        else
        {
            dragRect.yMin = start.y;
            dragRect.yMax = Input.mousePosition.y;
        }
    }

    private void DrawDragRectangle()
    {
        //드래그범위를 나타내는 Image UI 위치 중앙으로 설정
        dragRectangle.position = (start + end) * 0.5f;
        // Abs = 절댓값.
        // Image 크기 설정 
        dragRectangle.sizeDelta = new Vector2(Mathf.Abs(start.x-+ end.x), Mathf.Abs(start.y - end.y));
    }
}
