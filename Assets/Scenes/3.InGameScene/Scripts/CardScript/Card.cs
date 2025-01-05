using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int cardNum;
    public SpriteRenderer cardImage;
    public TextMeshPro cardName;

    public TextMeshPro positiveNum;
    public TextMeshPro negativeNum;

    public TextMeshPro costNum;

    public TextMeshPro description;


    public void Start()
    {
        //dataset.data.FindIndex(card => card.CardNum.Equals(cardNum));
        CardData cardData = CardMgr.instance.cardDatas.Find(card => card.CardNum == cardNum);
        cardImage.sprite = cardData.CardImage;
        cardName.text = cardData.CardName;
        positiveNum.text = cardData.PositiveNum.ToString();
        negativeNum.text = cardData.NegativeNum.ToString();
        costNum.text = cardData.CostNum.ToString();

        // 지금은 설명칸이 비어있어서 빼놓음.
        //description.text = cardData.Description;
    }
}
