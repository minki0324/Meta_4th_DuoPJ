using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despawn_audio : MonoBehaviour
{
    public float delay = 0.3f;

    private void OnEnable()
    {
        Invoke("Setfalse", delay);
    }

    private void Setfalse()
    {
        gameObject.SetActive(false);
    }
}
