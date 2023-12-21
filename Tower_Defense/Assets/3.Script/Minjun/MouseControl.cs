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


        //ȭ��ȿ� ���콺�� ���δ� �޼ҵ� ���߿� �ּ�Ǯ��
        //Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        
        //�Ǽ����϶� ���� �ȵ� ����
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

         //����Ŭ���� �������� ����Ÿ���� ��� �����ϴ¸޼ҵ�
            if (isCanDouble)
            {

                rts.DoubleClick(hitTower);
                return;
            }

            //Shift = ���߼��ñ�� �ȴ����� ���É���ֵ� ����ʱ�ȭ�ϰ� ���ο�� ����
            if (Input.GetKey(KeyCode.LeftShift)){
                rts.ShiftClickSelectUnit(hitTower);
            }
            else
            {
                rts.ClickSelectUnit(hitTower);
                //���ϼ��ý�  0.2�� ���� ����Ŭ���Ҽ��ִ� isCanDouble true �Ҵ�
                StartCoroutine(DoubleClickCool());
            }
        }
        else
        {
            //��Ŭ���� Shift �ȴ����� �ʱ�ȭ
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                rts.DeSelectAll();
            }
        }
    }
    


   
}

