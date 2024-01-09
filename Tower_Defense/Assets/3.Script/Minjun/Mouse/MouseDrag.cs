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
    private MouseControl mouseCon;
    private bool isClick;
    private void Awake()
    {
        maincamera = Camera.main;
        rts = GetComponent<RTSControlSystem>();
        mouseCon = GetComponent<MouseControl>();
        DrawDragRectangle();
    }

    private void Update()
    {

           
        DragStart();
       

    }

    private void SelectUnits()
    {
        //모든 빌더를 담은 배열을 이용해 아래 포이치문 돌려서 빌더부터 검사후 아무도 없다면 그때 타워부터
        if (dragRect.Contains(maincamera.WorldToScreenPoint(BuildManager.Instance.builder.transform.position))){
            BuildManager.Instance.builder.isSelectBuilder = true;
            rts.DeSelectAll();
            return;
        }

        foreach (Tower tower in BuildManager.Instance.AllTower)
        {
            if (rts.selectTowers.Count > 35) { return; }
            if(!GameManager.instance.CompareEnumWithTag(tower.gameObject.tag)) continue;
            //타워 헤드이름으로 비교해서 다르면 리턴해야함
            //생각해보니 타워 달라도 드래그 선택가능함
           
            if (dragRect.Contains(maincamera.WorldToScreenPoint(tower.transform.position)))
            {
                rts.DragSelectUnit(tower);
            }
        }
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
        dragRectangle.sizeDelta = new Vector2(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
    }
    private void DragStart()
    {
    

        if (Input.GetMouseButtonDown(0))
        {
            if (mouseCon.IsPointerOverUI()) return;
            start = Input.mousePosition;
            dragRect = new Rect();
            isClick = true;
        }
        if (Input.GetMouseButton(0) && isClick)
        {
            end = Input.mousePosition;

            DrawDragRectangle();
        }

        if (Input.GetMouseButtonUp(0) && isClick)
        {
            CalculateDragRact();
            SelectUnits();
            mouseCon.infoUI.SetInfoPanel();

            start = end = Vector2.zero;
            DrawDragRectangle();
            isClick = false;
        }
    }
}
