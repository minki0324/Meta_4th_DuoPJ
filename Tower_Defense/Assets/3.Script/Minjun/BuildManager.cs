using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Pathfinding;

public class BuildManager : NetworkBehaviour
{
    public static BuildManager Instance;

    public GameObject pointPrefab; // 가상의 점을 나타낼 프리팹
    public bool isCanBuild ;  // BuildArea에서 한개라도 적색으로 변할 시 false 반환함.
    public bool isBuilding;
    public bool isNoCompo;
    private int[] SelectTowerIndexArray;
    public GameObject towerFrame;
    private Transform TowerBaseFrame;
    private Transform TowerMountFrame;
    private Transform TowerHeadFrame;
    private GameObject currentTower;
    private GameObject currentArea;
    [SerializeField] private BuildAreaPrents[] area; //타워마다 22 32 33 등 크기가 다른 Area 할당해줘야함

    #region SyncVar
    public SyncList<Tower> AllTower = new SyncList<Tower>();
    #endregion


    //어떻게 할당해줄까...
    // 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        isCanBuild = true;

        SelectTowerIndexArray = new int[3];
        GameManager.instance.ListSet();
    }
    private void Start()
    {
        
    }


    private void Update()
    {
        // QWER 눌렀을때 타워지정
        // Area 활성화
        // 마우스 UI 없애기
        //마우스 왼쪽키 눌렀을때 건설 메소드 호출(위치,타워종류)
        //마우스 오른쪽키 눌렀을때 Area 비활성화
       

        if (!isBuilding)
        {
            //빌드키 눌렀을때 (추후 버튼클릭으로대체)
            //키에맞는 Area ,Tower 세팅하고 Preview 보여주는작업
            BuildReady();
        }
        else
        {
            
            //Preview 상태에서 왼쪽 마우스 누르면 건설 ( 콜라이더 켜주기,그위치에 건설)
            //오른쪽키 눌렀을때 Destroy하면서 취소.
            BuildDecision();
        }

    }

    private void BuildDecision()
    {
        Debug.Log("현재타워 : "+ currentTower);
        Debug.Log("현재Area : "+currentArea);
        currentTower.transform.position = currentArea.transform.position;

        if (Input.GetMouseButtonDown(0) && isCanBuild)
        {
            Vector3 targetPos = currentTower.transform.position;
            ClientBuildOrder(targetPos , SelectTowerIndexArray);

            //어떤클라이언트의 몇번 타워 인덱스인지 보내줘야함.
            //지어 위치 타워 보내줘야함
            Destroy(currentTower);
            //currentTower.GetComponent<BoxCollider>().enabled = true;
            currentArea.SetActive(false);
            isBuilding = false;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            //취소
            Destroy(currentTower);
            currentArea.SetActive(false);
            //타워초기화, Area 초기화
            currentTower = null;
            currentArea = null;
            isBuilding = false;
        }
    }

    private void BuildReady()
    {


        if (Input.GetKeyDown(KeyCode.Q))
        {
            BuildSetting(0);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            BuildSetting(1);

        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            BuildSetting(2);
        }
    }

    private void BuildSetting(int index)
    {
        //내가 선택한 타워인덱스 배열 가져오기
        SelectTowerIndexArray = GameManager.instance.towerIndex[index];
        AreaActiveTrue(index);
        GameObject tower = Instantiate(towerFrame, area[index].transform.position, Quaternion.identity);
        //몇번킬지 설정해야함
        isNoCompo = true;
        TowerAssembly(tower , SelectTowerIndexArray);
        isNoCompo = false; ;
        HologramTower(tower);
        currentTower = tower;
        currentArea = area[index].gameObject;
        //베이스 콜라이더 끄기
        TowerBaseFrame.GetChild(SelectTowerIndexArray[2]).GetComponent<BoxCollider>().enabled = false;
        isBuilding = true;






    }

    private void TowerAssembly(GameObject tower , int[] SelectTowerIndex)
    {
        TowerBaseFrame = tower.transform.GetChild(0);     //베이스 프레임할당
        TowerMountFrame = tower.transform.GetChild(1);    //마운트 프레임할당

        TowerHeadFrame = TowerMountFrame.GetChild(SelectTowerIndex[1]); //해드 프레임할당

        TowerBaseFrame.GetChild(SelectTowerIndex[2]).gameObject.SetActive(true);  //베이스프레임에서 TowerNumber 타워의 베이스 활성화
        TowerMountFrame.GetChild(SelectTowerIndex[1]).gameObject.SetActive(true); //마운트프레임에서 TowerNumber 타워의 마운트 활성화
        TowerHeadFrame.GetChild(SelectTowerIndex[0]).gameObject.SetActive(true); //해드프레임에서 TowerNumber 타워의 해드 활성화
       
    }

    private void HologramTower(GameObject gameobject)
    {
        Tower tower = gameobject.GetComponent<Tower>();
        tower?.HologramTower(gameobject);
        tower?.AttackRange.SetActive(true);


    }
    private void AreaActiveTrue(int index)
    {
        //기존에 다른 Area가 켜져있다면 꺼준다.

        for (int i = 0; i < area.Length; i++)
        {
            if (area[i].gameObject.activeSelf)
            {
                area[i].gameObject.SetActive(false);
            }
        }

        area[index].gameObject.SetActive(true);
    }


    #region Client
    [Client]
    private void ClientBuildOrder(Vector3 targetPos, int[] towerindex)
    {
        CMDBuildOrder(targetPos, towerindex);

    }
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    private void CMDBuildOrder(Vector3 targetPos, int[] towerindex)
    {
        GameObject newTower = Instantiate(towerFrame, targetPos, Quaternion.identity);
        TowerAssembly(newTower, towerindex);
        NetworkServer.Spawn(newTower/* , senderConnection*/);
        RPC_TowerAssembly(newTower, towerindex);
        AllTower.Add(newTower.GetComponent<Tower>());
        AstarPath.active.Scan();



    }

    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_TowerAssembly(GameObject tower, int[] SelectTowerIndex)
    {
        TowerAssembly(tower, SelectTowerIndex);
    }
    #endregion
}
