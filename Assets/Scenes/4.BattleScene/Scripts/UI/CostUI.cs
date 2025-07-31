using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CostUI : MonoBehaviour
{
    public TextMeshProUGUI costText;
    public PlayerManager playerManager;
    
    void Start()
    {
        costText.text = string.Format("{0}/{1}", playerManager.currentCost, playerManager.totalCost);
    }
    
    public void UpdateCostUI(int current, int total)
    {
        costText.text = string.Format("{0}/{1}", current.ToString(), total.ToString());
    }
}
