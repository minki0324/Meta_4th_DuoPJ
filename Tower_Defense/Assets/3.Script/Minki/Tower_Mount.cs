using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Tower_Mount : NetworkBehaviour
{
    public Mount_Data mount_Data;

    public int M_cost;
    public float M_Rot_Speed;

    #region Unity Callback
    private void Start()
    {
        Init_Data(mount_Data);
    }
    #endregion
    #region SyncVar
    #endregion
    #region Client
    #endregion
    #region Command
    #endregion
    #region ClientRPC
    #endregion
    #region Hook Method
    #endregion

    private void Init_Data(Mount_Data mount_Data)
    {
        M_cost = mount_Data.Cost;
        M_Rot_Speed = mount_Data.Rot_Speed;
    }
}
