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
                // ivisibleControl이 null이 아닌 경우 DisInvisible() 메서드를 호출하고, 그렇지 않으면 아무것도 수행하지 않습니다.
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
                // ivisibleControl이 null이 아닌 경우 DisInvisible() 메서드를 호출하고, 그렇지 않으면 아무것도 수행하지 않습니다.
                ivisibleControl?.OnInvisible();
            }
        }
    }
}
