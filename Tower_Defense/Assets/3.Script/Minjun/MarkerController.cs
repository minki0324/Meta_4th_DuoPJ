using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    [SerializeField]
    public GameObject marker;

    public void Selectunit()
    {
        marker.SetActive(true); 
    }

    public void DeSelectunit()
    {
        marker.SetActive(false);
    }
}
