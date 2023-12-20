using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSControlSystem : MonoBehaviour
{
    private List<Tower> selectTowers = new List<Tower>();


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
        newunit.Selectunit();
        selectTowers.Add(newunit);
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
        if (selectTowers.Contains(newunit))
        {
            DeSelectUnit(newunit);
        }
        else
        {
            SelectUnit(newunit);
        }
    }
}
