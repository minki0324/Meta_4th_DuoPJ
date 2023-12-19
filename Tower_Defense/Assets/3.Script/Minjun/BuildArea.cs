using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildArea : MonoBehaviour
{
    private BuildManager buildManager;


    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;
    [SerializeField] private LayerMask obstacleLayer; //���̾��ũ
    [SerializeField] private LayerMask buildAreaLayer; //���̾��ũ
    private Vector3 rayStartPos;
    public bool isCanBuild;
    private void OnTriggerStay(Collider other)
    {
        //��������ֹ�(����,Ÿ��) �� other�� ���̾ �����Ѱ�� == �ǹ��� �����´�.
        if ((obstacleLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            //�������� �ٲ��ֱ�
            onRed();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        onGreen();
    }
    private void onGreen()
    {
        GetComponent<Renderer>().material = validMaterial;
        BuildManager.Instance.isCanBuild = true;
    }
    private void onRed()
    {
        GetComponent<Renderer>().material = invalidMaterial;
        BuildManager.Instance.isCanBuild = false;
    }
}