using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// 카드 속성 = 스탯 
public enum StatType
{
    Attack, //공격력
    Defense, //방어력
    Cost, //코스트
    Health, //체력
    Evasion //회피율
}

// 속성의 + or - 값
public enum ValueType
{
    Positive, // +
    Negative  // -
}


public class UpDownMgr : MonoBehaviour
{
    // === UI 참조 변수 (CardSceneInitializer에 의해 씬 로드 시 재연결됨) ===

    [Header("카드 UI")]
    public Button[] Card;
    [Header("UI 부모 오브젝트")]
    public GameObject CardParentPanel; // 카드들을 담고 있는 최상위 패널 (예: UpDownObject)

    [Header("카드 요소")]
    public TextMeshProUGUI[] PositiveSkillText;
    public TextMeshProUGUI[] NegativeSkillText;
    public TextMeshProUGUI[] PositiveUpDownText;
    public TextMeshProUGUI[] NegativeUpDownText;
    public Image[] PositiveSkillIcons;
    public Image[] NegativeSkillIcons;

    // === 데이터 변수 ===
    public GenerateCard[] cardPositiveStats = new GenerateCard[3];
    public GenerateCard[] cardNegativeStats = new GenerateCard[3];

    // === 스프라이트 ===
    [Header("카드 Sprite")]
    public Sprite swordSprite;
    public Sprite shieldSprite;
    public Sprite costUpSprite;
    public Sprite costDownSprite;
    public Sprite hpSprite;
    public Sprite avoidanceSprite;

    // === 애니메이션 ===
    [Header("카드 리로드 애니메이션")]
    public Animation OpenAnimtion1;
    public Animation OpenAnimtion3;
    string Card1openAnimationTrigger = "Card1Open";
    string Card3openAnimationTrigger = "Card3Open";

    // === 싱글톤 및 데이터 기억 ===
    public static UpDownMgr instance;
    private HashSet<(StatType, ValueType)> usedCombinations = new();

    // 증감 구조체 
    public struct GenerateCard
    {
        // (GenerateCard 구조체 내부 코드 유지)
        public StatType stat;
        public ValueType valueType;
        public int valueAmount;

        public GenerateCard(StatType stat, ValueType type, int value)
        {
            this.stat = stat;
            this.valueType = type;
            this.valueAmount = value;
        }

