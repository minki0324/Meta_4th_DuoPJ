using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Base", menuName = "Scriptble Object/BaseData")]
public class Base_Data : ScriptableObject
{
    [Header("�⺻ ����")]
    public string name_;
    public int BaseID;
    public int Level;
    public string BuildArea;
    public int BuildAreaIndex;

    [Header("���� ����")]
    public int Cost;
    public float Health;

}
