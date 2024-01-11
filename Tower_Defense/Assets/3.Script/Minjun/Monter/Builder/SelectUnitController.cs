using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectUnitController : MonoBehaviour
{
    public enum CurrentUnit
    {
        Builder,
        Tower,
        Monster
    }
    [SerializeField] private GameObject builder;
    public Tower tower;
    public static SelectUnitController Instance;
    public CurrentUnit unit;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }


    private void UnitControl()
    {
        switch (unit)
        {
            case CurrentUnit.Builder:
                BuilderController();
                break;
            case CurrentUnit.Tower:
                TowerController();
                break;
            case CurrentUnit.Monster:
                BuilderController();
                break;
        }
    }

    private void TowerController()
    {
     
    }

    private void BuilderController()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

           

        }
    }
}
