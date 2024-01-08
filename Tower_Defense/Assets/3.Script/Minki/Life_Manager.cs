using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Life_Manager : NetworkBehaviour
{
    public static Life_Manager instance;
    [SerializeField] private Text[] Life_Txt;
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

    private void Start()
    {
        Life_Txt = new Text[2];
        player_image = new Image[2];
        img_index = new int[2];
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

        for (int i = 0; i < 2; i++)
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
    public int P1_Life = 30;
    [SyncVar(hook = nameof(P2_LifeChanged))]
    public int P2_Life = 30;
    [SyncVar(hook = nameof(P3_LifeChanged))]
    public int P3_Life = 30;
    [SyncVar(hook = nameof(P4_LifeChanged))]
    public int P4_Life = 30;

    [SyncVar(hook = nameof(P1_Dead_BoolChange))]
    public bool P1_isDead = false;
    [SyncVar(hook = nameof(P2_Dead_BoolChange))]
    public bool P2_isDead = false;
    [SyncVar(hook = nameof(P3_Dead_BoolChange))]
    public bool P3_isDead = false;
    [SyncVar(hook = nameof(P4_Dead_BoolChange))]
    public bool P4_isDead = false;

    #endregion
    #region Client
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    public void CMD_Setting_Name_Sprite(int player_num, int sprite_index, string name)
    {
        delay_LifeSet(player_num, sprite_index, name);
    }
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_Setting_Name_Sprite(int[] image_index, string[] names)
    {
        for(int i = 0; i < 2; i++)
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
        switch(player_num)
        {
            case 1:
                P1_Life--;
                break;
            case 2:
                P2_Life--;
                break;
           /* case 3:
                P3_Life--;
                break;
            case 4:
                P4_Life--;
                break;*/
        }
        switch(monster_num)
        {
            case 1:
                P1_Life++;
                break;
            case 2:
                P2_Life++;
                break;
          /*  case 3:
                P3_Life++;
                break;
            case 4:
                P4_Life++;
                break;*/
        }
    }

    private void Print_Life()
    {
        for(int i = 0; i < 2; i++)
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
}
