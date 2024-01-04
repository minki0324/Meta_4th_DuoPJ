using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMonster : MonoBehaviour
{

    //어택 전용 무브먼트입니다
    private Monster_Control mon;
    [SerializeField] private LayerMask towerLayer;
    public float AttackRange =10f;
    [SerializeField]private bool isAttack;
    [SerializeField]private Tower TargetTower;
    float AttackDelay =1000f;
    //public monType type;
    private void Start()
    {
        TryGetComponent(out mon);
    }
    private void Update()
    {
        if (mon.isDie) return;
        if (TargetTower != null)
        {

            isAttack = true;
        }
        else
        {
            isAttack = false;
        }
        if (!isAttack)
        {
            transform.position += -Vector3.forward * 5 * Time.deltaTime;
            TargetTower = DetectTarget();
        }
        else
        {
            if (AttackDelay > mon.state.attackSpeed)
            {
                TargetTower.currentHP -= mon.state.damage;
                AttackDelay = 0f;
            }
        }
        AttackDelay += Time.deltaTime;

    }
    private Tower DetectTarget()
    {
        Collider[] towers = Physics.OverlapSphere(transform.position, AttackRange, towerLayer);

        GameObject TempTower = null;
        float mindis = float.MaxValue;
        foreach (Collider tower in towers)
        {
            float distance = Vector3.Distance(tower.transform.position, transform.position);
            if (mindis > distance)
            {
                mindis = distance;
                TempTower = tower.gameObject;
            }

        }
        return (TempTower !=null) ? TempTower.transform.root.GetComponent<Tower>() : null;


    }
}
