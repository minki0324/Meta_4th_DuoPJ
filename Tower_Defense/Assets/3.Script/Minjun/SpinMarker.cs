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
        currentEulerAngles.y += Time.deltaTime * 65; // ȸ�� �ӵ��� ������ �� �ֽ��ϴ�.
        transform.eulerAngles = currentEulerAngles;
    }
}
