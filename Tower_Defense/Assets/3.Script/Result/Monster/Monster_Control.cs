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
    public void Selectunit()
    {
        marker.SetActive(true);
    }

    public void DeSelectunit()
    {
        marker.SetActive(false);
    }
}
