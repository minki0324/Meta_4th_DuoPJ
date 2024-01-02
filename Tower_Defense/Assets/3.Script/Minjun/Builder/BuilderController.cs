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
    [SyncVar]
    public bool isSelectBuilder;
    [SyncVar]
    public bool isMoving;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isSelectBuilder) {
            TargerSet();
        }
          
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetposition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetposition) <= 0.01)
            {
                transform.position = targetposition;
                isMoving = false;
            }
        }
        //BuilderMove();
    }
    [Client]
    public void BuildOrder()
    {
        //targetposition 설정해주고 distance가 어느정도 가까워지면 빌드명령하기
        if (Vector3.Distance(transform.position, targetposition) <= 1.5f)
        {
            //BuildManager.Instance.buol
            isMoving = false;
        }

    }
    [Client]
    public void BuilderMove()
    {
        CMDMove();

       
    }
    [Command]
    private void CMDMove()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetposition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetposition) <= 0.01)
            {
                transform.position = targetposition;
                isMoving = false;
            }
        }
    }

    [Client]
    private void TargerSet()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                targetposition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            }
            //타겟설정하고 움직이라고 명령. 불값바꿔줌
            isMoving = true;
        }
       
    }
}
