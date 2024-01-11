using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public enum Player_Num
{
    P1 = 1,
    P2,
    P3,
    P4
}

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public Player_Num Player_Num;
    [SerializeField] private Room_FinalData data;
    public int img_index;
    public string Nickname;

    public List<int[]> towerArrayIndex;
    public int towerIndexCount = 0;
    public int[] Tower_1_index = new int[4];
    public int[] Tower_2_index = new int[4];
    public int[] Tower_3_index = new int[4];
    public int[] Tower_4_index;
    public int[] Cost;
    public float[] Damage;
    public float[] Range;
    public float[] ATK_Speed;
    public float[] Head_Rot_speed;
    public float[] Health;


    public Room_Manager manager;
    public Life_Manager life;

    public bool isDead = false;
    

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
        manager = FindObjectOfType<Room_Manager>();
        Tower_4_index =new int[] { 8, 4, 5 ,3};

    }
    private void Start()
    {
        Init_Data();
        Cost[3] = 50;
    }
    public void ListSet()
    {
        towerArrayIndex = new List<int[]>();
        towerArrayIndex.Add(Tower_1_index);
        towerArrayIndex.Add(Tower_2_index);
        towerArrayIndex.Add(Tower_3_index);
        towerArrayIndex.Add(Tower_4_index);
    }

    public int[] GetCost()
    {
        return Cost;
    }
    #endregion
    #region SyncVar
    #endregion
    #region Client
    [Client]
    public void Active_Set(bool isActive, GameObject monster)
    {
        CMD_ActiveSet(isActive, monster);
    }

    // 내 닉네임과 스프라이트 인덱스 보내주기
    // 카메라 초기화 시점에 보내줘야함
    [Client]
    public void Send_name_sprite()
    {
        if (life == null)
        {
            life = FindObjectOfType<Life_Manager>();
        }
        if(!isServer)
        {
            life.CMD_Setting_Name_Sprite((int)Player_Num-1, img_index, Nickname);
        }
    }
    #endregion
    #region Command
    [Command(requiresAuthority = true)]
    public void CMD_TransformSet(GameObject gameObject, Vector3 pos, Quaternion rot)
    {
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rot;
        RPC_TransformSet(gameObject, pos, rot);
    }

    [Command(requiresAuthority = false)]
    private void CMD_ActiveSet(bool isActive, GameObject monster)
    {
        RPC_ActiveSet(isActive, monster);
    }

    #endregion
    #region ClientRPC
    [ClientRpc]
    public void RPC_TransformSet(GameObject gameObject, Vector3 pos, Quaternion rot)
    {
        if (gameObject == null)
        {
            Debug.Log("gameObject is null!");
            return;
        }

        gameObject.transform.position = pos;
        gameObject.transform.rotation = rot;
    }

    [ClientRpc]
    public void RPC_ActiveSet(bool isActive, GameObject monster)
    {
        if (isActive)
        {
            monster.SetActive(isActive);
        }
        else
        {
            monster.SetActive(!isActive);
        }
    }
    #endregion
    #region Hook Method
    #endregion

    private void Init_Data()
    {
        Cost = new int[4];
        Damage = new float[4];
        Range = new float[4];
        Head_Rot_speed = new float[4];
        ATK_Speed = new float[4];
        Health = new float[4];
    }

    public void Set_Player_Num()
    {
        Room_Player[] players = FindObjectsOfType<Room_Player>();

        foreach (var player in players)
        {
            if (player.isLocalPlayer)
            {
                Player_Num = (Player_Num)player.netId - 1;
            }
        }
        Debug.Log(Player_Num);
    }

    public void Set_Tower_Index()
    {
        Tower_1_index = data.Prefab_1_index;
        Tower_2_index = data.Prefab_2_index;
        Tower_3_index = data.Prefab_3_index;
    }

    public int[] Send_index()
    {
        int[] player_index = new int[] { (int)Player_Num, this.img_index };
        return player_index;
    }

    //나의 팀인덱스와 , 오브젝트의 태그를 비교해서 나의소유인지 확인하는 메소드
    public bool CompareEnumWithTag(string tag)
    {
        // Enum 값을 문자열로 변환합니다.
        string enumString = Player_Num.ToString();

        // Enum 문자열에서 앞의 숫자를 추출합니다.
        string enumNumber = new string(enumString.Where(char.IsDigit).ToArray());
        string newenumString = enumNumber + 'P';
        /*Debug.Log(newenumString);*/

        if (newenumString == tag)
        {
            /*Debug.Log(true);*/
            return true;
        }
        else
        {
            /*Debug.Log(tag);*/
            return false;
        }
    }

   
}
