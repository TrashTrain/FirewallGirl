using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Augments/ZeroCostGamble")]
public class ZeroCostGambleAugment : AugmentBase
{
    private bool alreadyApplied = false;

    public override void OnEquip(BattleContext context)
    {
        if (alreadyApplied)
            return;

        List<CardObject> deck = CardDatabaseManager.instance.GetCurrentDeck();

        if (deck == null || deck.Count == 0)
        {
            Debug.LogWarning("적용할 카드가 없습니다.");
            return;
        }

        int affectedCards = 0;
        int highCostCardCount = 0;

        foreach (CardObject cardData in deck)
        {
            // 코스트 0 카드의 긍정 효과 2배
            if (cardData.cost == 0)
            {
                cardData.positiveStatValue *= 2;
                affectedCards++;
            }

            // 코스트 3 이상 카드 개수 카운트
            if (cardData.cost >= 3)
            {
                highCostCardCount++;
            }
        }

        // (코스트 3 이상 카드 개수) × 3 만큼 현재 체력 감소
        int hpPenalty = highCostCardCount * 3;

        if (context.player != null && hpPenalty > 0)
        {
            context.player.currentHP =
                Mathf.Max(1, context.player.currentHP - hpPenalty);
        }

        alreadyApplied = true;

        // UI 반영
        if (context.player != null)
        {
            context.player.UpdateUI();
        }

        Debug.Log(
            $"{augmentName} 적용 완료\n" +
            $"코스트 0 카드 {affectedCards}장의 긍정 효과가 2배가 되었습니다.\n" +
            $"코스트 3 이상 카드 {highCostCardCount}장 발견\n" +
            $"현재 체력 {hpPenalty} 감소\n" +
            $"현재 체력: {context.player.currentHP}/{context.player.MaxHP}"
        );
    }
}