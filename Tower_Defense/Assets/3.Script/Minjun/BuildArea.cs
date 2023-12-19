using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildArea : MonoBehaviour
{
    private BuildManager buildManager;


    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;
    [SerializeField] private LayerMask obstacleLayer; //레이어마스크
 
   


    private void OnTriggerStay(Collider other)
    {
        //지정한장애물(몬스터,타워) 와 other의 레이어가 동일한경우 == 건물을 못짓는다.
        if ((obstacleLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            //적색으로 바꿔주기
            GetComponent<Renderer>().material = invalidMaterial;
            BuildManager.Instance.isCanBuild = false;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        //녹색으로 바꿔주기
        GetComponent<Renderer>().material = validMaterial;
        BuildManager.Instance.isCanBuild = true;
    }
}