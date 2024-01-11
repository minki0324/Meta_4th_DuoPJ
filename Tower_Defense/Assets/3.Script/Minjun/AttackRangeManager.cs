using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeManager : MonoBehaviour
{
    private Tower tower;
   
    private void Awake()
    {
        transform.root.TryGetComponent(out tower);
    }
    private void OnEnable()
    {
        RangeSet(tower.RealRange);
    }
    private void Start()
    {
        RangeSet(tower.RealRange);
    }
    public void RangeSet(float range)
    {
        if (range == 0) return;
        Vector3 scale = new Vector3(range * 2f, transform.localScale.y, range * 2f);
        transform.localScale = scale;
    }
}
