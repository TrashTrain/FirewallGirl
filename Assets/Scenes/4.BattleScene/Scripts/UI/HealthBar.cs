// HealthBar.cs (수정)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider hpSlider;
    public Image fillImage;
    public TextMeshProUGUI hpText;

    public void Start()
    {
        // 씬 로드 시마다 HealthBar 인스턴스를 PlayerManager에 재등록
        PlayerManager.instance.hpBar = this;

        // 이 요청이 PlayerManager의 Start()보다 늦게 실행되더라도 HP 바는 즉시 최신 값으로 초기화
        PlayerManager.instance.UpdateUI();
    }

    public void UpdateHPBar(int current, int max)
    {
        // 1. 슬라이더 최대/현재 값 설정 (이것이 TextMeshPro와 fillAmount의 값이 정확하게 맞도록 보장합니다.)
        hpSlider.maxValue = max;
        hpSlider.value = current;

        // 2. 텍스트 업데이트
        hpText.text = $"{current}/{max}";

        // 3. fillImage 업데이트 (옵션: Slider를 썼다면 Image는 필요 없을 수 있으나, 둘 다 사용해도 무방)
        float percent = (float)current / max;
        fillImage.fillAmount = percent;

    }
}