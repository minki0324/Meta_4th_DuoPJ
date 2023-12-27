using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanTower : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            Monster_Control mon = other.GetComponent<Monster_Control>();
            if (mon.state.type == MonsterState.monType.Invisible)
            {

                Debug.Log("�κ���Ϳ�");
                IvisibleControl ivisibleControl = other/*.transform.root*/.GetComponent<IvisibleControl>();
                mon.isInvi = false;
                Debug.Log(ivisibleControl);
                // ivisibleControl�� null�� �ƴ� ��� DisInvisible() �޼��带 ȣ���ϰ�, �׷��� ������ �ƹ��͵� �������� �ʽ��ϴ�.
                ivisibleControl?.DisInvisible();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {

            Monster_Control mon = other.GetComponent<Monster_Control>();
            if (mon.state.type == MonsterState.monType.Invisible)
            {
                IvisibleControl ivisibleControl = other/*.transform.root*/.GetComponent<IvisibleControl>();
                mon.isInvi = true;
                // ivisibleControl�� null�� �ƴ� ��� DisInvisible() �޼��带 ȣ���ϰ�, �׷��� ������ �ƹ��͵� �������� �ʽ��ϴ�.
                ivisibleControl?.OnInvisible();
            }
        }
    }
}
