using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName="CardData", menuName="Create Card Data/CardData", order=int.MaxValue)]

public class CardObject : ScriptableObject
{
    public int cardIndex;
    public string cardName;
    public Sprite cardImage;

    public int attackPower;
    public int defensePower;
    public int healthPoint;
    public int cost;
    
    [TextArea]
    public string description;
}
