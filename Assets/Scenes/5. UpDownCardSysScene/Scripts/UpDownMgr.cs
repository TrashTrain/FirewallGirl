using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Security.Cryptography.X509Certificates;

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
    [Header("카드 UI")]
    public Button[] Card;

    [Header("카드 요소")]
    public TextMeshProUGUI[] PositiveSkillText;
    public TextMeshProUGUI[] NegativeSkillText;
    public TextMeshProUGUI[] PositiveUpDownText;
    public TextMeshProUGUI[] NegativeUpDownText;

    public Image[] PositiveSkillIcons;
    public Image[] NegativeSkillIcons;


    [Header("카드 Sprite")]
    public Sprite swordSprite; //공격력 = 방패
    public Sprite shieldSprite; //방어력 = 칼 
    public Sprite costUpSprite; //코스트 = 상승 그래프
    public Sprite costDownSprite; //코스트 = 상승 그래프
    public Sprite hpSprite; //체력 = 물약 
    public Sprite avoidanceSprite; //회피율 = 바람

    [Header("카드 Reload 버튼")]
    public Button ReloadBtn; //카드 리로드 버튼

    [Header("카드 리로드 애니메이션")]
    public Animation OpenAnimtion1; 
    public Animation OpenAnimtion3; 
    
    string Card1openAnimationTrigger = "Card1Open";
    string Card3openAnimationTrigger = "Card3Open";

  

    //카드 기억: 중복허용x 자료구조형으로
    private HashSet<(StatType, ValueType)> usedCombinations = new();
    private const int RECENT_HISTORY_LIMIT = 10; // 기억할 최근 값의 수
    


    void Start()
    {
        foreach (Button card in Card)
            card.gameObject.SetActive(false);

        ReloadBtn.onClick.AddListener(ReloadBtnClick);

        for (int i = 0; i < Card.Length; i++)
        {
            int index = i;
            Card[i].onClick.AddListener(() => OnCardClicked());
        }

        CardSystem();

    }

    //카드를 선택한 경우 배틀씬으로 
    void OnCardClicked()
    {
        Debug.Log("Clicked");
        // 필요하면 선택된 카드 인덱스를 저장
        // GameManager.selectedCardId = cardIndex;

        SceneManager.LoadScene("BattleScene");
    }

    // 스프라이트 매칭 
    Sprite GetSprite(StatType stat, ValueType value)
    {
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

    // 증감 구조체 
    public struct GenerateCard
    {
        public StatType stat; // 속성
        public ValueType valueType; // + -
        public int valueAmount; // 절댓값

        public GenerateCard (StatType stat, ValueType type, int value)
        {
            this.stat = stat;
            this.valueType = type;
            this.valueAmount = value; 
        }

        public int GetSignedValue() => valueType == ValueType.Positive ? valueAmount : -valueAmount;

        public override string ToString() => $"{(valueType == ValueType.Positive ? "+" : "-")}{valueAmount}";

    }

    // 스탯 생성기 
    GenerateCard GenerateStat(ValueType value, HashSet<StatType> excludeStats) //+ or - , 제외할 스탯
    {
        List<StatType> availableStats = new(); // 허용할 스탯 
        foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
        {
            // 제외할 수치가 아니고, 이미 사용한 조합이 아니라면 허용할 스탯 리스트에 Add
            if (!excludeStats.Contains(stat) && !usedCombinations.Contains((stat, value)))
                availableStats.Add(stat);
        }
        // 허용할 스탯의 수가  = 0 인경우 
        if (availableStats.Count == 0)
            throw new System.Exception("사용 가능한 속성 조합이 부족합니다.");

        // 가능한 스탯 중 무작위로 selectedStat로 지정
        StatType selectedStat = availableStats[Random.Range(0, availableStats.Count)];
        int selectValueAmount = Random.Range(1, 4); // +1~+3 또는 -1~-3 의 절댓값
        // 제외할 스탯을 전부 재외하고 스탯을 하나 생성
        return new GenerateCard(selectedStat, value, selectValueAmount);
    }

    // 증감 시스템 
    public void CardSystem()
    {
        usedCombinations.Clear();

        for (int i = 0; i < 3; i++)
        {
            // 카드 안 속성 중복 방지 
            HashSet<StatType> localUsedStats = new();

            // 긍정 효과 생성 
            GenerateCard pos = GenerateStat (ValueType.Positive, localUsedStats); // + or - Stat
            usedCombinations.Add((pos.stat, ValueType.Positive));
            localUsedStats.Add(pos.stat);
            PositiveSkillText[i].text = pos.stat.ToKorean();
            PositiveUpDownText[i].text = pos.ToString();
            PositiveSkillIcons[i].sprite = GetSprite(pos.stat, pos.valueType);

            // 부정 효과 생성 
            GenerateCard neg = GenerateStat (ValueType.Negative, localUsedStats);
            usedCombinations.Add((neg.stat, ValueType.Negative));
            localUsedStats.Add(neg.stat);
            NegativeSkillText[i].text = neg.stat.ToKorean();
            NegativeUpDownText[i].text = neg.ToString();
            NegativeSkillIcons[i].sprite = GetSprite(neg.stat, neg.valueType);
        }
    }

    // 카드 리로드 버튼 
    public void ReloadBtnClick()
    {
        foreach (Button card in Card)
            card.gameObject.SetActive(true);

        OpenAnimtion1.Play(Card1openAnimationTrigger);
        OpenAnimtion3.Play(Card3openAnimationTrigger);


        CardSystem();
    }

}

// 속성 한국어 매칭
public static class StatTypeExtensions
{
    public static string ToKorean(this StatType stat)
    {
        switch (stat)
        {
            case StatType.Attack: return "공격력";
            case StatType.Defense: return "방어력";
            case StatType.Cost: return "코스트";
            case StatType.Health: return "체력";
            case StatType.Evasion: return "회피율";
            default: return stat.ToString();
        }
    }
}


