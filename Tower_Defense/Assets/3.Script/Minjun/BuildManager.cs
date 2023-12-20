using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Pathfinding;

public class BuildManager : NetworkBehaviour
{
    public static BuildManager Instance;

    public GameObject pointPrefab; // ������ ���� ��Ÿ�� ������
    public bool isCanBuild ;  // BuildArea���� �Ѱ��� �������� ���� �� false ��ȯ��.
    private bool isBuilding;
    private int TowerIndex;
    public GameObject[] towers;
    private GameObject currentTower;
    private GameObject currentArea;
    private GameObject SendOB;
    [SerializeField] private BuildAreaPrents[] area; //Ÿ������ 22 32 33 �� ũ�Ⱑ �ٸ� Area �Ҵ��������
    private KeyCode k;

    //��� �Ҵ����ٱ�...
    // 
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        isCanBuild = true;
    }
  

    private void Update()
    {
        // QWER �������� Ÿ������
        // Area Ȱ��ȭ
        // ���콺 UI ���ֱ�
        //���콺 ����Ű �������� �Ǽ� �޼ҵ� ȣ��(��ġ,Ÿ������)
        //���콺 ������Ű �������� Area ��Ȱ��ȭ
       
        Debug.Log("isCanBuild : " + isCanBuild);

        if (!isBuilding)
        {
            //����Ű �������� (���� ��ưŬ�����δ�ü)
            //Ű���´� Area ,Tower �����ϰ� Preview �����ִ��۾�
            BuildReady();
        }
        else
        {
            
            //Preview ���¿��� ���� ���콺 ������ �Ǽ� ( �ݶ��̴� ���ֱ�,����ġ�� �Ǽ�)
            //������Ű �������� Destroy�ϸ鼭 ���.
            BuildDecision();
        }

    }

    private void BuildDecision()
    {
        currentTower.transform.position = currentArea.transform.position;

        if (Input.GetMouseButtonDown(0) && isCanBuild)
        {
            Vector3 targetPos = currentTower.transform.position;
            ClientBuildOrder(targetPos , TowerIndex);
            //���� ��ġ Ÿ�� ���������
            Destroy(currentTower);
            //currentTower.GetComponent<BoxCollider>().enabled = true;
            currentArea.SetActive(false);
            isBuilding = false;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            //���
            Destroy(currentTower);
            currentArea.SetActive(false);
            //Ÿ���ʱ�ȭ, Area �ʱ�ȭ
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
        isBuilding = true;
        TowerIndex = index;
        AreaActiveTrue(index);
        SendOB = towers[index];
        GameObject tower = Instantiate(towers[index], area[index].transform.position, Quaternion.identity);
        currentTower = tower;
        currentArea = area[index].gameObject;
        tower.GetComponent<BoxCollider>().enabled = false;





    }

    private void AreaActiveTrue(int index)
    {
        //������ �ٸ� Area�� �����ִٸ� ���ش�.

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
    private void ClientBuildOrder(Vector3 targetPos, int towerindex)
    {
        CMDBuildOrder(targetPos, towerindex);

    }
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    private void CMDBuildOrder(Vector3 targetPos, int towerindex)
    {
        GameObject newTower = Instantiate(towers[towerindex], targetPos, Quaternion.identity);

        NetworkServer.Spawn(newTower/* , senderConnection*/);
        AstarPath.active.Scan();



    }

    #endregion
    #region ClientRPC
    #endregion
}
