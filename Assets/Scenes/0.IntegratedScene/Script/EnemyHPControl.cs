using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPControl : MonoBehaviour
{
    public TextMeshProUGUI hpText;

    [Header("Enemy Health Blocks Setup")]
    [Tooltip("16개의 적 체력 칸 게임 오브젝트를 순서대로 넣어주세요.")]
    public GameObject[] healthBlocks; // 16개의 칸을 담을 배열
    private readonly int maxBlocks = 16;

    // 초기화할 관리자 클래스나 슬라이더가 없으므로 Start 함수는 삭제하거나 비워둡니다.
    public void Start()
    {
        // 필요시 다른 초기화 코드를 여기에 작성하세요.
    }

    public void UpdateHPBar(int current, int max)
    {
        // 1. 체력 텍스트 업데이트
        hpText.text = $"{current}/{max}";

        // 방어 코드: max가 0일 경우 0으로 나누는 오류 방지
        if (max <= 0) return;

        // 2. 현재 체력 비율 계산 (0.0 ~ 1.0)
        float percent = (float)current / max;

        // 3. 켜져야 할 칸의 개수 계산 (조금이라도 체력이 남으면 1칸 유지)
        int activeBlockCount = Mathf.CeilToInt(percent * maxBlocks);
        activeBlockCount = Mathf.Clamp(activeBlockCount, 0, maxBlocks);

        // 4. 배열을 돌면서 조건에 맞게 칸을 켜고 끕니다.
        for (int i = 0; i < healthBlocks.Length; i++)
        {
            if (i < activeBlockCount)
            {
                healthBlocks[i].SetActive(true);  // 칸 켜기
            }
            else
            {
                healthBlocks[i].SetActive(false); // 칸 끄기
            }
        }
    }
}