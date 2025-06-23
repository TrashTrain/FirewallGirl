using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Player info
    public int maxHP;
    public int currentHP;
    public int cost;
    public int attackPower;
    public int defensePower;

    public HealthBar hpBar;
    public PowerUI powerUI;

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

    public void UpdateUI()
    {
        hpBar.UpdateHPBar(currentHP, maxHP);
        powerUI.UpdateAttackPowerUI(attackPower);
        powerUI.UpdateDefensePowerUI(defensePower);
    }
}
