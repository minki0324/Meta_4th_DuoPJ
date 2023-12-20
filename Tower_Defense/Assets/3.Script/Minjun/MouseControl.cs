using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetTargetInfo();
        }

       
    }

    private void GetTargetInfo()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            int TargetLayer = hit.collider.gameObject.layer;
            if (TargetLayer == LayerMask.NameToLayer("Monster"))
            {
                Monster_Control mon = hit.collider.gameObject.GetComponent<Monster_Control>();
                MarkerController marker = hit.collider.gameObject.GetComponent<MarkerController>();
                marker.Selectunit();
                Debug.Log("몬스터의 현재체력 : " + mon.state.currentHP);
                Debug.Log("몬스터의 공격력 : " + mon.state.currentHP);
                Debug.Log("몬스터의 이동속도 : " + mon.state.speed);

            }
            //else if(TargetLayer == LayerMask.NameToLayer("Tower"))
        }
    }
}
