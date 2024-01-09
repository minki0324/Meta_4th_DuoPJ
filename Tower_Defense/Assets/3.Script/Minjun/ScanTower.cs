using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanTower : MonoBehaviour
{
    [SerializeField] private Transform mount;

    private void Start()
    {
        mount = transform.root.GetChild(1);
    }
    private void Update()
    {
        mount.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
    }
   
}
