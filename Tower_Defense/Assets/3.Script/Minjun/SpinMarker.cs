using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinMarker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Spin_Marker();
    }
    private void Spin_Marker()
    {

        Vector3 currentEulerAngles = transform.eulerAngles;
        currentEulerAngles.y += Time.deltaTime * 65; // 회전 속도를 조절할 수 있습니다.
        transform.eulerAngles = currentEulerAngles;
    }
}
