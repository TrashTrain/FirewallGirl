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

    private void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // 디버깅용
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
        hpBar.UpdateHPBar(currentHP, maxHP);
        powerUI.UpdateAttackPowerUI(attackPower);
        powerUI.UpdateDefensePowerUI(defensePower);
        costUI.UpdateCostUI(currentCost, totalCost);
    }
}
