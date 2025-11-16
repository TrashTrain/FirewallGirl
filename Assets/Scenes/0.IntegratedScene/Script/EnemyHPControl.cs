using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPControl : MonoBehaviour
{
    public Slider hpSlider;
    public Image fillImage;
    public TextMeshProUGUI hpText;

    public void Start()
    {
        hpSlider.minValue = 0;
        hpSlider.maxValue = 100;
        hpSlider.value = 100;

    }

    public void UpdateHPBar(int current, int max)
    {
        float percent = (float)current / max;
        fillImage.fillAmount = percent;
        hpSlider.maxValue = max;
        hpSlider.value = current;
        hpText.text = $"{current}/{max}";
    }
}
