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
    [SerializeField] private LayerMask targetLayer; //���� , Ÿ�� , ����


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
        //�Ǽ����϶� ���� �ȵ� ����
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
            Debug.Log("������");
        }
        infoUI.isMonsterClick = false;
        //��Ŭ���� Shift �ȴ����� �ʱ�ȭ
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

        //�񱳸��
        // ��������ȣ == ������Ʈ�� �±� ( ex. 1P) 
        // Tower�� �ƴϸ� ���� (�������� ��������)
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
        //����Ŭ���� �������� ����Ÿ���� ��� �����ϴ¸޼ҵ�
        if (isCanDouble)
        {

            rts.DoubleClick(hitTower);
            infoUI.SetInfoPanel();
            return;
        }

        //Shift = ���߼��ñ�� �ȴ����� ���É���ֵ� ����ʱ�ȭ�ϰ� ���ο�� ����
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rts.ShiftClickSelectUnit(hitTower);
        }
        else // ���ϼ��ý� ������ �޼ҵ�
        {
            if (BuildManager.Instance.builder.isCanDestroyTower)
            {
                BuildManager.Instance.builder.DestroyOrder(hitTower.gameObject);
                BuildManager.Instance.builder.isSelectBuilder = true;
                return;
            }

            rts.ClickSelectUnit(hitTower);
            rts.SetAttackRange(hitTower);
            //���ϼ��ý�  0.2�� ���� ����Ŭ���Ҽ��ִ� isCanDouble true �Ҵ�
            StartCoroutine(DoubleClickCool());
        }


    }

    public void GetBuilderInfo(RaycastHit hit)
    {
        if (!GameManager.instance.CompareEnumWithTag(hit.collider.gameObject.tag)) return;
        //todo ���߿� ������ Ŭ�������� ����UI �� ���̰� �ϱ�
        //builder = hit.transform.root.GetComponent<BuilderController>();

        //��Ŀ ���� --�Ϸ�
        rts.DeSelectAll();
        BuildManager.Instance.builder.isSelectBuilder = true;



    }

    public bool IsPointerOverUI()
    {
        // UI�� ���콺 �����Ͱ� ��ġ�ϴ��� ���θ� Ȯ��
        return EventSystem.current.IsPointerOverGameObject();
    }


}

