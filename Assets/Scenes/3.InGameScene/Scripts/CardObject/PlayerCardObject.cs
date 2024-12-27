using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    ATK, DEF, SUP
}

[CreateAssetMenu(fileName = "CardData", menuName = "CreateCardData/CardData")]
public class PlayerCardObject : ScriptableObject
{
    public Sprite cardImage;
    public string cardName;
    
    public int positiveNum;
    public int negativeNum;

    [TextArea]
    public string description;
    public CardType type;
}
