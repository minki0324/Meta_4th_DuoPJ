using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Head", menuName = "Scriptble Object/HeadData")]
public class Head_Data : ScriptableObject
{
    public enum Weapon_Type { Targeting, Splash }
    public enum Atk_Area { Ground, Air, All}
    public enum Atk_Type
    {
        Vulcan,
        Sniper,
        Laser,
        Missile,
        Seeker,
        Air,
        Flame,
        PlasmaBeam,
        LaserImpulse
    }

    [Header("기본 정보")]
    public Weapon_Type weapon_Type;
    public Atk_Area atk_Area;
    public Atk_Type atk_Type;
    public int BaseID;
    public int Level;
    public Vector3 Position;
    public float DelayTime;

    [Header("레벨 정보")]
    public int Cost;
    public float Rot_Speed;
    public float Damage;
    public float ATK_Speed;
    public float ATK_Range;
    public int Reload;
}
