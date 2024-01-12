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
        Builder, //BuilderController에서 isSelectBuilder 일때 바꿔준다? || 1번눌렀을때 바꿔준다.
        Spawner, // 2번 눌렀을때 Spawner로 바꿔준다 //기존선택되있던거 초기화.
        Upgrade   // 3번 눌렀을때 Upgrade바꿔준다
    }
    [SerializeField] public RTSControlSystem rts;
    [SerializeField] private GameObject panel; // 단일유닛 info 패널 /해당 유닛의 정보들 모두 입력
    [SerializeField] private GameObject gridPanel; //다중 유닛 sprite 패널 / 해당유닛의 sprite ,HP 가져와서 HP 비례로 녹색 -> 빨간색 으로 변하게함 
    [SerializeField] private Text MaxHP_CurrentHP;
    [SerializeField] private Text Lvl_Speed;
    [SerializeField] private Text Atk_Range_AS;
    [SerializeField] private Text Name;
    [SerializeField] private Text style;
    [SerializeField] private Image unitImage;
    [SerializeField] private GameObject[] unitInfoButton; // 인덱스에 맞는 유닛의 단일유닛info패널로 바꿈.
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

        //원래같으면 Hook 사용으로 갱신될때마다 부르는게 맞으나 이게편함 ㅇㅇ
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
            SingleInfoSetting();//단일정보창 띄우기
        }
        else if (isMultiple)
        {
            MultiInfoSetting(); //다중유닛 정보창 띄우기
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
                SpawnerShortcutKey();
                
                break;
            case Type.Upgrade:
                SettingOrderUI(3);
                UpgradeShortcutKey();
                
                break;
        }
    }

    private void UpgradeShortcutKey()
    {
        if (Input.GetKeyDown(KeyCode.Q) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.W) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.E) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(2, 0);
        }
        else if (Input.GetKeyDown(KeyCode.R) && UpgradeCostCheck(100, 1))
        {
            //스캔범위
            UpgradeManager.Instance.onClickUp(3, 1);
        }
        else if (Input.GetKeyDown(KeyCode.A) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.D) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(2, 1);
        }
        else if (Input.GetKeyDown(KeyCode.F) && UpgradeCostCheck(100, 1))
        {
            //라이프증가
            //BuildManager.Instance.resourse.life += 5;
            //보류
        }
        else if (Input.GetKeyDown(KeyCode.Z) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(0, 2);
        }
        else if (Input.GetKeyDown(KeyCode.X) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(1, 2);
        }
        else if (Input.GetKeyDown(KeyCode.C) && UpgradeCostCheck(100, 1))
        {
            UpgradeManager.Instance.onClickUp(2, 2);
        }
        else if (Input.GetKeyDown(KeyCode.V) && UpgradeCostCheck(100, 1))
        {
            //인구증가
            BuildManager.Instance.resourse.max_food += 5;
        }
    }

    private void SpawnerShortcutKey()
    {
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
    }

    public void BuilderInfoUI()
    {
        ButtonActiveReset();
        panel.SetActive(true);
        Lvl_Speed.text = string.Format("{0} \n {1}", 99, '-');
        Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", '-', '-', '-');
        MaxHP_CurrentHP.text = string.Format("{0} / {1}", 9999, 9999);
        unitImage.sprite = BuilderImage;
        Name.text = "해피";
        style.text = "건설자";
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
            //단일 선택했을시 그 오브젝트의 정보를 가져옵니다.
            //체력, 공격력 
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
        bool isAttackUp = rts.selectTowers[0].AttackLevel > 0;
        bool isRangeUp = rts.selectTowers[0].RangeLevel > 0;

        //소수점 셋째자리수까지만 반올림해서 살림
        float upDamage = rts.selectTowers[0].upDamage;
        float upAS = rts.selectTowers[0].upAS;
        float upRange = rts.selectTowers[0].upRange;

        if (isAttackUp && isRangeUp)
        {
            Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", $"{rts.selectTowers[0].damage} +{upDamage}", $"{rts.selectTowers[0].range} +{upRange}", $"{rts.selectTowers[0].atkSpeed} -{upAS}");
        }
        else if (isAttackUp || isRangeUp)
        {
            if (isAttackUp)
            {
                Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", $"{rts.selectTowers[0].damage} +{upDamage}", rts.selectTowers[0].range, $"{rts.selectTowers[0].atkSpeed} -{upAS}");
            }
            else if (isRangeUp)
            {
                Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", rts.selectTowers[0].damage, $"{rts.selectTowers[0].range} +{upRange}", rts.selectTowers[0].atkSpeed);

            }

        }
        else
        {
            Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", rts.selectTowers[0].damage, rts.selectTowers[0].range, rts.selectTowers[0].atkSpeed);
        }

        MaxHP_CurrentHP.text = string.Format("{0} / {1}", rts.selectTowers[0].maxHP, rts.selectTowers[0].currentHP);
        Lvl_Speed.text = string.Format("{0} \n {1}", rts.selectTowers[0].level, rts.selectTowers[0].Speed);

        //Atk_Range_AS.text = $"{rts.selectTowers[0].damage} +{upDamage}\n" +
        //      $"{rts.selectTowers[0].range} +{upRange}\n" +
        //      $"{rts.selectTowers[0].atkSpeed} +{upAS}";

        unitImage.sprite = rts.selectTowers[0].unitSprite;
        Name.text = rts.selectTowers[0].towerName;
        style.text = rts.selectTowers[0].towerType;
    }


    private float F3Round(float temp)
    {
        return Mathf.Round(temp * 1000) / 100f;
    }

    public void MonsterInfoSetting(Monster_Control monster)
    {
        ButtonActiveReset();
        panel.SetActive(true);
        //rts.selectTowers[0].maxHP;
        unitImage.gameObject.GetComponent<unitSpriteController>().monster = monster;
        Lvl_Speed.text = string.Format("{0} \n {1}", monster.Lvl, monster.M_speed);
        Atk_Range_AS.text = string.Format("{0} \n {1}\n {2}", monster.M_damage, '-', '-');
        MaxHP_CurrentHP.text = string.Format("{0} / {1}", monster.M_maxHp, (int)monster.M_currentHP);
        unitImage.sprite = monster.unitImage;
        Name.text = monster.state.monsterName;
        style.text = monster.state.monsterType;
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
    public bool UpgradeCostCheck(int mineralCost, int CrystalCost)
    {
        if (BuildManager.Instance.resourse.current_mineral >= mineralCost && BuildManager.Instance.resourse.current_crystal >= CrystalCost)
        {
            BuildManager.Instance.resourse.current_mineral -= mineralCost;
            BuildManager.Instance.resourse.current_crystal -= CrystalCost;
            return true;
        }
        else
        {
            return false;
        }
    }
}
