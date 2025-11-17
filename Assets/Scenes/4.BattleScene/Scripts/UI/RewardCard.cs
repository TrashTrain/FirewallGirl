using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class RewardCard : MonoBehaviour
{
    public RewardCardObject rewardCardData;

    private void Start()
    {
        Image cardImage = transform.Find("CardImage").GetComponent<Image>();
        TextMeshProUGUI cardText = transform.Find("CardText").GetComponent<TextMeshProUGUI>();
        
        cardImage.sprite = rewardCardData.cardImage;
        cardText.text = rewardCardData.description;
    }
}
