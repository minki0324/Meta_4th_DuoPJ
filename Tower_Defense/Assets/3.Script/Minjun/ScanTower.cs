using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanTower : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Invisible"))
        {
            Debug.Log("�κ���Ϳ�");
            IvisibleControl ivisibleControl = other.transform.root.GetComponent<IvisibleControl>();
        // ivisibleControl�� null�� �ƴ� ��� DisInvisible() �޼��带 ȣ���ϰ�, �׷��� ������ �ƹ��͵� �������� �ʽ��ϴ�.
            ivisibleControl?.DisInvisible();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Invisible"))
        {
            IvisibleControl ivisibleControl = other.transform.root.GetComponent<IvisibleControl>();
            // ivisibleControl�� null�� �ƴ� ��� DisInvisible() �޼��带 ȣ���ϰ�, �׷��� ������ �ƹ��͵� �������� �ʽ��ϴ�.
            ivisibleControl?.OnInvisible();
        }
    }
}
