using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseControl : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private BuildManager bm;
    [SerializeField]public InfoConecttoUI infoUI;
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
            if (IsPointerOverUI()) return;
            Debug.Log("���Ͼȴ���");
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

        //�������ǿ� ���� ���콺��ġ��UI ���? �� ������ ������

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayer))
        {
            Debug.Log(hit);
            if (hit.transform.parent.GetComponent<Tower>() == null) return;
          
            Tower hitTower = hit.transform.parent.GetComponent<Tower>();

         //����Ŭ���� �������� ����Ÿ���� ��� �����ϴ¸޼ҵ�
            if (isCanDouble)
            {

                rts.DoubleClick(hitTower);
                infoUI.SetInfoPanel();
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
                //todo �ӽþ�..
                //StartCoroutine(test()); 
                rts.DeSelectAll();
            }
        }


        infoUI.SetInfoPanel();

    }
    
    private IEnumerator test()
    {

        yield return new WaitForSeconds(0.1f);
        rts.DeSelectAll();
    }
    private bool IsPointerOverUI()
    {
        // UI�� ���콺 �����Ͱ� ��ġ�ϴ��� ���θ� Ȯ��
        return EventSystem.current.IsPointerOverGameObject();
    }


}

