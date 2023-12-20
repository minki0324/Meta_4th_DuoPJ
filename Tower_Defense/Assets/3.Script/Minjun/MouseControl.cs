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
                Debug.Log("������ ����ü�� : " + mon.state.currentHP);
                Debug.Log("������ ���ݷ� : " + mon.state.currentHP);
                Debug.Log("������ �̵��ӵ� : " + mon.state.speed);

            }
            //else if(TargetLayer == LayerMask.NameToLayer("Tower"))
        }
    }
}
