using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
public class BuilderSpawn : NetworkBehaviour
{
    [SerializeField] private GameObject builder;
    [SerializeField] private Transform SpawnPoint;
    private Transform[] SpawnPoints;
    private Transform mySpawnPoint;
    private void Awake()
    {
        SpawnPoints = new Transform[SpawnPoint.childCount];
        for (int i = 0; i < SpawnPoint.childCount; i++)
        {
            SpawnPoints[i] = SpawnPoint.GetChild(i);
        }
      
    }
    private void Start()
    {
        if (!isServer && isLocalPlayer)
        {
            FindPoint();
            SpawnBuilder();
        }
    }
    private void FindPoint()
    {
        foreach (Transform point in SpawnPoint)
        {
            if (GameManager.instance.CompareEnumWithTag(point.gameObject.tag))
            {
                mySpawnPoint = point;
            }
        }
    }
    private void SpawnBuilder()
    {
        GameObject newBuilder = Instantiate(builder, mySpawnPoint.position, Quaternion.identity);
        newBuilder.tag = EnumtoTag();
        NetworkServer.Spawn(newBuilder);
    }
    private string EnumtoTag()
    {
        string enumString = GameManager.instance.Player_Num.ToString();

        // Enum 문자열에서 앞의 숫자를 추출합니다.
        string enumNumber = new string(enumString.Where(char.IsDigit).ToArray());
        string newenumString = enumNumber + 'P';
        return newenumString;
    }
}
