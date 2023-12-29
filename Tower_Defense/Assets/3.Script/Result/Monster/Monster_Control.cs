using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Pathfinding;

public class Monster_Control : NetworkBehaviour
{
    public MonsterState state;
    public AIDestinationSetter Astar;
    public AIPath aiPath;
    private NetworkAnimator netAni;
    //[SerializeField] private Material DieMaterial;
    [SyncVar]
    public float M_maxHp;
    [SyncVar]
    public float M_currentHP;
    [SyncVar]
    public float M_speed;
    [SyncVar]
    public float M_damage;
    [SyncVar]
    public float M_cost;
    [SyncVar]
    public bool isInvi;
    [SyncVar]
    public bool isDie;
    [SerializeField]
    public GameObject marker;

    #region Unity Callback
    private void Start()
    {
        netAni = GetComponent<NetworkAnimator>();

        if (state.type != MonsterState.monType.Fly) {
            TryGetComponent(out aiPath);
            aiPath.maxSpeed = state.speed;
        }
        else
        {

        }
    }

    private void OnEnable()
    {
        Init_Data(state);

    }

    private void Update()
    {

        //�״°� üũ��
        //CheckDie();

    }
    #endregion
    #region SyncVar
    [SyncVar]
    public int goalCount = 0;
    #endregion
    #region Client
    #endregion
    #region Command
    #endregion
    #region ClientRPC
    #endregion
    #region Hook Method
    #endregion
   
    
    private void CheckDie(float oldHP , float newHP)
    {
        if (!isDie && isServer)
        {
            if (newHP <= 0)
            {
                StartCoroutine(onDie());
            }

        }
    }
    public void Init_Data(MonsterState monsterState)
    {
        M_maxHp = monsterState.maxHp;
        M_currentHP = monsterState.currentHP;
        M_speed = monsterState.speed;
        M_damage = monsterState.damage;
        M_cost = monsterState.cost;
        isDie = false;
    }
    public void Selectunit()
    {
        marker.SetActive(true);
    }

    public void DeSelectunit()
    {
        marker.SetActive(false);
    }
    public IEnumerator onDie()
    {
        //���̽�Ÿ ����    ���
        //���¸��� �ٲٱ�  ��� ����
        //�ִϸ��̼� ����  ���
        //Active false     1��
        if (isServer)
        {
            isDie = true;
            Astar.target = null;
            aiPath.isStopped = true;
            netAni.SetTrigger("Die");
        }

        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);

    }
}
