using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanTower : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Invisible"))
        {
            Debug.Log("인비들어와용");
            IvisibleControl ivisibleControl = other.transform.root.GetComponent<IvisibleControl>();
        // ivisibleControl이 null이 아닌 경우 DisInvisible() 메서드를 호출하고, 그렇지 않으면 아무것도 수행하지 않습니다.
            ivisibleControl?.DisInvisible();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Invisible"))
        {
            IvisibleControl ivisibleControl = other.transform.root.GetComponent<IvisibleControl>();
            // ivisibleControl이 null이 아닌 경우 DisInvisible() 메서드를 호출하고, 그렇지 않으면 아무것도 수행하지 않습니다.
            ivisibleControl?.OnInvisible();
        }
    }
}
