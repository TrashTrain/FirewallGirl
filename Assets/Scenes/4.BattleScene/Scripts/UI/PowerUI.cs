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
        PlayerManager player = PlayerManager.instance;

        player.powerUI = this;

        if (player != null)
        {
            attackPowerText.text = player.attackPower.ToString();
            defensePowerText.text = player.defensePower.ToString();
        }
        else
        {
            attackPowerText.text = 1.ToString();
            defensePowerText.text = 1.ToString();
        }

        PlayerManager.instance.powerUI = this; // ���ο� ���� UI�� �ν��Ͻ��� ����
        PlayerManager.instance.UpdateUI();      // ������ڸ��� �ֽ� �����ͷ� UI ����
    }

    public void UpdateAttackPowerUI(int value)
    {
        attackPowerText.text = BossUnrendered.GraphicChangeLevel >= 1 ? "???" : value.ToString();
    }

    public void UpdateDefensePowerUI(int value)
    {
        defensePowerText.text = BossUnrendered.GraphicChangeLevel >= 1 ? "???" : value.ToString();
    }
}
