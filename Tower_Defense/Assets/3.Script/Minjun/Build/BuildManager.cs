using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Pathfinding;
using Pathfinding.Util;

public class BuildManager : NetworkBehaviour
{
    public static BuildManager Instance;
    public BuilderController builder;
    public GameObject pointPrefab; // 가상의 점을 나타낼 프리팹
    public bool isCanBuild;  // BuildArea에서 한개라도 적색으로 변할 시 false 반환함.
    public bool isBuilding;
    private int[] SelectTowerIndexArray;
    private Transform TowerBaseFrame;
    private Transform TowerMountFrame;
    private Transform TowerHeadFrame;
    public GameObject towerFrame;
    public GameObject ScanTower;
    private GameObject currentTower;
    private GameObject currentArea;
    [SerializeField] private BuildAreaPrents[] area; //타워마다 22 32 33 등 크기가 다른 Area 할당해줘야함

    [SerializeField] private Transform SeekerStart;
    [SerializeField] private Transform SeekerEnd;
    [SerializeField] public Resourse resourse; //직접참좀
    private Transform ServerSeekerStart;
    private Transform ServerSeekerEnd;
    public int currentTowerindex;
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

        SelectTowerIndexArray = new int[4];
        GameManager.instance.ListSet();
    }
    private void Start()
    {
        Client_SeekerSet();
    }
    [Client]
    private void Client_SeekerSet()
    {
        SeekerStart = SeekerSet(SeekerStart);
        SeekerEnd = SeekerSet(SeekerEnd);
    }


    private void Update()
    {
        // QWER 눌렀을때 타워지정
        // Area 활성화
        // 마우스 UI 없애기
        //마우스 왼쪽키 눌렀을때 건설 메소드 호출(위치,타워종류)
        //마우스 오른쪽키 눌렀을때 Area 비활성화
        if (isClient)
        {

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
    }
    private Transform SeekerSet(Transform seeker)
    {

        for (int i = 0; i < seeker.childCount; i++)
        {
            if (GameManager.instance.CompareEnumWithTag(seeker.GetChild(i).tag))
            {
                return seeker.GetChild(i);
            }

        }
        return null;
    }
    private Transform SeekerSet(Transform seeker, string clitag)
    {

        for (int i = 0; i < seeker.childCount; i++)
        {
            if (seeker.GetChild(i).tag == clitag)
            {
                return seeker.GetChild(i);
            }

        }
        return null;
    }
    private void BuildDecision()
    {
        currentTower.transform.position = currentArea.transform.position;
        if (Input.GetMouseButtonDown(0) && isCanBuild)
        {
            //resourse.current_mineral -= GameManager.instance.Cost[towerindex];
            if (MonneyCheck(currentTowerindex))
            {
                Vector3 targetPos = currentTower.transform.position;
                //본인의 팀인덱스 줘야함
                int TeamIndex = ((int)GameManager.instance.Player_Num);
                builder.isGoBuild = true;

                builder.BuildOrder(targetPos, SelectTowerIndexArray, TeamIndex);
            }
            Destroy(currentTower);
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
        //여기서 

        if (builder != null)
        {
            if (!builder.isSelectBuilder) return;
        }
        if (Input.GetKeyDown(KeyCode.Q) && MonneyCheck(0))
        {

            BuildSetting(0);
        }
        else if (Input.GetKeyDown(KeyCode.W) && MonneyCheck(1))
        {
            BuildSetting(1);

        }
        else if (Input.GetKeyDown(KeyCode.E) && MonneyCheck(2))
        {
            BuildSetting(2);
        }
        else if (Input.GetKeyDown(KeyCode.R) && MonneyCheck(3))
        {
            BuildSetting(3);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            DestroyButton();
        }
    }

    public bool MonneyCheck(int towerindex)
    {
        if (GameManager.instance.Cost[towerindex] <= resourse.current_mineral)
        {

            currentTowerindex = towerindex;
            return true;
        }
        else
        {
            return false;
        }
        
    }
    public void DestroyButton()
    {
        builder.isCanDestroyTower = true;
    }
    //private bool MonneyCheck()
    //{
    //    if (GameManager.instance.Cost[towerindex] <= resourse.current_mineral)
    //    {

    //        currentTowerCost = GameManager.instance.Cost[towerindex];
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }

    //}
    public void BuildSetting(int index)
    {
        //내가 선택한 타워인덱스 배열 가져오기
        SelectTowerIndexArray = GameManager.instance.towerArrayIndex[index];
        GameObject tower = Instantiate(towerFrame, area[0].transform.position, Quaternion.identity);
        //몇번킬지 설정해야함
        TowerAssembly(tower, SelectTowerIndexArray);             //타워조립
        AreaActiveTrue(currentArea);
        HologramTower(tower);
        currentTower = tower;
        //베이스 콜라이더 끄기
        TowerBaseFrame.GetChild(SelectTowerIndexArray[2]).GetComponent<BoxCollider>().enabled = false;
        float range = tower.GetComponent<Tower>().head.head_Data.ATK_Range;
        tower.GetComponentInChildren<AttackRangeManager>().RangeSet(range);
        isBuilding = true;






    }

    private void TowerAssembly(GameObject tower, int[] SelectTowerIndex)
    {
        TowerBaseFrame = tower.transform.GetChild(0);     //베이스 프레임할당
        TowerMountFrame = tower.transform.GetChild(1);    //마운트 프레임할당

        TowerHeadFrame = TowerMountFrame.GetChild(SelectTowerIndex[1]); //해드 프레임할당

        TowerBaseFrame.GetChild(SelectTowerIndex[2]).gameObject.SetActive(true);  //베이스프레임에서 TowerNumber 타워의 베이스 활성화
        TowerMountFrame.GetChild(SelectTowerIndex[1]).gameObject.SetActive(true); //마운트프레임에서 TowerNumber 타워의 마운트 활성화
        TowerHeadFrame.GetChild(SelectTowerIndex[0]).gameObject.SetActive(true); //해드프레임에서 TowerNumber 타워의 해드 활성화
        Tower newtower = tower.GetComponent<Tower>();
        newtower.towerNum = SelectTowerIndex[3];
        newtower.head = TowerHeadFrame.GetChild(SelectTowerIndex[0]).GetComponent<Tower_Attack>();
        newtower.towerbase = TowerBaseFrame.GetChild(SelectTowerIndex[2]).gameObject;
        int towerAreaIndex = newtower.towerbase.GetComponent<BaseData>().baseData.BuildAreaIndex;
        //newtower.AttackRange.GetComponent<AttackRangeManager>().RangeSet();
        currentArea = area[towerAreaIndex].gameObject;





    }

    private void HologramTower(GameObject gameobject)
    {
        Tower tower = gameobject.GetComponent<Tower>();
        tower?.HologramTower(gameobject);
        tower?.AttackRange.SetActive(true);


    }
    private void AreaActiveTrue(GameObject currentArea)
    {
        //기존에 다른 Area가 켜져있다면 꺼준다.

        for (int i = 0; i < area.Length; i++)
        {
            if (area[i].gameObject.activeSelf)
            {
                area[i].gameObject.SetActive(false);
            }
        }
        currentArea.SetActive(true);
    }
    private bool CheckIfPathClear(Transform Start, Transform End)
    {

        // 시작 위치와 끝 위치를 노드로 변환
        GraphNode startNode = AstarPath.active.GetNearest(Start.position).node;
        GraphNode endNode = AstarPath.active.GetNearest(End.position).node;

        // AstarPathUtilities를 사용하여 길이 존재하는지 확인
        bool pathExists = PathUtilities.IsPathPossible(startNode, endNode);

        return pathExists;
    }
    private void Roadblock(GameObject newTower)
    {
        if (!CheckIfPathClear(ServerSeekerStart, ServerSeekerEnd))
        {
            RPC_DestroyTower(newTower);
            RTSControlSystem.Instance.Destroytower(newTower.GetComponent<Tower>());
            Destroy(newTower);

            Debug.Log("길이막혀서 타워가 파괴되었습니다!");
            AstarPath.active.Scan();

            return;
        }
    }
    //public IEnumerator DestroyMotion_co()
    //{
    //    //메테리얼변경 , 타워기능 정지 , 모션후 타워 파괴
    //}

    #region Client
    [Client]
    public void ClientBuildOrder(Vector3 targetPos, int[] towerindex, int teamIndex)
    {
        int Atkvalue = UpgradeManager.Instance.TowerUpgradesArray[towerindex[3], 0];
        int Rangevalue = UpgradeManager.Instance.TowerUpgradesArray[towerindex[3], 1];
        int HPvalue = UpgradeManager.Instance.TowerUpgradesArray[towerindex[3], 2];
        CMDBuildOrder(targetPos, towerindex, teamIndex, SeekerStart.tag, SeekerEnd.tag, Atkvalue, Rangevalue, HPvalue);


    }
    [Client]
    public void Client_Destroy(GameObject newTower)
    {
        Debug.Log("클라디스트로이 : " + newTower);
        CMD_DestroyTower(newTower);
    }
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    private void CMDBuildOrder(Vector3 targetPos, int[] towerindex, int teamIndex, String Start, String End, int Atkvalue, int Rangevalue, int HPvalue)
    {
        ServerSeekerStart = SeekerSet(SeekerStart, Start); // 막힌지안막힌지 체크하는 시커들
        ServerSeekerEnd = SeekerSet(SeekerEnd, End);

        GameObject newTower = Instantiate(towerFrame, targetPos, Quaternion.identity); 
        TowerAssembly(newTower, towerindex); //커스텀한 타워 조립하고 Tower에있는 변수들 할당
        NetworkServer.Spawn(newTower);  //네트워크스폰해서 동기화

        Tower towerScript = newTower.GetComponent<Tower>();
        //타워 업그레이드 계수 전해주기 (몇번타워?)

        //이미업글되있던 값들 새로운타워에게도 적용해주기
        towerScript.AttackLevel = Atkvalue;
        towerScript.RangeLevel = Rangevalue;
        towerScript.HPLevel = HPvalue;
        towerScript.UpgradeUpdate();

        //태그부여
        newTower.tag = $"{teamIndex}P";

        RPC_TowerAssembly(newTower, towerindex);
        Rpc_SpawnMonster(newTower, teamIndex);
        //서버가 관리하는 타워리스트에 추가
        AllTower.Add(towerScript);
        //타워 활동시작.
        towerScript.isActive = true;
        //Astar 지형업데이트
        AstarPath.active.Scan();
        //길막확인
        Roadblock(newTower);
    }
  

    [Command(requiresAuthority = false)]
    public void ClitoCMD_DestroyTower(GameObject newTower)
    {
        RPC_DestroyTower(newTower);
        RTSControlSystem.Instance.Destroytower(newTower.GetComponent<Tower>());
        Destroy(newTower);


    }
    [Server]
    public void CMD_DestroyTower(GameObject newTower)
    {
        RPC_DestroyTower(newTower);
        RTSControlSystem.Instance.Destroytower(newTower.GetComponent<Tower>());
        Destroy(newTower);

    }

    #endregion
    #region ClientRPC


    [ClientRpc]
    private void RPC_TowerAssembly(GameObject tower, int[] SelectTowerIndex)
    {
        TowerAssembly(tower, SelectTowerIndex);
    }
    [ClientRpc]
    private void Rpc_SpawnMonster(GameObject tower, int TeamIndex)
    {
        if (!isServer)
        {
            // 태그 할당
            tower.tag = $"{TeamIndex}P";
        }
    }
    [ClientRpc]
    private void RPC_DestroyTower(GameObject newTower)
    {
        if (RTSControlSystem.Instance.selectTowers.Contains(newTower.GetComponent<Tower>()))
        {
            RTSControlSystem.Instance.selectTowers.Remove(newTower.GetComponent<Tower>());
            //UI 세팅 다시해줘야함
        }
        Destroy(newTower);
    }

    #endregion
}
