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

        //여기조건에 현재 마우스위치가UI 라면? 을 넣을수 있을까

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayer))
        {
            Debug.Log(hit);
            //비교목록
            // 나의팀번호 == 오브젝트의 태그 ( ex. 1P) 
            // Tower가 아니면 리턴 (빌더꺼는 따로있음)
            if (!GameManager.instance.CompareEnumWithTag(hit.collider.gameObject.transform.root.tag)) return;
            if (hit.transform.root.GetComponent<Tower>() == null) return;
          
            Tower hitTower = hit.transform.root.GetComponent<Tower>();

         //더블클릭시 같은팀의 같은타워를 모두 선택하는메소드
            if (isCanDouble)
            {

                rts.DoubleClick(hitTower);
                infoUI.SetInfoPanel();
                return;
            }

            //Shift = 다중선택기능 안누르면 선택됬던애들 모두초기화하고 새로운애 선택
            if (Input.GetKey(KeyCode.LeftShift)){
                rts.ShiftClickSelectUnit(hitTower);
            }
            else // 단일선택시 나오는 메소드
            {
                rts.ClickSelectUnit(hitTower);
                rts.SetAttackRange(hitTower);
                //단일선택시  0.2초 동안 더블클릭할수있는 isCanDouble true 할당
                StartCoroutine(DoubleClickCool());
            }
        }
        else
        {
            //땅클릭시 Shift 안누르면 초기화
            if (!Input.GetKey(KeyCode.LeftShift))   
            {
                //todo 임시야..
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
            //todo 나중에 적팀꺼 클릭했을때 단일UI 라도 보이게 하기
                builder = hit.transform.root.GetComponent<BuilderController>();
            
            if (builder == null) return;
            //마커 띄우기 --완료
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
        // UI에 마우스 포인터가 위치하는지 여부를 확인
        return EventSystem.current.IsPointerOverGameObject();
    }

}

