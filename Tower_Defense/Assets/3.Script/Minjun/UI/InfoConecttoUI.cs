using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InfoConecttoUI : MonoBehaviour
{
    public static InfoConecttoUI Instance;


    public enum Type
    {
        Empty = 0,
        Builder, //BuilderController���� isSelectBuilder �϶� �ٲ��ش�? || 1���������� �ٲ��ش�.
        Spawner, // 2�� �������� Spawner�� �ٲ��ش� //�������õ��ִ��� �ʱ�ȭ.
        Upgrade   // 3�� �������� Upgrade�ٲ��ش�
    }
    [SerializeField] public RTSControlSystem rts;
    [SerializeField] private GameObject panel; // �������� info �г� /�ش� ������ ������ ��� �Է�
    [SerializeField] private GameObject gridPanel; //���� ���� sprite �г� / �ش������� sprite ,HP �����ͼ� HP ��ʷ� ��� -> ������ ���� ���ϰ��� 
    [SerializeField] private Text MaxHP_CurrentHP;
    [SerializeField] private Text Lvl_Speed;
    [SerializeField] private Text Atk_Range_AS;
    [SerializeField] private Text Name;
    [SerializeField] private Text style;
    [SerializeField] private Image unitImage;
    [SerializeField] private GameObject[] unitInfoButton; // �ε����� �´� ������ ��������info�гη� �ٲ�.
    [SerializeField] private GameObject[] orderUI;
    [SerializeField] private Monster_Spawn spawner;
    [SerializeField] private Sprite BuilderImage;
    public Type type;
    private bool isSingle;
    private bool isMultiple;
    public bool isMonsterClick;
    public bool isBuilderClick;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
        type = Type.Empty;
        TryGetComponent(out spawner);
    }
    private void Update()
    {

        //���������� Hook ������� ���ŵɶ����� �θ��°� ������ �̰����� ����
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            rts.DeSelectAll();
            BuildManager.Instance.builder.isSelectBuilder = true;
            isMonsterClick = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            rts.DeSelectAll();
            BuildManager.Instance.builder.isSelectBuilder = false;
            isMonsterClick = false;
            type = Type.Spawner;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            rts.DeSelectAll();
            BuildManager.Instance.builder.isSelectBuilder = false;
            isMonsterClick = false;
            type = Type.Upgrade;
        }
            UpdateUI();

    }

    public void UpdateUI()
    {
        if (!isBuilderClick &&!isMonsterClick && rts.selectTowers.Count == 0)
        {
            isSingle = false;
            isMultiple = false;
            panel.SetActive(false);
            ButtonActiveReset();
        }else if (isBuilderClick)
        {
            BuilderInfoUI();
        }
        else if (isSingle)
        {
            SingleInfoSetting();//��������â ����
        }
        else if (isMultiple)
        {
            MultiInfoSetting(); //�������� ����â ����
        }

        switch (type)
        {
            case Type.Empty:
                SettingOrderUI(0);
                break;
            case Type.Builder:
                SettingOrderUI(1);
                break;
            case Type.Spawner:
                SettingOrderUI(2);
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    spawner.Onclick(0);
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    spawner.Onclick(1);
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    spawner.Onclick(2);
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    spawner.Onclick(3);
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    spawner.Onclick(4);
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    spawner.Onclick(5);
                }
                break;
            case Type.Upgrade:
                SettingOrderUI(3);
                break;
        }
    }

    public void BuilderInfoUI()
    {
        ButtonActiveReset();
        panel.SetActive(true);
        Lvl_Speed.text = string.Format("{0} \n {1}", 99, '-');
        Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", '-', '-', '-');
        MaxHP_CurrentHP.text = string.Format("{0} / {1}", 9999, 9999);
        unitImage.sprite = BuilderImage;
        Name.text = "����";
        style.text = "�Ǽ���";
    }

    private void SettingOrderUI(int index)
    {
        for (int i = 0; i < orderUI.Length; i++)
        {
            if (i == index)
            {
                orderUI[i].SetActive(true);
            }
            else
            {
                orderUI[i].SetActive(false);
            }
        }
    }

    public void SetInfoPanel()
    {
        if ( rts.selectTowers.Count == 0)
        {
            isSingle = false;
            isMultiple = false;
            panel.SetActive(false);
            ButtonActiveReset();
        }

        if (rts.selectTowers.Count == 1)
        {
            //���� ���������� �� ������Ʈ�� ������ �����ɴϴ�.
            //ü��, ���ݷ� 
            isSingle = true;
            isMultiple = false;

        }
        else if (rts.selectTowers.Count > 1)
        {

            isSingle = false;
            isMultiple = true;
        }
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
        Name.text = rts.selectTowers[0].towerName;
        style.text = rts.selectTowers[0].towerType;
    }
    public void MonsterInfoSetting(Monster_Control monster)
    {
        ButtonActiveReset();
        panel.SetActive(true);
        //rts.selectTowers[0].maxHP;
        unitImage.gameObject.GetComponent<unitSpriteController>().monster = monster;
        Lvl_Speed.text = string.Format("{0} \n {1}", monster.Lvl, monster.M_speed);
        Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", monster.M_damage, '-', '-');
        MaxHP_CurrentHP.text = string.Format("{0} / {1}", monster.M_maxHp, monster.M_currentHP);
        unitImage.sprite = monster.unitImage;
        Name.text = monster.state.monsterName;
        style.text = monster.state.monsterType;
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
