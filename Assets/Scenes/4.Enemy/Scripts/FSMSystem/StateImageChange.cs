using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // [추가] 마우스 오버 이벤트를 처리하기 위한 네임스페이스
using TMPro; // [추가] TextMeshPro 텍스트 컴포넌트 사용을 위한 네임스페이스

// 인스펙터에서 상태(State), 이미지(Sprite), 설명(Description)을 한 쌍으로 묶어서 보여주기 위한 구조체
[System.Serializable]
public struct StateIconMapping
{
    public Virus.State state;
    public Sprite iconSprite;
    [TextArea(2, 5)] // [추가] 인스펙터에서 텍스트를 여러 줄로 편하게 입력할 수 있게 해주는 속성
    public string description;
}

// [추가] IPointerEnterHandler, IPointerExitHandler 인터페이스 상속
public class StateImageChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("상태별 아이콘 맵핑")]
    public List<StateIconMapping> stateIcons;

    [Header("툴팁 UI 설정")]
    public GameObject tooltipPanel;    // 설명을 보여줄 팝업의 배경(패널) 오브젝트
    public TextMeshProUGUI tooltipText; // 설명을 띄워줄 텍스트 컴포넌트

    private Image _imageComponent;
    private string _currentDescription = ""; // 현재 상태의 설명을 임시로 저장해둘 변수

    private void Awake()
    {
        _imageComponent = GetComponent<Image>();

        // 게임 시작 시 팝업이 켜져있을 수 있으므로 강제로 꺼줍니다.
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 몬스터의 상태를 전달받아 그에 맞는 아이콘과 설명 텍스트를 갱신합니다.
    /// </summary>
    /// <param name="state">변경할 몬스터의 다음 행동 상태</param>
    public void UpdateStateImage(Virus.State state)
    {
        if (_imageComponent == null) return;

        // 리스트를 순회하며 요청받은 상태와 짝지어진 데이터를 찾아 적용합니다.
        foreach (var mapping in stateIcons)
        {
            if (mapping.state == state)
            {
                Debug.Log("mapping state : " + mapping.state);
                _imageComponent.sprite = mapping.iconSprite;
                _currentDescription = mapping.description; // 💡 찾은 설명을 저장해둠
                return; // 찾았으니 함수 종료
            }
        }

        // 인스펙터에 매핑되지 않은 상태일 경우 예외 처리
        _currentDescription = "설명이 등록되지 않은 상태입니다.";
        Debug.LogWarning($"[StateImageChange] {state} 상태에 해당하는 아이콘이 설정되지 않았습니다! (오브젝트명: {gameObject.name})");
    }

    // =========================================================
    // 💡 마우스 이벤트 처리 파트
    // =========================================================

    // 마우스가 아이콘 위에 올라갔을 때 실행됨 (1번 조건 충족)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipText.text = _currentDescription; // 저장해둔 현재 상태의 설명을 텍스트UI에 적용
            tooltipPanel.SetActive(true);           // 팝업 패널 활성화
        }
    }

    // 마우스가 아이콘에서 벗어났을 때 실행됨 (2번 조건 충족)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false); // 팝업 패널 비활성화
        }
    }
}