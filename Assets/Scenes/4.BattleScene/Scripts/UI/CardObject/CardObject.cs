using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="CardData", menuName="Create Card Data/CardData", order=int.MaxValue)]

public class CardObject : ScriptableObject
{
    public int cardIndex;
    public string cardName;
    public Sprite cardImage;

    public StatType positiveStatType;
    public StatType negativeStatType;

    public int positiveStatValue;
    public int negativeStatValue;
    
    public int cost;
    
    [TextArea]
    public string description;
}
