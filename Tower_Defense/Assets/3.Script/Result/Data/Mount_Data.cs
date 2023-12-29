using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mount", menuName = "Scriptble Object/MountData")]
public class Mount_Data : ScriptableObject
{
    [Header("�⺻ ����")]
    public string name_;
    public int BaseID;
    public int Level;
    public Vector3 Position;

    [Header("���� ����")]
    public int Cost;
    public float Rot_Speed;
}
