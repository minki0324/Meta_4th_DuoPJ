using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Tower : NetworkBehaviour
{
    [SerializeField]
    public GameObject marker;
    [SerializeField] private Material holoColor;
    public float maxHP;
    public float currentHP;
    public float damage;
    public float range;
    public float atkSpeed;
    public string Speed;
    public int level;
    public Sprite unitSprite;
    
    private void Awake()
    {
        maxHP = 50;
        currentHP = Random.Range(0 ,51);
        damage = 5;
        range = 6;
        atkSpeed = 7;
        Speed = "-";
        level = 9;
    }


    private void Update()
    {
     
    Vector3 currentEulerAngles = marker.transform.eulerAngles;
        currentEulerAngles.y += Time.deltaTime * 65; // 회전 속도를 조절할 수 있습니다.
        marker.transform.eulerAngles = currentEulerAngles;
    }
    public void Selectunit()
    {
        marker.SetActive(true);
    }

    public void DeSelectunit()
    {
        marker.SetActive(false);
    }
    public void HologramTower(GameObject gameObject)
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer )
        {
            meshRenderer.materials = new Material[0];
            meshRenderer.material = holoColor;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        foreach (Transform child in gameObject.transform)
        {
            HologramTower(child.gameObject);
        }
    }
}
