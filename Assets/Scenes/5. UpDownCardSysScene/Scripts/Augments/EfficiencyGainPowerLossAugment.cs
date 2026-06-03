using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Augments/EfficiencyGainPowerLoss")]
public class EfficiencyGainPowerLossAugment : AugmentBase
{
    [Header("설정 수치")]
    public int costReduction = 1;
    public float powerMultiplier = 0.5f; // 반감 (50%)

    public int posChangeValue;
    public int negChangeValue;

    public override void OnEquip(BattleContext context)
    {
        List<CardObject> deck = CardDatabaseManager.instance.GetCurrentDeck();

        if (deck == null || deck.Count == 0)
        {
            Debug.LogWarning("적용할 덱이 없습니다.");
            return;
        }

        foreach (CardObject cardData in deck)
        {
            // 1. 코스트 감소 (최소 0 유지)
            cardData.cost = Mathf.Max(0, cardData.cost - costReduction);

            // 2. 긍정/부정 효과 수치 반감
            // int형이므로 반올림(Mathf.RoundToInt) 또는 내림을 선택할 수 있습니다.
            cardData.positiveStatValue = Mathf.RoundToInt(cardData.positiveStatValue * powerMultiplier);
            cardData.negativeStatValue = Mathf.RoundToInt(cardData.negativeStatValue * powerMultiplier);
        }

       

        Debug.Log($"{augmentName} 적용 완료: {deck.Count}장의 카드가 조정되었습니다.");

        // UI 갱신이 필요할 경우 호출 (예: 현재 핸드에 있는 카드들)
        context.player.UpdateUI();
    }
}