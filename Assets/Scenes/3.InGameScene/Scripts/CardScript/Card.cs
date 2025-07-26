using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Card : MonoBehaviour
{
    public Image costUI;
    public int cardNum;
    public SpriteRenderer cardImage;
    public TextMeshPro cardName;

    public TextMeshPro positiveNum;
    public TextMeshPro negativeNum;

    public TextMeshPro costNum;

    public TextMeshPro description;


    public void Start()
    {
        if (CardMgr.instance == null)
        {
            Debug.LogError("CardMgr.instance�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        //dataset.data.FindIndex(card => card.CardNum.Equals(cardNum));
        CardData cardData = CardMgr.instance.cardDatas.Find(card => card.CardNum == cardNum);


        cardImage.sprite = cardData.CardImage;
        cardName.text = cardData.CardName;
        positiveNum.text = cardData.PositiveNum.ToString();
        negativeNum.text = cardData.NegativeNum.ToString();
        costNum.text = cardData.CostNum.ToString();

        // ������ ����ĭ�� ����־ ������.
        //description.text = cardData.Description;
    }
}
