using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private BuildManager bm;
    private Camera maincamera;
    private RTSControlSystem rts;
    private bool isCanDouble;


    private void Awake()
    {
        maincamera = Camera.main;
        rts = GetComponent<RTSControlSystem>();
    }
    private void Start()
    {


        //화면안에 마우스를 가두는 메소드 나중에 주석풀기
        //Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        
        //건설중일땐 선택 안돼 리턴
        if (BuildManager.Instance.gameObject == null)
        {
            Debug.Log("nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn");
            bm.gameObject.SetActive(true);
            return;
        }
        if (BuildManager.Instance.isBuilding) return;

        if (Input.GetMouseButtonDown(0))
        {
           
            GetTargetInfo();
          
        }
       
    }

    private IEnumerator DoubleClickCool()
    {

        isCanDouble = true;
       
        yield return new WaitForSeconds(0.2f);

        isCanDouble = false;
    }

    private void GetTargetInfo()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayer))
        {
            if (hit.transform.parent.GetComponent<Tower>() == null) return;

            Tower hitTower = hit.transform.parent.GetComponent<Tower>();

         //더블클릭시 같은팀의 같은타워를 모두 선택하는메소드
            if (isCanDouble)
            {

                rts.DoubleClick(hitTower);
                return;
            }

            //Shift = 다중선택기능 안누르면 선택됬던애들 모두초기화하고 새로운애 선택
            if (Input.GetKey(KeyCode.LeftShift)){
                rts.ShiftClickSelectUnit(hitTower);
            }
            else
            {
                rts.ClickSelectUnit(hitTower);
                //단일선택시  0.2초 동안 더블클릭할수있는 isCanDouble true 할당
                StartCoroutine(DoubleClickCool());
            }
        }
        else
        {
            //땅클릭시 Shift 안누르면 초기화
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                rts.DeSelectAll();
            }
        }
    }
    


   
}

