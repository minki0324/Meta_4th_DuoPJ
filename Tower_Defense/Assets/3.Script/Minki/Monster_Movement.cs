using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Mirror;

public class Monster_Movement : NetworkBehaviour
{
 
    //�ö��� ���� �����Ʈ�Դϴ�

    //public monType type;
    private void Start()
    {
        
    }
    private void Update()
    {
        //if(type == monType.Fly)
        transform.position += -Vector3.forward * 5 * Time.deltaTime;

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
