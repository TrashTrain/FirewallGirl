using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UpDownMgr;

public class PlayerManager : MonoBehaviour
{
    // Player info
    public int maxHP=100;
    public int currentHP;
    public int currentCost = 10;
    public int totalCost;
    public int attackPower = 10;
    public int defensePower = 10;
    public float evasionRate = 0.1f;

    public HealthBar hpBar;
    public PowerUI powerUI;
    public CostUI costUI;

    public static PlayerManager instance;
    UpDownMgr upDownInfo = UpDownMgr.instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // 씬이 바뀌어도 파괴되지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 인스턴스가 존재하면 새로운 인스턴스는 파괴 (중복 방지)
            Destroy(gameObject);
        }
        currentHP = maxHP;
    }

    private void Start()
    {
        // 씬 로드 시 초기 HP 설정 (Awake에서 DontDestroyOnLoad 후 Start 시점에 한번 초기화)
        if (currentHP == 0) currentHP = maxHP;

        UpdateUI();
    }

    // UpDownMgr로부터 스탯 변화를 받아 적용하는 함수
    public void ApplyStatChange(GenerateCard posCard, GenerateCard negCard)
    {
        ApplySingleStatChange(posCard);
        ApplySingleStatChange(negCard);

        UpdateUI(); // 스탯 변경 후 UI 업데이트
    }

    // 단일 GenerateCard를 적용하는 내부 로직
    private void ApplySingleStatChange(GenerateCard card)
    {
        switch (card.stat)
        {
            case StatType.Attack:
                attackPower += card.GetSignedValue();
                break;
            case StatType.Defense:
                defensePower += card.GetSignedValue();
                break;
            case StatType.Cost:
                // 코스트 증가는 TotalCost(최대치)에 영향을 줄 수 있습니다.
                totalCost += card.GetSignedValue();
                currentCost = totalCost; // 코스트 최대치가 바뀌면 현재 코스트를 최대치로 리필 (게임 디자인에 따라 변경 가능)
                break;
            case StatType.Health:
                int healthChange = card.GetSignedValue();
                // 현재 HP를 변경하되, maxHP (100)를 초과하지 않도록 Clamp
                currentHP = Mathf.Clamp(currentHP + healthChange, 1, maxHP);
                break;
            case StatType.Evasion:
                // 회피율은 소수점 기반으로 처리한다고 가정
                // 1~3의 값이 왔다면, 1%~3%로 적용 (게임 디자인에 따라 변경)
                float percentageChange = card.GetSignedValue() * 0.01f;
                evasionRate = Mathf.Clamp(evasionRate + percentageChange, 0f, 0.9f); // 최대 90% 제한
                break;
        }

        Debug.Log($"Applied Stat: {card.stat} {card.ToString()}. Current Attack: {attackPower}, Total Cost: {totalCost}");
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
        if (hpBar != null)
        {
            hpBar.UpdateHPBar(currentHP, maxHP);
        }


        if (powerUI != null)
        {
            powerUI.UpdateAttackPowerUI(attackPower);
            powerUI.UpdateDefensePowerUI(defensePower);
        }

        if (costUI != null)
        {
            costUI.UpdateCostUI(currentCost, totalCost);
        }

    }
}
