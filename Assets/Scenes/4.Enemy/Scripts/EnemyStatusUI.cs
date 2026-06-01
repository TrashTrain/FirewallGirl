using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 보스 등 개별 적 유닛의 자체 상태 효과(버프/디버프)를 표시하는 UI.
/// EnemyUIController에 optional 참조로 붙이고,
/// 보스가 RegisterEffect로 효과를 등록하면 UpdateData() 호출 시 자동 갱신된다.
/// </summary>
public class EnemyStatusUI : MonoBehaviour
{
    [Header("UI Icons (HP바 하단 패널)")]
    public Image buffIcon;
    public Image debuffIcon;

    [Header("Popup (아이콘 호버 시 표시)")]
    public Transform popupContainer;
    public GameObject popupPrefab;

    [Header("Default Icons")]
    public Sprite defaultBuffIconSprite;
    public Sprite defaultDebuffIconSprite;

    private readonly List<ActiveEffect> _registeredEffects = new List<ActiveEffect>();

    private List<StatusInfo> _currentBuffs   = new List<StatusInfo>();
    private List<StatusInfo> _currentDebuffs = new List<StatusInfo>();

    private void Start()
    {
        if (buffIcon  != null) buffIcon.gameObject.SetActive(false);
        if (debuffIcon != null) debuffIcon.gameObject.SetActive(false);

        SetupHoverEvent(buffIcon,   true);
        SetupHoverEvent(debuffIcon, false);
    }

    // ─── 효과 등록/해제 ──────────────────────────────────────

    public void RegisterEffect(ActiveEffect effect)   => _registeredEffects.Add(effect);
    public void UnregisterEffect(ActiveEffect effect) => _registeredEffects.Remove(effect);

    // ─── UI 갱신 (UpdateData 등에서 호출) ───────────────────

    public void RefreshStatusUI()
    {
        _currentBuffs.Clear();
        _currentDebuffs.Clear();

        foreach (var effect in _registeredEffects)
        {
            if (!effect.isActive()) continue;
            var info = new StatusInfo(effect.icon, effect.getText());
            if (effect.isBuff) _currentBuffs.Add(info);
            else               _currentDebuffs.Add(info);
        }

        if (buffIcon != null)
        {
            buffIcon.gameObject.SetActive(_currentBuffs.Count > 0);
            if (_currentBuffs.Count > 0)
                buffIcon.sprite = defaultBuffIconSprite;
        }

        if (debuffIcon != null)
        {
            debuffIcon.gameObject.SetActive(_currentDebuffs.Count > 0);
            if (_currentDebuffs.Count > 0)
                debuffIcon.sprite = defaultDebuffIconSprite;
        }
    }

    // ─── 호버 팝업 ──────────────────────────────────────────

    private void SetupHoverEvent(Image icon, bool isBuff)
    {
        if (icon == null) return;

        EventTrigger trigger = icon.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = icon.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((_) => {
            RefreshStatusUI();
            ShowPopups(isBuff ? _currentBuffs : _currentDebuffs);
        });
        trigger.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((_) => ClearPopups());
        trigger.triggers.Add(exit);
    }

    private void ShowPopups(List<StatusInfo> statuses)
    {
        ClearPopups();
        if (popupPrefab == null || popupContainer == null) return;

        foreach (var status in statuses)
        {
            GameObject popup = Instantiate(popupPrefab, popupContainer);
            TextMeshProUGUI txt = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = status.text;
        }
    }

    private void ClearPopups()
    {
        if (popupContainer == null) return;
        foreach (Transform child in popupContainer)
            Destroy(child.gameObject);
    }

    private struct StatusInfo
    {
        public Sprite icon;
        public string text;
        public StatusInfo(Sprite icon, string text) { this.icon = icon; this.text = text; }
    }
}
