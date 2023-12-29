using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Base", menuName = "Scriptble Object/BaseData")]
public class Base_Data : ScriptableObject
{
    [Header("기본 정보")]
    public string name_;
    public int BaseID;
    public int Level;
    public string BuildArea;

    [Header("레벨 정보")]
    public int Cost;
    public int Health;

}
