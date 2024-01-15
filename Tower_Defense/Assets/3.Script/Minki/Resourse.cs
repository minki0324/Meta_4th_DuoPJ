using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Resourse : MonoBehaviour
{
    [SerializeField] private Text Mineral_txt;
    [SerializeField] private Text Crystal_txt;
    [SerializeField] private Text Food_txt;

    public int current_mineral = 100;
    public int current_crystal = 1;
    public int current_food = 0;
    public int max_food = 30;

    private void Update()
    {
        Print_Resourse();
    }

    private void Print_Resourse()
    {
        Mineral_txt.text = current_mineral.ToString("N0");
        Crystal_txt.text = current_crystal.ToString("N0");
        Food_txt.text = $"{current_food} / {max_food}";
    }
}
