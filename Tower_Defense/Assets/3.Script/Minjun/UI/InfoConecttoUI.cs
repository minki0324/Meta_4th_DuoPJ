using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoConecttoUI : MonoBehaviour
{
    [SerializeField] public RTSControlSystem rts;
    [SerializeField] private GameObject panel; // �������� info �г� /�ش� ������ ������ ��� �Է�
    [SerializeField] private GameObject gridPanel; //���� ���� sprite �г� / �ش������� sprite ,HP �����ͼ� HP ��ʷ� ��� -> ������ ���� ���ϰ��� 
    [SerializeField] private Text MaxHP_CurrentHP;
    [SerializeField] private Text Lvl_Speed;
    [SerializeField] private Text Atk_Range_AS;
    [SerializeField] private Image unitImage;
    [SerializeField] private GameObject[] unitInfoButton; // �ε����� �´� ������ ��������info�гη� �ٲ�.

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
            //���� ���������� �� ������Ʈ�� ������ �����ɴϴ�.
            //ü��, ���ݷ� 
            SingleInfoSetting(); //��������â ����
        }
        else if (rts.selectTowers.Count > 1)
        {
            
            
            MultiInfoSetting(); //�������� ����â ����
        }

        //���ִ��� �𸣰��� �ϴ� ����
        //else
        //{
        //    panel.SetActive(false);
        //}
    }
    private void ButtonActiveReset()
    {
        for (int i = 0; i < unitInfoButton.Length; i++)
        {
            if (!unitInfoButton[i].gameObject.activeSelf) break; //��Ȱ��ȭ �� ��ư�� ������ �׵ڿ� ��ư�� �� �����ִ°ű� ������ for������.
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
        ButtonActiveReset(); //������ �����ִ� ��ư�� �ٽ� ��Ȱ��ȭ
        for (int i = 0; i < rts.selectTowers.Count; i++)
        {
            unitInfoButton[i].gameObject.SetActive(true);     //��ưȰ��ȭ
            unitInfoButton[i].GetComponent<unitSpriteController>().myObject = rts.selectTowers[i];
            unitInfoButton[i].GetComponent<Image>().sprite = rts.selectTowers[i].unitSprite; // ��ư�� sprite�� �ش������� sprite�� �ٲ���
        }
    }
    public void SpriteClick()
    {
        Debug.Log("�Ҹ����ϳ�");
        //����Ʈ �ʱ�ȭ�ϰ� Ŭ���ѹ�ư�� �ش��ϴ� ������Ʈ �ٽ� list���
        /*InfoCo*/
        rts.DeSelectAll();
        rts.selectTowers.Add(GetComponent<unitSpriteController>().myObject);
        SingleInfoSetting();

    }

}
