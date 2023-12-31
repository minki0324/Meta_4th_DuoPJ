using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster" , menuName = "Scriptble Object/MonsterData")]
public class MonsterState : ScriptableObject
{
    public enum monType
    {
        basic,
        Strong,
        Fast,
        Fly,
        Attack,
        Invisible
    }
    public Sprite mon_img;

    public int MonsterID;
    public int level;
    public monType type;
    public float maxHp;
    public float currentHP;
    public float speed;
    public float damage;
    public float cost;
    public float attackSpeed;
    public bool isDie;

}
