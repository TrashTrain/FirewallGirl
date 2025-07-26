using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CostUI : MonoBehaviour
{
    public TextMeshProUGUI costText;
    
    void Start()
    {
        costText.text = "0/0";
    }
    
    public void UpdateCostUI(int current, int total)
    {
        costText.text = string.Format("{0}/{1}", current.ToString(), total.ToString());
    }
}
