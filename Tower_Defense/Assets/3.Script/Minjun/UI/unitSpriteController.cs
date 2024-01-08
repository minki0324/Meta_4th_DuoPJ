using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class unitSpriteController : MonoBehaviour
{
    public Tower myObject;
    public Monster_Control monster;
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
            Debug.Log("���ϼ��ýÿ��� �����ϴ� �������");
            //���ϼ��ýÿ��� �����ϴ� �������
        }

    }
 
    private void Update()
    {
        if (myObject != null)
        {
            greenValue = myObject.currentHP / myObject.maxHP;
        }
        else if(monster != null)
        {
            greenValue = monster.M_currentHP / monster.M_maxHp;
        }else
        {
            greenValue = 1;
        }
        redValue = 1 - greenValue;
        CurrentColor = new Color(redValue - greenValue, greenValue, 0 , 200);

        spriteColor.color = AngleColor.color = CurrentColor;
    }

    public void onDamage()
    {
        greenValue = Mathf.RoundToInt(myObject.currentHP / myObject.maxHP);
        //Debug.Log("����" + roundValue);
        redValue = 1 - greenValue;
        CurrentColor = new Color(redValue , greenValue, 0, 200);

        spriteColor.color = AngleColor.color = CurrentColor;
    }
    public void SpriteClick()
    {

        //����Ʈ �ʱ�ȭ�ϰ� Ŭ���ѹ�ư�� �ش��ϴ� ������Ʈ �ٽ� list���
        /*InfoCo*/
        InfoConecttoUI info = transform.root.gameObject.GetComponent<InfoConecttoUI>();
        info.rts.DeSelectAll();
        info.rts.SelectUnit(GetComponent<unitSpriteController>().myObject);
        info.SingleInfoSetting();

    }
}
