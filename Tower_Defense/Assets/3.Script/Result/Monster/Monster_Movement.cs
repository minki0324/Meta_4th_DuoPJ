using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Mirror;

public class Monster_Movement : NetworkBehaviour
{

    //�ö��� ���� �����Ʈ�Դϴ�
    private Monster_Control mon;

    //public monType type;
    private void Start()
    {
        TryGetComponent(out mon);
    }
    private void Update()
    {
        if (!mon.isDie)
        {

            transform.position += -Vector3.forward * 5 * Time.deltaTime;
        }

    }
    [Client]
    private void CliMove()
    {
        Move();
    }

    [Server]
    private void Move()
    {
    
            
    }

}
