using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Augments/ChargeAttack")]
public class ChargeAttackAugment : AugmentBase
{
    private bool alreadyApplied = false;

    public override void OnEquip(BattleContext context)
    {
        if (alreadyApplied)
            return;

        List<CardObject> deck = CardDatabaseManager.instance.GetCurrentDeck();

        if (deck == null || deck.Count == 0)
        {
            Debug.LogWarning("적용할 덱이 없습니다.");
            return;
        }

        foreach (CardObject cardData in deck)
        {
            // 공격력 증가 카드만 2배
            if (cardData.positiveStatType == StatType.Attack)
            {
                cardData.positiveStatValue *= 2;
            }

        }

        context.player.chargeAttackMode = true;
        context.player.chargeTurnReady = false;

        alreadyApplied = true;

        context.player.UpdateUI();

        Debug.Log(
            $"{augmentName} 적용 완료\n" +
            $"공격력 증가 카드 수치가 2배가 되었습니다."
        );
    }
}