using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSControlSystem : MonoBehaviour
{
    private List<Tower> selectTowers = new List<Tower>();


    //유닛이 선택됬을때 호출하는 메소드
    //이미지 true , 리스트 추가

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
    //유닛이 선택취소 됬을때 호출하는 메소드
    //이미지 false , 리스트 삭제
    public void DeSelectUnit(Tower newunit)
    {
        newunit.DeSelectunit();
        selectTowers.Remove(newunit);
    }

    //선택됬던 모든유닛 모두 초기화 메소드
    public void DeSelectAll()
    {
        for (int i = 0; i < selectTowers.Count; i++)
        {
            selectTowers[i].DeSelectunit();
        }
        selectTowers.Clear();
    }

    //마우스 클릭으로 유닛을선택할때 호출
    public void ClickSelectUnit(Tower newunit)
    {
        //기존에 선택되어 있던 유닛 모두 해제
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
