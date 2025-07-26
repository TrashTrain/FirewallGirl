using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardCard : MonoBehaviour
{
    public RewardCardObject rewardCardData;

    private void Start()
    {
        Image cardImage = transform.Find("CardImage").GetComponent<Image>();
        cardImage.sprite = rewardCardData.cardImage;
    }
}
