using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildArea : MonoBehaviour
{
    private BuildManager buildManager;


    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;
    [SerializeField] private LayerMask obstacleLayer; //���̾��ũ
 
   


    private void OnTriggerStay(Collider other)
    {
        //��������ֹ�(����,Ÿ��) �� other�� ���̾ �����Ѱ�� == �ǹ��� �����´�.
        if ((obstacleLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            //�������� �ٲ��ֱ�
            GetComponent<Renderer>().material = invalidMaterial;
            BuildManager.Instance.isCanBuild = false;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        //������� �ٲ��ֱ�
        GetComponent<Renderer>().material = validMaterial;
        BuildManager.Instance.isCanBuild = true;
    }
}