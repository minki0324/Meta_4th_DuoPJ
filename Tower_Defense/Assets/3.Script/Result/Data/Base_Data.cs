using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Base", menuName = "Scriptble Object/BaseData")]
public class Base_Data : ScriptableObject
{
    [Header("�⺻ ����")]
    public int BaseID;
    public int Level;

    [Header("���� ����")]
    public int Cost;
    public int Health;

}
