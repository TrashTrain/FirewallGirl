using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Augments/CardStatAverage")]
public class CardStatAverageAugment : AugmentBase
{
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
            int posMagnitude = Mathf.Abs(cardData.positiveStatValue);
            int negMagnitude = Mathf.Abs(cardData.negativeStatValue);
            
            int totalStats = posMagnitude + negMagnitude;
            int average = totalStats / 2;
            
            cardData.positiveStatValue = average;
            cardData.negativeStatValue = -average;
        }
        
        Debug.Log($"덱에 있는 {deck.Count}장 카드의 긍정/부정 스탯 수치가 서로 교환되었습니다!");
    }
}
