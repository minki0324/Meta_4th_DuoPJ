using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Control : MonoBehaviour
{
    public enum Effect_type
    {
        muzzle,
        projectile,
        impact
    }

    [SerializeField] private float delay;
    #region Unity Callback
    private void OnEnable()
    {
        Invoke("active", delay);
    }
    #endregion

    private void active()
    {
        gameObject.SetActive(false);
    }
}
