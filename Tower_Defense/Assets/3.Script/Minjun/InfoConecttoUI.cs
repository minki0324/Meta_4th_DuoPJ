using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoConecttoUI : MonoBehaviour
{
    [SerializeField] private RTSControlSystem rts;
    [SerializeField] private GameObject panel;
    [SerializeField] private Text MaxHP_CurrentHP;
    [SerializeField] private Text Lvl_Speed;
    [SerializeField] private Text Atk_Range_AS;

    private void Update()
    {

        if(rts.selectTowers.Count == 0)
        {
            panel.SetActive(false);
        }

        if(rts.selectTowers.Count == 1)
        {
            //단일 선택했을시 그 오브젝트의 정보를 가져옵니다.
            //체력, 공격력 
            panel.SetActive(true);
            //rts.selectTowers[0].maxHP;
            Lvl_Speed.text = string.Format("{0} \n {1}" , rts.selectTowers[0].level , rts.selectTowers[0].Speed);
            Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", rts.selectTowers[0].damage, rts.selectTowers[0].range, rts.selectTowers[0].atkSpeed);
            MaxHP_CurrentHP.text = string.Format("{0} / {1}", rts.selectTowers[0].maxHP, rts.selectTowers[0].currentHP);
        }
        else
        {
            panel.SetActive(false);
        }
    }

    
}
