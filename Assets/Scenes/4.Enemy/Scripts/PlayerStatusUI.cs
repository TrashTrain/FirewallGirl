using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    public static PlayerStatusUI instance;

    [Header("UI Icons (체력바 아래 패널)")]
    public Image buffIcon;      // Horizontal Layout Group 하위의 첫 번째
    public Image debuffIcon;    // Horizontal Layout Group 하위의 두 번째

    [Header("Popup (캐릭터 우측 컨테이너)")]
    public Transform popupContainer;
    public GameObject popupPrefab;   // 배경 + TMP Text로 구성된 프리팹

    [Header("Default Status Icons (대표 아이콘 설정)")]
    // 각 카테고리의 상태가 1개라도 있을 때 띄울 대표 범용 아이콘
    public Sprite defaultBuffIconSprite;
    public Sprite defaultDebuffIconSprite;

    // 하드코딩된 특정 디버프용 아이콘 (카운트 체크용이 아니므로 그대로 유지)
    [Header("Specific Debuff Icons")]
    public Sprite iconDefenseBan;
    public Sprite iconLag;
    public Sprite iconDotDamage;

    // 현재 보유 중인 상태 목록
    private List<StatusInfo> currentBuffs = new List<StatusInfo>();
    private List<StatusInfo> currentDebuffs = new List<StatusInfo>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        buffIcon.gameObject.SetActive(false);
        debuffIcon.gameObject.SetActive(false);

        SetupHoverEvent(buffIcon, true);
        SetupHoverEvent(debuffIcon, false);
    }

    public void RefreshStatusUI()
    {
        currentBuffs.Clear();
        currentDebuffs.Clear();

        // ========================================================
        // 1. 개별 변수 기반 특수 디버프 체크
        // ========================================================
        if (PlayerManager.instance.cannotGainDefenseTurns > 0)
            currentDebuffs.Add(new StatusInfo(iconDefenseBan, $"방어 불가\n({PlayerManager.instance.cannotGainDefenseTurns}턴 남음)"));

        if (PlayerManager.instance.lagDebuffTurns > 0)
            currentDebuffs.Add(new StatusInfo(iconLag, $"쿨타임 증가\n({PlayerManager.instance.lagDebuffTurns}턴 남음)"));

        if (PlayerManager.instance.currentDotDamage > 0)
            currentDebuffs.Add(new StatusInfo(iconDotDamage, $"지속 피해\n(매턴 {PlayerManager.instance.currentDotDamage} 피해)"));

        // ========================================================
        // 2. 💡 [수정] activeModifiers 리스트 기반 버프/디버프 자동 분류
        // ========================================================
        if (PlayerManager.instance.activeModifiers != null)
        {
            foreach (var mod in PlayerManager.instance.activeModifiers)
            {
                // Modifier 자체에 담긴 UI 정보를 바탕으로 분류
                string displayText = $"{mod.descriptionText}\n({mod.durationTurns}턴 남음)";
                StatusInfo info = new StatusInfo(mod.statusIcon, displayText);

                if (mod.isBuff)
                {
                    currentBuffs.Add(info);
                }
                else
                {
                    currentDebuffs.Add(info);
                }
            }
        }

        // ========================================================
        // 3. 체력바 아래 대표 아이콘 표기 (카운트 로그 추가)
        // ========================================================
        // Debug.Log($"[UI 갱신] 버프: {currentBuffs.Count}개, 디버프: {currentDebuffs.Count}개");

        // 버프 아이콘 처리
        if (currentBuffs.Count > 0)
        {
            buffIcon.gameObject.SetActive(true);
            buffIcon.sprite = defaultBuffIconSprite;
        }
        else
        {
            buffIcon.gameObject.SetActive(false);
        }

        // 디버프 아이콘 처리
        if (currentDebuffs.Count > 0)
        {
            debuffIcon.gameObject.SetActive(true);
            debuffIcon.sprite = defaultDebuffIconSprite;
        }
        else
        {
            debuffIcon.gameObject.SetActive(false);
        }
    }

    private void SetupHoverEvent(Image icon, bool isBuff)
    {
        EventTrigger trigger = icon.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = icon.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) => {
            RefreshStatusUI(); // 띄우기 직전 데이터 갱신
            ShowPopups(isBuff ? currentBuffs : currentDebuffs);
        });
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => {
            ClearPopups();
        });
        trigger.triggers.Add(exit);
    }

    private void ShowPopups(List<StatusInfo> statuses)
    {
        ClearPopups();

        foreach (var status in statuses)
        {
            GameObject popup = Instantiate(popupPrefab, popupContainer);

            // 💡 프리팹 내부의 TMP Text에 상태 정보(설명+턴수)를 덮어씁니다.
            TextMeshProUGUI txt = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = status.text;
            }
        }
    }

    private void ClearPopups()
    {
        foreach (Transform child in popupContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private struct StatusInfo
    {
        public Sprite icon; // 개별 아이콘은 팝업에서 사용 (또는 대표 아이콘으로 활용 가능)
        public string text;
        public StatusInfo(Sprite icon, string text) { this.icon = icon; this.text = text; }
    }
}