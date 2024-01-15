using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class Life_Manager : NetworkBehaviour
{
    public static Life_Manager instance;
    [SerializeField] private Text[] Life_Txt;
    private Minimap_Click timer;

    public Sprite[] image_array;
    public Image[] player_image;
    public string[] player_name;

    public int[] img_index;

    public bool isInit = false;

    #region Unity Callback
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void PlayerCheck()
    {
       
    }

    private void Start()
    {
        Life_Txt = new Text[4];
        player_image = new Image[4];
        img_index = new int[4];
        timer = FindObjectOfType<Minimap_Click>();
        StartCoroutine(init_data());
    }

    private void Update()
    {
        if(!isServer && isInit)
        {
            Print_Life();
        }
    }

    private IEnumerator init_data()
    {
        yield return new WaitForSeconds(0.5f);
        GameObject panel = GameObject.FindGameObjectWithTag("Life");

        for (int i = 0; i < player_name.Length; i++)
        {
            player_image[i] = panel.transform.GetChild(i).GetChild(0).GetComponent<Image>();
            Life_Txt[i] = panel.transform.GetChild(i).GetChild(3).GetComponent<Text>();
        }

        if (!isServer)
        {
            GameManager.instance.Send_name_sprite();
            isInit = true;
        }
    }
    #endregion
    #region SyncVar
    [SyncVar(hook = nameof(P1_LifeChanged))]
    public int P1_Life = 0;
    [SyncVar(hook = nameof(P2_LifeChanged))]
    public int P2_Life = 0;
    [SyncVar(hook = nameof(P3_LifeChanged))]
    public int P3_Life = 0;
    [SyncVar(hook = nameof(P4_LifeChanged))]
    public int P4_Life = 0;

    [SyncVar(hook = nameof(P1_Dead_BoolChange))]
    public bool P1_isDead = true;
    [SyncVar(hook = nameof(P2_Dead_BoolChange))]
    public bool P2_isDead = true;
    [SyncVar(hook = nameof(P3_Dead_BoolChange))]
    public bool P3_isDead = true;
    [SyncVar(hook = nameof(P4_Dead_BoolChange))]
    public bool P4_isDead = true;

    #endregion
    #region Client
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    public void CMD_Setting_Name_Sprite(int player_num, int sprite_index, string name)
    {
        StartCoroutine(delay_LifeSet(player_num, sprite_index, name));
    }
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_Setting_Name_Sprite(int[] image_index, string[] names)
    {
        for(int i = 0; i < player_name.Length; i++)
        {
            player_image[i].sprite = image_array[image_index[i]];
        }
        player_name = names;
    }
    #endregion
    #region Hook Method
    private void P1_LifeChanged(int old_, int new_)
    {
        P1_Life = new_;
    }
    private void P2_LifeChanged(int old_, int new_)
    {
        P2_Life = new_;
    }
    private void P3_LifeChanged(int old_, int new_)
    {
        P3_Life = new_;
    }
    private void P4_LifeChanged(int old_, int new_)
    {
        P4_Life = new_;
    }

    private void P1_Dead_BoolChange(bool old_, bool new_)
    {
        P1_isDead = new_;
    }
    private void P2_Dead_BoolChange(bool old_, bool new_)
    {
        P2_isDead = new_;
    }
    private void P3_Dead_BoolChange(bool old_, bool new_)
    {
        P3_isDead = new_;
    }
    private void P4_Dead_BoolChange(bool old_, bool new_)
    {
        P4_isDead = new_;
    }
    #endregion

    public void Life_Set(int player_num, int monster_num)
    {
        switch (player_num)
        {
            case 1:
                P1_Life--;
                break;
            case 2:
                P2_Life--;
                break;
            case 3:
                P3_Life--;
                break;
            case 4:
                P4_Life--;
                break;
        }

        if (P1_Life == 10 || P2_Life == 10 || P3_Life == 10 || P4_Life == 10) 
        {
           Error_Log.instance.RPC_Log($"{player_name[player_num - 1]}님의 라이프가 10 밖에 남지 않았습니다.");
        }

        if (!IsPlayerDead(player_num))
        {
            switch (monster_num)
            {
                case 1:
                    P1_Life++;
                    break;
                case 2:
                    P2_Life++;
                    break;
                case 3:
                    P3_Life++;
                    break;
                case 4:
                    P4_Life++;
                    break;
            }
            if (isServer)
            {
                float time = timer.current_timer;
                float sec = time % 60f;
                float min = time / 60f;
                Kill_Log.instance.Adding_Message($"<color=FFFFFF>[{(int)min} : {(int)sec}]</color>  <color=E34E4E> {player_name[monster_num - 1]} </color> 님이 <color=FFFFFF> {player_name[player_num - 1]} </color>님의 <color=FFFFFF> Life 1 </color>을 획득합니다.");
            }
        }
        Die(player_num);
    }

    private void Print_Life()
    {
        for(int i = 0; i < player_name.Length; i++)
        {
            int life;
            switch(i)
            {
                case 0:
                    life = P1_Life;
                    break;
                case 1:
                    life = P2_Life;
                    break;
                case 2:
                    life = P3_Life;
                    break;
                case 3:
                    life = P4_Life;
                    break;
                default:
                    life = -1;
                    break;
            }
            Life_Txt[i].text = $"{player_name[i]} / {life}";
        }
    }

    private IEnumerator delay_LifeSet(int player_num, int sprite_index, string name)
    {
        yield return new WaitForSeconds(0.2f);

        player_image[player_num].sprite = image_array[sprite_index];
        player_name[player_num] = name;
        img_index[player_num] = sprite_index;
        RPC_Setting_Name_Sprite(img_index, player_name);
    }

    private void Die(int player_num)
    {
        float time = timer.current_timer;
        float sec = time % 60f;
        float min = time / 60f;
        switch (player_num)
        {
            case 1:
                if(P1_Life <= 0)
                {
                    Kill_Log.instance.Adding_Message($"<color=FFFFFF>[{(int)min} : {(int)sec}]</color>  <color=E34E4E> {player_name[player_num]} </color> 님이 패배했습니다.");
                    Error_Log.instance.Print_Log($"{player_name[player_num]}님이 패배 했습니다.");
                    P1_isDead = true;
                }
                break;
            case 2:
                if (P2_Life <= 0)
                {
                    Kill_Log.instance.Adding_Message($"<color=FFFFFF>[{(int)min} : {(int)sec}]</color>  <color=E34E4E> {player_name[player_num]} </color> 님이 패배했습니다.");
                    Error_Log.instance.Print_Log($"{player_name[player_num]}님이 패배 했습니다.");
                    P2_isDead = true;
                }
                break;
            case 3:
                if (P3_Life <= 0)
                {
                    Kill_Log.instance.Adding_Message($"<color=FFFFFF>[{(int)min} : {(int)sec}]</color>  <color=E34E4E> {player_name[player_num]} </color> 님이 패배했습니다.");
                    Error_Log.instance.Print_Log($"{player_name[player_num]}님이 패배 했습니다.");
                    P3_isDead = true;
                }
                break;
            case 4:
                if (P4_Life <= 0)
                {
                    Kill_Log.instance.Adding_Message($"<color=FFFFFF>[{(int)min} : {(int)sec}]</color>  <color=E34E4E> {player_name[player_num]} </color> 님이 패배했습니다.");
                    Error_Log.instance.Print_Log($"{player_name[player_num]}님이 패배 했습니다.");
                    P4_isDead = true;
                }
                break;
        }
    }
    public bool IsPlayerDead(int playerNum)
    {
        switch (playerNum)
        {
            case 1:
                return P1_isDead;
            case 2:
                return P2_isDead;
            case 3:
                return P3_isDead;
            case 4:
                return P4_isDead;
            default:
                return true; // 잘못된 플레이어 번호
        }
    }
    public bool isVectoryCheck(int playerNum)
    {
        switch (playerNum)
        {
            case 1:
                return P2_isDead && P3_isDead && P4_isDead;
            case 2:
                return P1_isDead && P3_isDead && P4_isDead;
            case 3:
                return P1_isDead && P2_isDead && P4_isDead;
            case 4:
                return P1_isDead && P2_isDead && P3_isDead;
            default:
                return true; // 잘못된 플레이어 번호
        }
    }
}
