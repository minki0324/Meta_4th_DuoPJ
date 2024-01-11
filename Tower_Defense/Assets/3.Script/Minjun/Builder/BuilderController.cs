using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Mirror;
using System;

public class BuilderController : NetworkBehaviour
{


    //[SerializeField] private AIPath aipath;
    //[SerializeField] private AIDestinationSetter aIDestinationSetter;
    [SerializeField] private float moveSpeed = 5;
    public Vector3 targetposition;
    [SerializeField] private Transform SpawnPoint;
    [SerializeField] private GameObject Marker;
    private Transform[] SpawnPoints;
    private Transform mySpawnPoint;
    //[SerializeField] private LayerMask IgnoreCast;
    [SerializeField] private LayerMask groundLayer;
    public bool isSelectBuilder;
    public bool tempisSelectBuilder;
    [SyncVar]
    public bool isMoving;
    [SyncVar]
    public bool isGoBuild;
    public bool isCanDestroyTower;
    private Coroutine OrderCoroutine;
    private Coroutine DestroyCoroutine;
    private float minX;
    private float maxX;
    private void Awake()
    {
        SpawnPoints = new Transform[SpawnPoint.childCount];
        for (int i = 0; i < SpawnPoint.childCount; i++)
        {
            SpawnPoints[i] = SpawnPoint.GetChild(i);
        }

    }



    void Start()
    {
        Initialsettings();
        minX =transform.position.x -  30f;
        maxX = transform.position.x +  30f;

    }

    // Update is called once per frame
    void Update()
    {
        InfoConecttoUI.Instance.isBuilderClick = isSelectBuilder;
        if (isSelectBuilder)
        {
            TargerSet();
            if (InfoConecttoUI.Instance.type != InfoConecttoUI.Type.Builder)
            {
                InfoConecttoUI.Instance.type = InfoConecttoUI.Type.Builder;

                InfoConecttoUI.Instance.isBuilderClick = true;
            }
        }

        if (isCanDestroyTower)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.D))
            {

            }
            else if (Input.anyKeyDown)
            {
                isCanDestroyTower = false;
            }
        }
        if (isMoving)
        {
            BuilderMove();
        }
        onMarker();
        //ClampBuilderPosition();
    }

    private void ClampBuilderPosition()
    {
      float Xpos = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(Xpos, transform.position.y, transform.position.z);
    }

    private void Initialsettings()
    {
        if (isLocalPlayer && !isServer)
        {
            int TeamIndex = ((int)GameManager.instance.Player_Num);
            BuildManager.Instance.builder = this;
            tag = $"{TeamIndex}P";
            FindPoint();
            CliTag(gameObject, tag);
            transform.position = mySpawnPoint.position;
        }
    }
    private void FindPoint()
    {
        foreach (Transform point in SpawnPoint)
        {
            if (GameManager.instance.CompareEnumWithTag(point.gameObject.tag))
            {
                mySpawnPoint = point;
                return;
            }
        }
    }
    private void onMarker()
    {
        if (isSelectBuilder)
        {
            Marker.SetActive(true);
        }
        else
        {
            Marker.SetActive(false);

        }
    }
    #region Client
    [Client]
    public void BuildOrder(Vector3 targetPos, int[] towerindex, int teamIndex)
    {
        isGoBuild = true;
        isMoving = false;
        CustomAllStopCo();
        OrderCoroutine = StartCoroutine(BuildOrder_co(targetPos, towerindex, teamIndex)); ;


    }
    [Client]
    public void DestroyOrder(GameObject destroyTower)
    {
        isGoBuild = true;
        isMoving = false;

        CustomAllStopCo();
        DestroyCoroutine = StartCoroutine(BuilderDestroyOrder_co(destroyTower));

    }

    private void CustomAllStopCo()
    {
        if (OrderCoroutine != null)
        {
            StopCoroutine(OrderCoroutine);
        }
        if (DestroyCoroutine != null)
        {
            StopCoroutine(DestroyCoroutine);
        }
    }

    private IEnumerator BuildOrder_co(Vector3 targetPos, int[] towerindex, int teamIndex)
    {
        transform.LookAt(targetPos);// Ÿ�����°��� �ٶ󺸴� �ڵ�
        while (isGoBuild)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) <= 1)
            {
                if (BuildManager.Instance.MonneyCheck(BuildManager.Instance.currentTowerindex))
                {
                    BuildManager.Instance.resourse.current_mineral -= GameManager.instance.Cost[BuildManager.Instance.currentTowerindex];
                    BuildManager.Instance.ClientBuildOrder(targetPos, towerindex, teamIndex);
                }

              
                isGoBuild = false;
                yield break;
            }
            yield return null;
        }
    }
    private IEnumerator BuilderDestroyOrder_co(GameObject destroyTower)
    {
        transform.LookAt(destroyTower.transform.position);// Ÿ�����°��� �ٶ󺸴� �ڵ�
        while (isGoBuild)
        {
            transform.position = Vector3.MoveTowards(transform.position, destroyTower.transform.position, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, destroyTower.transform.position) <= 1)
            {
                BuildManager.Instance.ClitoCMD_DestroyTower(destroyTower);
                isCanDestroyTower = false;
                isGoBuild = false;
                yield break;
            }
            yield return null;
        }
    }
    public void BuilderMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetposition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetposition) <= 0.01)
        {
            transform.position = targetposition;
            isMoving = false;
        }
    }

    [Client]
    private void TargerSet()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CustomAllStopCo();
            isGoBuild = false;
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, groundLayer ))
            {
                Debug.Log(hit.transform.gameObject.layer);
                targetposition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                transform.LookAt(targetposition);
            }
            //Ÿ�ټ����ϰ� �����̶�� ���. �Ұ��ٲ���
            isMoving = true;
        }

    }
    [Client]
    private void CliTag(GameObject builder, string tag)
    {
        CMDTag(builder, tag);
    }
    #endregion
    #region Command

    [Command]
    private void CMDTag(GameObject builder, string tag)
    {
        builder.tag = tag;
        RPCTag(builder, tag);
    }
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPCTag(GameObject builder, string tag)
    {
        builder.tag = tag;
    }
    #endregion




}
