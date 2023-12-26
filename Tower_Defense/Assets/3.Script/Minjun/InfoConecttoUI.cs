using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoConecttoUI : MonoBehaviour
{
    [SerializeField] public RTSControlSystem rts;
    [SerializeField] private GameObject panel; // 단일유닛 info 패널 /해당 유닛의 정보들 모두 입력
    [SerializeField] private GameObject gridPanel; //다중 유닛 sprite 패널 / 해당유닛의 sprite ,HP 가져와서 HP 비례로 녹색 -> 빨간색 으로 변하게함 
    [SerializeField] private Text MaxHP_CurrentHP;
    [SerializeField] private Text Lvl_Speed;
    [SerializeField] private Text Atk_Range_AS;
    [SerializeField] private Image unitImage;
    [SerializeField] private GameObject[] unitInfoButton; // 인덱스에 맞는 유닛의 단일유닛info패널로 바꿈.

    private void Update()
    {

      
    }
    public void SetInfoPanel()
    {
        if (rts.selectTowers.Count == 0)
        {
            panel.SetActive(false);
            ButtonActiveReset();
        }

        if (rts.selectTowers.Count == 1)
        {
            //단일 선택했을시 그 오브젝트의 정보를 가져옵니다.
            //체력, 공격력 
            SingleInfoSetting(); //단일정보창 띄우기
        }
        else if (rts.selectTowers.Count > 1)
        {
            
            
            MultiInfoSetting(); //다중유닛 정보창 띄우기
        }

        //왜있는지 모르겠음 일단 보류
        //else
        //{
        //    panel.SetActive(false);
        //}
    }
    private void ButtonActiveReset()
    {
        for (int i = 0; i < unitInfoButton.Length; i++)
        {
            if (!unitInfoButton[i].gameObject.activeSelf) break; //비활성화 된 버튼이 있으면 그뒤에 버튼은 다 꺼져있는거기 때문에 for문나감.
            unitInfoButton[i].gameObject.SetActive(false);
        }
    }
    public void SingleInfoSetting()
    {
        ButtonActiveReset();
        panel.SetActive(true);
        unitImage.gameObject.GetComponent<unitSpriteController>().myObject = rts.selectTowers[0];
        //rts.selectTowers[0].maxHP;
        Lvl_Speed.text = string.Format("{0} \n {1}", rts.selectTowers[0].level, rts.selectTowers[0].Speed);
        Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", rts.selectTowers[0].damage, rts.selectTowers[0].range, rts.selectTowers[0].atkSpeed);
        MaxHP_CurrentHP.text = string.Format("{0} / {1}", rts.selectTowers[0].maxHP, rts.selectTowers[0].currentHP);
        unitImage.sprite = rts.selectTowers[0].unitSprite;
    }
    private void MultiInfoSetting()
    {

        panel.SetActive(false);
        gridPanel.SetActive(true);
        ButtonActiveReset(); //기존에 켜져있던 버튼들 다시 비활성화
        for (int i = 0; i < rts.selectTowers.Count; i++)
        {
            unitInfoButton[i].gameObject.SetActive(true);     //버튼활성화
            unitInfoButton[i].GetComponent<unitSpriteController>().myObject = rts.selectTowers[i];
            unitInfoButton[i].GetComponent<Image>().sprite = rts.selectTowers[i].unitSprite; // 버튼의 sprite를 해당유닛의 sprite로 바꿔줌
        }
    }
    public void SpriteClick()
    {
        Debug.Log("불리긴하냐");
        //리스트 초기화하고 클릭한버튼에 해당하는 오브젝트 다시 list담기
        /*InfoCo*/
        rts.DeSelectAll();
        rts.selectTowers.Add(GetComponent<unitSpriteController>().myObject);
        SingleInfoSetting();

    }

}
