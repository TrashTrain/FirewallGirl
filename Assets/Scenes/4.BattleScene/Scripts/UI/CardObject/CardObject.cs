using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    None,
    Vaccine,
    Patch,
    Root
}

[CreateAssetMenu(fileName = "CardData", menuName = "Create Card Data/CardData", order = int.MaxValue)]

public class CardObject : ScriptableObject
{
    public int cardIndex;
    public string cardName;
    public string cardNameEng;
    public Sprite cardImage;

    public StatType positiveStatType;
    public StatType negativeStatType;
    
    public CardType cardType = CardType.None; // 카드의 속성 (백신, 패치, 루트)

    public int positiveStatValue;
    public int negativeStatValue;
    
    public int cost;
    public int coolTime = 0;
    
    [TextArea]
    public string summaryDescription;
    
    [TextArea]
    public string description;
}
