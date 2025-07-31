using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUI : MonoBehaviour
{
    public TextMeshProUGUI attackPowerText;
    public TextMeshProUGUI defensePowerText;

    public void Start()
    {
        attackPowerText.text = 1.ToString();
        defensePowerText.text = 1.ToString();
    }

    public void UpdateAttackPowerUI(int value)
    {
        attackPowerText.text = value.ToString();
    }
    
    public void UpdateDefensePowerUI(int value)
    {
        defensePowerText.text = value.ToString();
    }
}
