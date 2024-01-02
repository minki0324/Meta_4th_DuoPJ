using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
public class MouseControl : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask BuilderLayer;
    [SerializeField] private BuildManager bm;
    [SerializeField]public InfoConecttoUI infoUI;
    private BuilderController builder;
    private NetworkIdentity myIdentity;
    private Camera maincamera;
    private RTSControlSystem rts;
    private bool isCanDouble;
    private bool isBuilder;


    private void Awake()
    {
        maincamera = Camera.main;
        rts = GetComponent<RTSControlSystem>();
        myIdentity = GetComponent<NetworkIdentity>();
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
            GetTowerInfo();
            GetBuilderInfo();
        }
       

    }

    private IEnumerator DoubleClickCool()
    {

        isCanDouble = true;
       
        yield return new WaitForSeconds(0.2f);

        isCanDouble = false;
    }

    private void GetTowerInfo()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //�������ǿ� ���� ���콺��ġ��UI ���? �� ������ ������

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayer))
        {
            Debug.Log(hit);
            //�񱳸��
            // ��������ȣ == ������Ʈ�� �±� ( ex. 1P) 
            // Tower�� �ƴϸ� ���� (�������� ��������)
            if (!GameManager.instance.CompareEnumWithTag(hit.collider.gameObject.transform.root.tag)) return;
            if (hit.transform.root.GetComponent<Tower>() == null) return;
          
            Tower hitTower = hit.transform.root.GetComponent<Tower>();

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
            else // ���ϼ��ý� ������ �޼ҵ�
            {
                rts.ClickSelectUnit(hitTower);
                rts.SetAttackRange(hitTower);
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
    private void GetBuilderInfo()
    {


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, BuilderLayer))
        {
            if (!GameManager.instance.CompareEnumWithTag(hit.collider.gameObject.tag)) return;
            //todo ���߿� ������ Ŭ�������� ����UI �� ���̰� �ϱ�
                builder = hit.transform.root.GetComponent<BuilderController>();
            
            if (builder == null) return;
            //��Ŀ ���� --�Ϸ�
            rts.DeSelectAll();
            builder.isSelectBuilder = true;

        }
        else
        {
            if (builder != null)
            {
                builder.isSelectBuilder = false;
            }
        }
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

