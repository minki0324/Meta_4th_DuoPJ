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
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            Monster_Control mon = other.GetComponent<Monster_Control>();
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
