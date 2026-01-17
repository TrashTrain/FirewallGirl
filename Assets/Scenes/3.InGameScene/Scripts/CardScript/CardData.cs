using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[System.Serializable]
public class CardData
{
    private int cardIndex;
    private Sprite cardImage;
    private string cardName;

    private StatType positiveStatType;
    private StatType negativeStatType;

    private int positiveStat;
    private int negativeStat;

    private int cost;

    private string description;

    // public CardData(int cardNum, Sprite cardImage, string cardName, int positiveNum, int negativeNum, int costNum, string description = null)
    // {
    //     this.CardNum = cardNum;
    //     this.CardImage = cardImage;
    //     this.CardName = cardName;
    //     this.PositiveNum = positiveNum;
    //     this.NegativeNum = negativeNum;
    //     this.Description = description;
    //     this.CostNum = costNum;
    // }

    public CardData(int cardIndex, string cardName, Sprite cardImage, StatType positiveStatType, StatType negaStatType,
        int positiveStat, int negativeStat, int cost, string description = null)
    {
        this.CardIndex = cardIndex;
        this.CardName = cardName;
        this.CardImage = cardImage;
        this.PositiveStatType = positiveStatType;
        this.NegativeStatType = negaStatType;
        this.PositiveStat = positiveStat;
        this.NegativeStat = negativeStat;
        this.Cost = cost;
        this.Description = description;
    }

    public int CardIndex { get => cardIndex; set => cardIndex = value; }
    public Sprite CardImage { get => cardImage; set => cardImage = value; }
    public string CardName { get => cardName; set => cardName = value; }
    public StatType PositiveStatType { get => positiveStatType; set => positiveStatType = value; }
    public StatType NegativeStatType { get => negativeStatType; set => negativeStatType = value; }
    public int PositiveStat { get => positiveStat; set => positiveStat = value; }
    public int NegativeStat { get => negativeStat; set => negativeStat = value; }
    public int Cost { get => cost; set => cost = value; }
    public string Description { get => description; set => description = value; }
}
