using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
public class MouseControl : MonoBehaviour
{
    [SerializeField] private BuildManager bm;
    [SerializeField] public InfoConecttoUI infoUI;
    private Camera maincamera;
    private RTSControlSystem rts;
    private bool isCanDouble;
    Monster_Control monster;
    [SerializeField] private LayerMask targetLayer; //몬스터 , 타워 , 빌더


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
        if (infoUI.isMonsterClick)
        {
            infoUI.MonsterInfoSetting(monster);
            if (monster.isDie)
            {
                infoUI.isMonsterClick = false;
                monster.DeSelectunit();
            }
        }
        else
        {
            if (monster != null) { 
            monster.DeSelectunit();
            
            }
        }
        //건설중일땐 선택 안돼 리턴
        if (BuildManager.Instance.gameObject == null)
        {
            Debug.Log("nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn");
            bm.gameObject.SetActive(true);
            return;
        }
        if (BuildManager.Instance.isBuilding) return;
        if (IsPointerOverUI()) return;
        onClickObject();




    }

    private void onClickObject()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit , targetLayer))
            {
                LayerMask hitLayer = hit.collider.gameObject.layer;
                string layerName = LayerMask.LayerToName(hitLayer);
                if(BuildManager.Instance.builder != null)
                {

                BuildManager.Instance.builder.isSelectBuilder = false;
                }
                infoUI.isMonsterClick = false;
                switch (layerName)
                {
                    case "Monster":
                        infoUI.isMonsterClick = true;
                        GetMonsterInfo(hit);
                        return;
                    case "Builder":
                        GetBuilderInfo(hit);
                        break;
                    case "Tower":
                        GetTowerInfo(hit);
                        break;
                    default: 
                        ResetInfo();
                        break;
                }
            }
            else
            {
                ResetInfo();
            }
            InfoConecttoUI.Instance.type = InfoConecttoUI.Type.Empty;
            infoUI.SetInfoPanel();
        }
    }

    private void GetMonsterInfo(RaycastHit hit)
    {
        if (monster != null)
        {
            monster.DeSelectunit();
        }
        monster = hit.transform.GetComponent<Monster_Control>();

        monster.Selectunit();
        infoUI.MonsterInfoSetting(monster);

    }

    private void ResetInfo()
    {
        if(BuildManager.Instance.builder != null) { 
        BuildManager.Instance.builder.isSelectBuilder = false;
        }
        else
        {
            Debug.Log("빌더널");
        }
        infoUI.isMonsterClick = false;
        //땅클릭시 Shift 안누르면 초기화
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            rts.DeSelectAll();
        }
    }

    private IEnumerator DoubleClickCool()
    {

        isCanDouble = true;

        yield return new WaitForSeconds(0.2f);

        isCanDouble = false;
    }

    public void GetTowerInfo(RaycastHit hit)
    {

        //비교목록
        // 나의팀번호 == 오브젝트의 태그 ( ex. 1P) 
        // Tower가 아니면 리턴 (빌더꺼는 따로있음)
        if (hit.transform.root.GetComponent<Tower>() == null) return;
        Tower hitTower = hit.transform.root.GetComponent<Tower>();
        if (GameManager.instance.CompareEnumWithTag(hit.collider.gameObject.transform.root.tag))
        {
            TeamActivity(hitTower);
        }
        else
        {
            EnemyActivity(hitTower);
        }

        

    }

    private void EnemyActivity(Tower hitTower)
    {
        rts.ClickSelectUnit(hitTower);
    }

    private void TeamActivity(Tower hitTower)
    {
        //더블클릭시 같은팀의 같은타워를 모두 선택하는메소드
        if (isCanDouble)
        {

            rts.DoubleClick(hitTower);
            infoUI.SetInfoPanel();
            return;
        }

        //Shift = 다중선택기능 안누르면 선택됬던애들 모두초기화하고 새로운애 선택
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rts.ShiftClickSelectUnit(hitTower);
        }
        else // 단일선택시 나오는 메소드
        {
            if (BuildManager.Instance.builder.isCanDestroyTower)
            {
                BuildManager.Instance.builder.DestroyOrder(hitTower.gameObject);
                BuildManager.Instance.builder.isSelectBuilder = true;
                return;
            }

            rts.ClickSelectUnit(hitTower);
            rts.SetAttackRange(hitTower);
            //단일선택시  0.2초 동안 더블클릭할수있는 isCanDouble true 할당
            StartCoroutine(DoubleClickCool());
        }


    }

    public void GetBuilderInfo(RaycastHit hit)
    {
        if (!GameManager.instance.CompareEnumWithTag(hit.collider.gameObject.tag)) return;
        //todo 나중에 적팀꺼 클릭했을때 단일UI 라도 보이게 하기
        //builder = hit.transform.root.GetComponent<BuilderController>();

        //마커 띄우기 --완료
        rts.DeSelectAll();
        BuildManager.Instance.builder.isSelectBuilder = true;



    }

    public bool IsPointerOverUI()
    {
        // UI에 마우스 포인터가 위치하는지 여부를 확인
        return EventSystem.current.IsPointerOverGameObject();
    }


}

