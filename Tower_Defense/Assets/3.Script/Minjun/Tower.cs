using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Tower : NetworkBehaviour
{
    [SerializeField]
    public GameObject marker;
    private void Update()
    {
     
    Vector3 currentEulerAngles = marker.transform.eulerAngles;
        currentEulerAngles.y += Time.deltaTime * 65; // 회전 속도를 조절할 수 있습니다.
        marker.transform.eulerAngles = currentEulerAngles;
    }
    public void Selectunit()
    {
        marker.SetActive(true);
    }

    public void DeSelectunit()
    {
        marker.SetActive(false);
    }
}
