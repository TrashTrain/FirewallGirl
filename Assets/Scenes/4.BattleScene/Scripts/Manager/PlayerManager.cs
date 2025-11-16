using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Player info
    public int maxHP;
    public int currentHP;
    public int currentCost;
    public int totalCost;
    public int attackPower = 0;
    public int defensePower = 0;

    public HealthBar hpBar;
    public PowerUI powerUI;
    public CostUI costUI;

    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
        if (instance == null)
            instance = new PlayerManager();
        UpdateUI();
    }

    private void Start()
    {
        
    }
    public void TakeDamage(int damage)
    {
        // 디버깅용
        Debug.Log("PlayerHP : " + currentHP);
        Debug.Log("PlayerTakeDamage : " + damage);
        currentHP = Mathf.Max(0, currentHP - damage);
        UpdateUI();
    }

    public void TakeCost()
    {
        currentCost = Mathf.Max(0, currentCost - 1);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (hpBar == null) {
            Debug.LogError("없음");
        }
        hpBar.UpdateHPBar(currentHP, maxHP);
        powerUI.UpdateAttackPowerUI(attackPower);
        powerUI.UpdateDefensePowerUI(defensePower);
        costUI.UpdateCostUI(currentCost, totalCost);
    }
}
