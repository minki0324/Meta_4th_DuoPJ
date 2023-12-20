using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;

    private Camera maincamera;
        private RTSControlSystem rts;

    private void Awake()
    {
        maincamera = Camera.main;
        rts = GetComponent<RTSControlSystem>();
    }


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
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity , targetLayer))
        {
            if (hit.transform.parent.GetComponent<Tower>() == null) return;

            if (Input.GetKey(KeyCode.LeftShift)){
                rts.ShiftClickSelectUnit(hit.transform.parent.GetComponent<Tower>());
            }
            else
            {
                rts.ClickSelectUnit(hit.transform.parent.GetComponent<Tower>());
            }


            //int TargetLayer = hit.collider.gameObject.layer;
            //if (TargetLayer == LayerMask.NameToLayer("Monster"))
            //{
            //    Tower mon = hit.collider.gameObject.GetComponent<Tower>();
            //    mon.Selectunit();
            //    Debug.Log("������ ����ü�� : " + mon.state.currentHP);
            //    Debug.Log("������ ���ݷ� : " + mon.state.currentHP);
            //    Debug.Log("������ �̵��ӵ� : " + mon.state.speed);

            //}
           
            //else if(TargetLayer == LayerMask.NameToLayer("Tower"))
        }
        else
        {
            Debug.Log("Ÿ�� �Ȱ���");
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                rts.DeSelectAll();
            }
        }
    }
}
