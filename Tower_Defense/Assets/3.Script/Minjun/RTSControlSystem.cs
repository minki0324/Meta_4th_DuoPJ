using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSControlSystem : MonoBehaviour
{
    public List<Tower> selectTowers = new List<Tower>();


    //������ ���É����� ȣ���ϴ� �޼ҵ�
    //�̹��� true , ����Ʈ �߰�

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
            for (int i = 0; i < BuildManager.Instance.AllTower.Count; i++)
            {
                
                //���� : ������(�̱���) , ���� ������ Ÿ�� (�̸���)
                if (hittower.name == BuildManager.Instance.AllTower[i].name && !selectTowers.Contains(BuildManager.Instance.AllTower[i]))
                {

                    Renderer renderer = BuildManager.Instance.AllTower[i].transform.GetChild(0).GetComponent<Renderer>();

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
                            SelectUnit(BuildManager.Instance.AllTower[i]);
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



    private bool IsObjectVisible(Renderer renderer)
    {
        // ī�޶��� �þ߿� �ִ��� ���θ� Ȯ��
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Bounds bounds = renderer.bounds;

        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
}
