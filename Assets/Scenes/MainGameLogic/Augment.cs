using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AugmentRank { Common, Rare, Unique, Epic }

[Serializable]
public class AugmentData
{
    public string id;              // ??? ?????? ???? ID (???)
    public string augmentName;
    [TextArea] public string augmentDesc;
    public AugmentRank augmentRank;
    public int positiveValue;
    public int negativeValue;
}


public class AugmentDeck
{
    public List<AugmentData> augmentDatas;

}

// ????? ???? ??????.
public class Augment : MonoBehaviour
{
    
    [SerializeField] private string augmentName;
    [SerializeField] private string augmentDesc;
    [SerializeField] private AugmentRank augmentRank;
    [SerializeField] private int positiveValue;
    [SerializeField] private int negativeValue;

    private AugmentDeck augmentDeck;

    public Augment (string name, string desc, AugmentRank rank, int positive, int negative)
    {
        augmentName = name;
        augmentDesc = desc;
        augmentRank = rank;
        positiveValue = positive;
        negativeValue = negative;
    }

    public void AddAugmentDeck(AugmentData data)
    {
        augmentDeck.augmentDatas.Add(data);
    }


    public int positive
    {
        get { return positiveValue; }
        set { positiveValue = value; }
    }

    public int negative
    {
        get { return negativeValue; }
        set { negativeValue = value; }

    }
}
