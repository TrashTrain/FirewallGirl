using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Augments/CardStatChange")]
public class CardStatChangeAugment : AugmentBase
{
    public int posChangeValue;
    public int negChangeValue;

    public override void OnEquip(BattleContext context)
    {
        List<CardObject> deck = CardDatabaseManager.instance.GetCurrentDeck();
        
        if (deck == null || deck.Count == 0)
        {
            Debug.LogWarning("덱이 비어있거나 찾을 수 없어 증강체를 적용할 수 없습니다.");
            return;
        }
        
        foreach (CardObject cardData in deck)
        {
            cardData.positiveStatValue += posChangeValue;
            cardData.negativeStatValue -= negChangeValue;
        }
        
        Debug.Log($"덱에 있는 {deck.Count}장 카드의 스탯이 영구 변경되었습니다!");
    }
}
