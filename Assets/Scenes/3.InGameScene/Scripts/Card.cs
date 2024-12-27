using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public Sprite cardImage;
    public string cardName;

    public int positiveNum;
    public int negativeNum;

    [TextArea]
    public string description;
    public CardType type;

}
