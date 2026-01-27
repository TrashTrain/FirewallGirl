using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class PlayerCard : MonoBehaviour
{
    public CardObject cardData;
    
    [SerializeField]
    private int costValue;

    private int positiveStatValue;
    private int negativeStatValue;

    // 카드 설명 추가.
    private string description;

    public int posValue
    {
        get { return positiveStatValue; }
        set { positiveStatValue = value; }
    }
    
    public int negValue
    {
        get { return negativeStatValue; }
        set { negativeStatValue = value; }
    }
    
    public int cost
    {
        get { return costValue; }
        set { costValue = value; }
    }

    public string desc
    {
        get { return description; } 
        set { description = value; }
    }
    
    private Sprite GetStatIcon(StatType stat)
    {
        if (StatIconManager.Instance == null)
        {
            Debug.LogError("StatIconManager가 씬에 없음");
            return null;
        }
        return StatIconManager.Instance.GetIcon(stat);
    }

    private void Start()
    {
        if (cardData == null)
        {
            Debug.LogError("cardData 없음.");
            return;
        }
        costValue = cardData.cost;
        positiveStatValue = cardData.positiveStatValue;
        negativeStatValue = cardData.negativeStatValue;
        
        Image cardImage = transform.Find("Content").GetComponent<Image>(); // 카드 그림
        TextMeshProUGUI cardName = transform.Find("CardName/Text").GetComponent<TextMeshProUGUI>(); // 카드 이름

        Image positiveIcon = transform.Find("PositiveStat").GetComponent<Image>();
        Image negativeIcon = transform.Find("NegativeStat").GetComponent<Image>();
        
        TextMeshProUGUI posStatText = transform.Find("PositiveStat/Text").GetComponent<TextMeshProUGUI>(); // 긍정 수치 텍스트
        TextMeshProUGUI negStatText = transform.Find("NegativeStat/Text").GetComponent<TextMeshProUGUI>(); // 부정 수치 텍스트
        
        TextMeshProUGUI costText = transform.Find("Cost/CostText").GetComponent<TextMeshProUGUI>(); // 코스트 수치 텍스트
        TextMeshProUGUI descriptionText = transform.Find("Description").GetComponent<TextMeshProUGUI>(); // 카드 설명 텍스트
        
        cardImage.sprite = cardData.cardImage;
        cardName.text = cardData.cardName;

        positiveIcon.sprite = GetStatIcon(cardData.positiveStatType);
        negativeIcon.sprite = GetStatIcon(cardData.negativeStatType);
        
        posStatText.text = positiveStatValue.ToString();
        negStatText.text = negativeStatValue.ToString();
        
        costText.text = costValue.ToString();
        descriptionText.text = cardData.description;
    }
}
