using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[System.Serializable]
public class CardData
{
    private int cardNum;
    private Sprite cardImage;
    private string cardName;

    private int positiveNum;
    private int negativeNum;

    private string description;

    public CardData(int cardNum, Sprite cardImage, string cardName, int positiveNum, int negativeNum, string description = null)
    {
        this.CardNum = cardNum;
        this.CardImage = cardImage;
        this.CardName = cardName;
        this.PositiveNum = positiveNum;
        this.NegativeNum = negativeNum;
        this.Description = description;
    }

    public int CardNum { get => cardNum; set => cardNum = value; }
    public Sprite CardImage { get => cardImage; set => cardImage = value; }
    public string CardName { get => cardName; set => cardName = value; }
    public int PositiveNum { get => positiveNum; set => positiveNum = value; }
    public int NegativeNum { get => negativeNum; set => negativeNum = value; }
    public string Description { get => description; set => description = value; }
}