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

    private int attackPower;
    private int defensePower;

    public int ap
    {
        get { return attackPower; }
        set { attackPower = value; }
    }
    
    public int dp
    {
        get { return defensePower; }
        set { defensePower = value; }
    }
    
    public int cost
    {
        get { return costValue; }
        set { costValue = value; }
    }

    private void Start()
    {
        costValue = cardData.cost;
        attackPower = cardData.attackPower;
        defensePower = cardData.defensePower;
        
        Image cardImage = transform.Find("Content").GetComponent<Image>();
        TextMeshProUGUI cardName = transform.Find("CardName/Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI attackPowerText = transform.Find("AttackPower/Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI defensePowerText = transform.Find("Defense/Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        
        cardImage.sprite = cardData.cardImage;
        cardName.text = cardData.cardName;
        attackPowerText.text = attackPower.ToString();
        defensePowerText.text = defensePower.ToString();
        costText.text = costValue.ToString();
    }
}
