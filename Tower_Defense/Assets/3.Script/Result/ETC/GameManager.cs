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

    public List<int[]> towerIndex;
    public int towerIndexCount = 0;
    public int[] Tower_1_index = new int[3];
    public int[] Tower_2_index = new int[3];
    public int[] Tower_3_index = new int[3];

    public Room_Manager manager;


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
       
   
    }
    public void ListSet()
    {
        towerIndex = new List<int[]>();
        towerIndex.Add(Tower_1_index);
        towerIndex.Add(Tower_2_index);
        towerIndex.Add(Tower_3_index);
        Debug.Log(towerIndex.Count);
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
