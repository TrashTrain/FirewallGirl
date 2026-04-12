using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    // 기존의 Slider와 단일 Image는 더 이상 사용하지 않으므로 제거합니다.
    public TextMeshProUGUI hpText;

    [Header("Health Blocks Setup")]
    [Tooltip("16개의 체력 칸(파란색 이미지) 게임 오브젝트를 순서대로 넣어주세요.")]
    public GameObject[] healthBlocks; // 16개의 칸을 담을 배열
    private readonly int maxBlocks = 16; // 총 칸 수 고정

    public void Start()
    {
        PlayerManager.instance.hpBar = this;
    }

    public void UpdateHPBar(int current, int max)
    {
        // 1. 체력 텍스트 업데이트 (기존 동일)
        hpText.text = $"{current}/{max}";

        // 2. 현재 체력 비율 계산 (0.0 ~ 1.0)
        float percent = (float)current / max;

        // 3. 켜져야 할 칸의 개수 계산
        // Mathf.CeilToInt를 사용하여 체력이 조금이라도 남아있다면 최소 1칸은 보이게 처리합니다.
        int activeBlockCount = Mathf.CeilToInt(percent * maxBlocks);

        // 예외 처리를 위해 0 ~ 16 사이의 값으로 고정
        activeBlockCount = Mathf.Clamp(activeBlockCount, 0, maxBlocks);

        // 4. 배열을 돌면서 조건에 맞게 칸을 켜고 끕니다.
        for (int i = 0; i < healthBlocks.Length; i++)
        {
            if (i < activeBlockCount)
            {
                healthBlocks[i].SetActive(true);  // 체력에 해당하면 파란 칸 켜기
            }
            else
            {
                healthBlocks[i].SetActive(false); // 깎인 체력이면 파란 칸 끄기
            }
        }
    }
}