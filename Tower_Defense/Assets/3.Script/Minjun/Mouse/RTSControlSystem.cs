using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSControlSystem : MonoBehaviour
{
    public List<Tower> selectTowers = new List<Tower>();
    public static RTSControlSystem Instance ;

    //유닛이 선택됬을때 호출하는 메소드
    //이미지 true , 리스트 추가

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
        if (hittower == selectTowers[0]) //더블클릭했을때 기존클릭한 타워와 같다면.
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
                        // 현재 오브젝트가 카메라의 시야에 보이는지 여부를 체크
                        bool isVisible = IsObjectVisible(renderer);

                        // 보이는지 여부에 따라 처리
                        if (isVisible)
                        {

                            Debug.Log("보임");
                            // 여기에 보이는 경우의 로직 추가
                            if (selectTowers.Count > 35)
                            {
                                return;
                            }
                            SelectUnit(currenttower);
                        }
                        else
                        {
                            Debug.Log("안보임");
                            // 여기에 보이지 않는 경우의 로직 추가
                            continue;
                        }
                    }
                    else
                    {
                        Debug.Log("없음");

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
            //UI 세팅 다시해줘야함
           
        }
    }

    private bool IsObjectVisible(Renderer renderer)
    {
        // 카메라의 시야에 있는지 여부를 확인
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Bounds bounds = renderer.bounds;

        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    
}