        public int GetSignedValue() => valueType == ValueType.Positive ? valueAmount : -valueAmount;
        public override string ToString() => $"{(valueType == ValueType.Positive ? "+" : "-")}{valueAmount}";
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start() { }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "UpDownSysScene")
        {
            // 1. UpDownMgr 컴포넌트가 붙은 최상위 오브젝트 강제 활성화
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            // 2. Canvas (CardSceneInitializer가 붙어있는) 강제 활성화
            // Hierarchy 이미지 상 UpDownMgr가 Canvas의 부모에 있으므로 자식에서 찾습니다.
            Canvas canvasToActivate = GetComponentInChildren<Canvas>(true);

            if (canvasToActivate != null && !canvasToActivate.gameObject.activeSelf)
            {
                Debug.Log("OnSceneLoaded: Canvas 강제 활성화 시도.");
                canvasToActivate.gameObject.SetActive(true);
            }

            // 3. Canvas와 CardSceneInitializer가 활성화되고 Start()를 마칠 시간을 준 후 초기화 확인
            StartCoroutine(DelayedInitializationCheck());
        }
    }

    // 지연된 초기화 확인 코루틴
    IEnumerator DelayedInitializationCheck()
    {
        // 1프레임 대기 (Canvas 활성화 및 CardSceneInitializer.Start() 실행 대기)
        yield return null;

        // 이 시점에서 CardSceneInitializer가 InitializeUI를 호출하여 모든 UI를 연결하고 활성화했어야 합니다.
        if (CardParentPanel == null)
        {
            Debug.LogError("CardParentPanel이 NULL입니다. CardSceneInitializer의 인스펙터 연결 또는 실행 순서를 확인하세요.");
            yield break;
        }

        // CardParentPanel이 활성화되지 않았다면 강제 활성화 (4번 시점 문제 해결)
        if (!CardParentPanel.activeSelf)
        {
            Debug.Log("DelayedCheck: CardParentPanel이 활성화되지 않아 강제 활성화합니다.");
            CardParentPanel.SetActive(true);
            // 이 경우 ReloadAnimation을 다시 호출하여 카드를 새로 생성하고 표시해야 합니다.
            ReloadAnimation();
        }
    }


    /// <summary>
    /// CardSceneInitializer로부터 호출되어 UI 컴포넌트를 재연결하고 초기화합니다.
    /// </summary>
    public void InitializeUI(Button[] cards, TextMeshProUGUI[] posTexts, TextMeshProUGUI[] negTexts,
                           TextMeshProUGUI[] posUpDownTexts, TextMeshProUGUI[] negUpDownTexts,
                           Image[] posIcons, Image[] negIcons, GameObject parentPanel)
    {
        // 1. UI 컴포넌트 재연결
        Card = cards;
        PositiveSkillText = posTexts;
        NegativeSkillText = negTexts;
        PositiveUpDownText = posUpDownTexts;
        NegativeUpDownText = negUpDownTexts;
        PositiveSkillIcons = posIcons;
        NegativeSkillIcons = negIcons;
        CardParentPanel = parentPanel;

        // 2. 기존 리스너 제거 및 재설정
        for (int i = 0; i < Card.Length; i++)
        {
            Card[i].onClick.RemoveAllListeners();
            int index = i;
            Card[i].onClick.AddListener(() => OnCardClicked(index));

            Card[i].gameObject.SetActive(false); // 초기 상태는 비활성화
        }

        // 3. UI 활성화 및 카드 시스템 실행
        if (CardParentPanel != null)
        {
            CardParentPanel.SetActive(true); // 카드 부모 패널 활성화
        }

        ReloadAnimation(); // 카드 생성 및 애니메이션 실행 (CardSystem 포함)
    }


    // 카드를 선택한 경우 배틀씬으로 
    void OnCardClicked(int cardIndex)
    {
        // 1. PlayerManager에 스탯 적용 요청
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.ApplyStatChange(
                cardPositiveStats[cardIndex],
                cardNegativeStats[cardIndex]);
        }
        else
        {
            Debug.LogError("PlayerManager 인스턴스를 찾을 수 없습니다! 스탯 적용 실패.");
        }

        // 2. 카드 UI 비활성화 (다음 씬으로 넘어갈 때 보이지 않도록)
        if (CardParentPanel != null)
        {
            CardParentPanel.SetActive(false);
        }

        // 3. 씬 전환 (SceneLoader를 통해 IntegratedScene으로)
        SceneLoader.LoadBattleScene();
    }

    // 카드 리로드 (애니메이션 포함)
    public void ReloadAnimation()
    {
        //  (기존 ReloadAnimation 내부 코드 유지) 
        foreach (Button card in Card)
            card.gameObject.SetActive(true);

        if (OpenAnimtion1 != null) OpenAnimtion1.Play(Card1openAnimationTrigger);
        if (OpenAnimtion3 != null) OpenAnimtion3.Play(Card3openAnimationTrigger);

        CardSystem();
    }


    // 증감 시스템 (카드 정보 생성)
    public void CardSystem()
    {
        // ... (기존 CardSystem 내부 코드 유지) ...
        usedCombinations.Clear();

        for (int i = 0; i < 3; i++)
        {
            HashSet<StatType> localUsedStats = new();

            GenerateCard pos = GenerateStat(ValueType.Positive, localUsedStats);
            usedCombinations.Add((pos.stat, ValueType.Positive));
            localUsedStats.Add(pos.stat);

            if (i < PositiveSkillText.Length)
            {
                PositiveSkillText[i].text = pos.stat.ToKorean();
                PositiveUpDownText[i].text = pos.ToString();
                PositiveSkillIcons[i].sprite = GetSprite(pos.stat, pos.valueType);
                cardPositiveStats[i] = pos;
            }

            GenerateCard neg = GenerateStat(ValueType.Negative, localUsedStats);
            usedCombinations.Add((neg.stat, ValueType.Negative));
            localUsedStats.Add(neg.stat);

            if (i < NegativeSkillText.Length)
            {
                NegativeSkillText[i].text = neg.stat.ToKorean();
                NegativeUpDownText[i].text = neg.ToString();
                NegativeSkillIcons[i].sprite = GetSprite(neg.stat, neg.valueType);
                cardNegativeStats[i] = neg;
            }
        }
    }

    // 스탯 생성기 
    GenerateCard GenerateStat(ValueType value, HashSet<StatType> excludeStats)
    {
        // (기존 GenerateStat 내부 코드 유지)
        List<StatType> availableStats = new();
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            if (!excludeStats.Contains(stat) && !usedCombinations.Contains((stat, value)))
                availableStats.Add(stat);
        }

        if (availableStats.Count == 0)
        {
            Debug.LogWarning($"사용 가능한 속성 조합이 부족하여 중복을 허용하고 재시도합니다.");

            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                if (!usedCombinations.Contains((stat, value)))
                    availableStats.Add(stat);
            }
            if (availableStats.Count == 0)
                throw new Exception("더 이상 사용할 수 있는 고유한 스탯 조합이 없습니다.");
        }

        StatType selectedStat = availableStats[UnityEngine.Random.Range(0, availableStats.Count)];
        int selectValueAmount = UnityEngine.Random.Range(1, 4);
        return new GenerateCard(selectedStat, value, selectValueAmount);
    }

    // 스프라이트 매칭 
    Sprite GetSprite(StatType stat, ValueType value)
    {
        // (기존 GetSprite 내부 코드 유지)
        return stat switch
        {
            StatType.Attack => swordSprite,
            StatType.Defense => shieldSprite,
            StatType.Cost => value == ValueType.Positive ? costUpSprite : costDownSprite,
            StatType.Health => hpSprite,
            StatType.Evasion => avoidanceSprite,
            _ => null,
        };
    }
}

// 속성 한국어 매칭
public static class StatTypeExtensions
{
    public static string ToKorean(this StatType stat)
    {
        // (기존 ToKorean 내부 코드 유지) 
        return stat switch
        {
            StatType.Attack => "공격력",
            StatType.Defense => "방어력",
            StatType.Cost => "코스트",
            StatType.Health => "체력",
            StatType.Evasion => "회피율",
            _ => stat.ToString(),
        };
    }
}