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
    public float M_maxHp;
    public float M_currentHP;
    public float M_speed;
    public float M_damage;
    public float M_cost;
    [SerializeField]
    public GameObject marker;

    #region Unity Callback
    private void Start()
    {
        if(state.type != MonsterState.monType.Fly) {
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
    public void Init_Data(MonsterState monsterState)
    {
        M_maxHp = monsterState.maxHp;
        M_currentHP = monsterState.currentHP;
        M_speed = monsterState.speed;
        M_damage = monsterState.damage;
        M_cost = monsterState.cost;
    public void Selectunit()
    {
        marker.SetActive(true);
    }

    public void DeSelectunit()
    {
        marker.SetActive(false);
    }
}
