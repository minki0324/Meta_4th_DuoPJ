using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class BuildManager : NetworkBehaviour
{
    public static BuildManager Instance;

    public GameObject pointPrefab; // 가상의 점을 나타낼 프리팹
    public bool isCanBuild;  // BuildArea에서 한개라도 적색으로 변할 시 false 반환함.
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        CreateVirtualPoints();
    }
    private void CreateVirtualPoints()
    {
        for (float x = -9f; x <= 13.5f; x += 1.5f)
        {
            for (float z = -9f; z <= 13.5f; z += 1.5f)
            {
                Vector3 position = new Vector3(x, 0.0f, z);
                Instantiate(pointPrefab, position, Quaternion.identity);
            }
        }
    }
}
