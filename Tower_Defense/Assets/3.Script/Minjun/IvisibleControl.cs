using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IvisibleControl : MonoBehaviour
{
    private Monster_Control Monster;
    [SerializeField]private SkinnedMeshRenderer _renderer;
    [SerializeField] private Material[] Disinvi_materials;
    [SerializeField] private Material[] invi_materials;
    private void Awake()
    {
        TryGetComponent(out Monster);
    }

    private void OnEnable()
    {
        OnInvisible();
    }
    private void Update()
    {
       
    }

    public void OnInvisible()
    {
        _renderer.materials = new Material[0];
        _renderer.materials = invi_materials;
        Monster.isInvi = true;
    }
    public void DisInvisible()
    {
        _renderer.materials = new Material[0];
        _renderer.materials = Disinvi_materials;
        Monster.isInvi = false;

    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    DisInvisible();
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    OnInvisible();
    //}

}
