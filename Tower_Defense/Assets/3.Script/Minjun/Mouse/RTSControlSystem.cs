using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSControlSystem : MonoBehaviour
{
    public List<Tower> selectTowers = new List<Tower>();
    public static RTSControlSystem Instance ;

    //������ ���É����� ȣ���ϴ� �޼ҵ�
    //�̹��� true , ����Ʈ �߰�

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void DragSelectUnit(Tower newunit)
    {
        if (!selectTowers.Contains(newunit))
        {
            SelectUnit(newunit);
        }
    }
    public void SelectUnit(Tower newunit)
    {
        if (selectTowers.Count > 35)
        {

            return;
        }
        newunit.Selectunit();
        selectTowers.Add(newunit);
    }
    public void SetAttackRange(Tower newunit)
    {
        newunit.AttackRange.SetActive(true);
    }
    //������ ������� ������ ȣ���ϴ� �޼ҵ�
    //�̹��� false , ����Ʈ ����
    public void DeSelectUnit(Tower newunit)
    {
        newunit.DeSelectunit();
        selectTowers.Remove(newunit);
    }

    //���É�� ������� ��� �ʱ�ȭ �޼ҵ�
    public void DeSelectAll()
    {
        for (int i = 0; i < selectTowers.Count; i++)
        {
            selectTowers[i].DeSelectunit();
        }
        selectTowers.Clear();
    }

    //���콺 Ŭ������ �����������Ҷ� ȣ��
    public void ClickSelectUnit(Tower newunit)
    {
        //������ ���õǾ� �ִ� ���� ��� ����
        DeSelectAll();
        SelectUnit(newunit);
    }

    public void ShiftClickSelectUnit(Tower newunit)
    {
        if (selectTowers.Count == 1)
        {
            selectTowers[0].AttackRange.SetActive(false);
        }
        if (selectTowers.Contains(newunit))
        {
            DeSelectUnit(newunit);
        }
        else
        {
            if (selectTowers.Count > 35)
            {
                return;
            }
            SelectUnit(newunit);
        }
    }
    public void DoubleClick(Tower hittower)
    {
        if (hittower == selectTowers[0]) //����Ŭ�������� ����Ŭ���� Ÿ���� ���ٸ�.
        {
            DeSelectAll();
            SelectUnit(hittower);
            foreach (Tower currenttower in BuildManager.Instance.AllTower)
            {
                if (hittower.head.name == currenttower.head.name && !selectTowers.Contains(currenttower)
                  && GameManager.instance.CompareEnumWithTag(currenttower.gameObject.tag))
                {
                    Renderer renderer = currenttower.towerbase.GetComponent<Renderer>();

                    if (renderer != null)
                    {
                        // ���� ������Ʈ�� ī�޶��� �þ߿� ���̴��� ���θ� üũ
                        bool isVisible = IsObjectVisible(renderer);

                        // ���̴��� ���ο� ���� ó��
                        if (isVisible)
                        {

                            Debug.Log("����");
                            // ���⿡ ���̴� ����� ���� �߰�
                            if (selectTowers.Count > 35)
                            {
                                return;
                            }
                            SelectUnit(currenttower);
                        }
                        else
                        {
                            Debug.Log("�Ⱥ���");
                            // ���⿡ ������ �ʴ� ����� ���� �߰�
                            continue;
                        }
                    }
                    else
                    {
                        Debug.Log("����");

                    }



                }
            }
        }
    }

    public void Destroytower(Tower tower)
    {
        if (BuildManager.Instance.AllTower.Contains(tower))
        {
            BuildManager.Instance.AllTower.Remove(tower);
        }
        if (selectTowers.Contains(tower))
        {
            selectTowers.Remove(tower);
            //UI ���� �ٽ��������
           
        }
    }

    private bool IsObjectVisible(Renderer renderer)
    {
        // ī�޶��� �þ߿� �ִ��� ���θ� Ȯ��
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Bounds bounds = renderer.bounds;

        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    
}
