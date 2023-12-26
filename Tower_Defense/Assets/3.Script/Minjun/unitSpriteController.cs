using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class unitSpriteController : MonoBehaviour
{
    public Tower myObject;
    public Image spriteColor;
    public Image AngleColor;
    public Color CurrentColor;
    float greenValue;
    float redValue;

    private void Awake()
    {
        spriteColor = GetComponent<Image>();
        if(transform.GetChild(0).GetComponent<Image>() != null) { 
        AngleColor = transform.GetChild(0).GetComponent<Image>();
        }
        else
        {
            AngleColor = GetComponent<Image>();
            Debug.Log("단일선택시에만 떠야하는 디버그임");
            //단일선택시에만 떠야하는 디버그임
        }

    }
 
    private void Update()
    {
      
        greenValue = myObject.currentHP / myObject.maxHP;
        redValue = 1 - greenValue;
        CurrentColor = new Color(redValue - greenValue, greenValue, 0 , 200);

        spriteColor.color = AngleColor.color = CurrentColor;
    }

    public void onDamage()
    {
        greenValue = Mathf.RoundToInt(myObject.currentHP / myObject.maxHP);
        //Debug.Log("라운드" + roundValue);
        redValue = 1 - greenValue;
        CurrentColor = new Color(redValue , greenValue, 0, 200);

        spriteColor.color = AngleColor.color = CurrentColor;
    }
    public void SpriteClick()
    {
        Debug.Log("불리긴하냐");
        //리스트 초기화하고 클릭한버튼에 해당하는 오브젝트 다시 list담기
        /*InfoCo*/
        InfoConecttoUI info = transform.root.gameObject.GetComponent<InfoConecttoUI>();
        info.SingleInfoSetting();
        info.rts.selectTowers.Clear();
        info.rts.selectTowers.Add(GetComponent<unitSpriteController>().myObject);

    }
}
