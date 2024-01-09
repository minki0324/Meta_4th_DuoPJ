using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scan : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            Monster_Control mon = other.GetComponent<Monster_Control>();
            if (mon.isDie) return;
            if (mon.state.type == MonsterState.monType.Invisible)
            {

                IvisibleControl ivisibleControl = other/*.transform.root*/.GetComponent<IvisibleControl>();
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
                // ivisibleControl�� null�� �ƴ� ��� DisInvisible() �޼��带 ȣ���ϰ�, �׷��� ������ �ƹ��͵� �������� �ʽ��ϴ�.
                ivisibleControl?.OnInvisible();
            }
        }
    }
}
