using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public struct StateIconMapping
{
    public string key;
    public Sprite iconSprite;
    [TextArea(2, 5)]
    public string description;
}

public class StateImageChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("상태별 아이콘 맵핑")]
    public List<StateIconMapping> stateIcons;

    [Header("툴팁 UI 설정")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private Image _imageComponent;
    private string _currentDescription = "";

    private void Awake()
    {
        _imageComponent = GetComponent<Image>();

        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    // 기존 바이러스 호환용: Virus.State → string으로 위임
    public void UpdateStateImage(Virus.State state) => UpdateStateImage(state.ToString());

    public void UpdateStateImage(string key)
    {
        if (_imageComponent == null) return;

        foreach (var mapping in stateIcons)
        {
            if (mapping.key == key)
            {
                _imageComponent.sprite = mapping.iconSprite;
                _currentDescription = mapping.description;
                return;
            }
        }

        _currentDescription = "설명이 등록되지 않은 상태입니다.";
        Debug.LogWarning($"[StateImageChange] '{key}'에 해당하는 아이콘이 설정되지 않았습니다! (오브젝트명: {gameObject.name})");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipText.text = _currentDescription;
            tooltipPanel.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}
