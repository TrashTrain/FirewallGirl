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
        PlayerManager player =  PlayerManager.instance;
        player.costUI = this;
        costText.text = string.Format("{0}/{1}", player.currentCost, player.totalCost);
    }
    
    public void UpdateCostUI(int current, int total)
    {
        costText.text = string.Format("{0}/{1}", current.ToString(), total.ToString());
    }
}
