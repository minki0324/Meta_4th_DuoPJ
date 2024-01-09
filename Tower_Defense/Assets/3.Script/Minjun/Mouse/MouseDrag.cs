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
        //��� ������ ���� �迭�� �̿��� �Ʒ� ����ġ�� ������ �������� �˻��� �ƹ��� ���ٸ� �׶� Ÿ������
        if (dragRect.Contains(maincamera.WorldToScreenPoint(BuildManager.Instance.builder.transform.position))){
            BuildManager.Instance.builder.isSelectBuilder = true;
            rts.DeSelectAll();
            return;
        }

        foreach (Tower tower in BuildManager.Instance.AllTower)
        {
            if (rts.selectTowers.Count > 35) { return; }
            if(!GameManager.instance.CompareEnumWithTag(tower.gameObject.tag)) continue;
            //Ÿ�� ����̸����� ���ؼ� �ٸ��� �����ؾ���
            //�����غ��� Ÿ�� �޶� �巡�� ���ð�����
           
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
        //�巡�׹����� ��Ÿ���� Image UI ��ġ �߾����� ����
        dragRectangle.position = (start + end) * 0.5f;
        // Abs = ����.
        // Image ũ�� ���� 
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
