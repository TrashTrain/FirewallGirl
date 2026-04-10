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
    public Image[] Card;

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
    
    string Card1openAnimationTrigger = "Card1Open";
    string Card3openAnimationTrigger = "Card3Open";

  

    //카드 기억: 중복허용x 자료구조형으로
    private HashSet<(StatType, ValueType)> usedCombinations = new();
    private const int RECENT_HISTORY_LIMIT = 10; // 기억할 최근 값의 수

    // 현재 생성된 카드들의 정보를 저장해둘 리스트
    private List<(GenerateCard pos, GenerateCard neg)> currentCardsInfo = new();
    
    // 현재 화면에 뜬 증강체들을 기억할 리스트
    private List<AugmentBase> currentAugmentRewards = new List<AugmentBase>();



    void Start()
    {
        // 1. 카드가 할당되었는지 확인
        if (Card == null || Card.Length == 0)
        {
            Debug.LogError("UpDownMgr: Card 배열이 비어있습니다! 인스펙터에서 버튼들을 넣어주세요.");
            return;
        }

        CardSystem();

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
    // public void CardSystem()
    // {
    //     Debug.Log("CardSystem 시작");
    //     usedCombinations.Clear();
    //     currentCardsInfo.Clear(); // 리스트 초기화
    //
    //     for (int i = 0; i < Card.Length; i++)
    //     {
    //         if (Card[i] == null) continue;
    //
    //         Card[i].gameObject.SetActive(true);
    //
    //         
    //         HashSet<StatType> localUsedStats = new HashSet<StatType>();
    //
    //         // 긍정 스탯 생성 및 기록
    //         GenerateCard pos = GenerateStat(ValueType.Positive, localUsedStats);
    //         usedCombinations.Add((pos.stat, ValueType.Positive));
    //         localUsedStats.Add(pos.stat); // 같은 카드 내 중복 방지
    //
    //         // 부정 스탯 생성 및 기록
    //         GenerateCard neg = GenerateStat(ValueType.Negative, localUsedStats);
    //         usedCombinations.Add((neg.stat, ValueType.Negative));
    //         localUsedStats.Add(neg.stat);
    //
    //         // 정보 저장 (나중에 클릭 시 사용)
    //         currentCardsInfo.Add((pos, neg));
    //
    //         RewardCard rewardCardScript = Card[i].GetComponent<RewardCard>();
    //         if (rewardCardScript != null)
    //         {
    //             rewardCardScript.SetupCard(
    //                 pos, neg,
    //                 GetSprite(pos.stat, pos.valueType),
    //                 GetSprite(neg.stat, neg.valueType)
    //             );
    //             Debug.Log($"{i}번 카드 세팅 완료: {pos.stat} / {neg.stat}");
    //         }
    //
    //         int index = i;
    //
    //         UpDownCardClickHandler clickHandler = Card[index].GetComponent<UpDownCardClickHandler>();
    //         if (clickHandler == null)
    //         {
    //             clickHandler = Card[index].gameObject.AddComponent<UpDownCardClickHandler>();
    //         }
    //
    //         clickHandler.Init(index, OnCardClicked);
    //     }
    // }
    
    public void CardSystem()
    {
        Debug.Log("증강체 보상 시스템 시작");
        currentAugmentRewards.Clear();

        // 1. Resources/Augments 폴더에 있는 모든 증강체(ScriptableObject)를 불러옵니다.
        // 주의: 파일들이 반드시 Assets/Resources/Augments 경로 안에 있어야 합니다!
        AugmentBase[] loadedAugments = Resources.LoadAll<AugmentBase>("Augments");
        List<AugmentBase> augmentPool = new List<AugmentBase>(loadedAugments);

        if (augmentPool.Count == 0)
        {
            Debug.LogError("Resources/Augments 경로에 증강체 데이터가 없습니다!");
            return;
        }
        
        // ✅ 2. 가중치 풀(Pool) 생성 로직
        foreach (var aug in loadedAugments)
        {
            if (aug is PlayerStatRandomizeAugment)
            {
                // 랜덤 스탯 증강체는 확률을 대폭 높이기 위해 통에 4개를 넣습니다. 
                // (선택지에 2개 이상 중복 등장 가능)
                for (int j = 0; j < 5; j++) augmentPool.Add(aug);
            }
            else
            {
                // 다른 일반 증강체는 1개씩만 넣습니다.
                augmentPool.Add(aug);
            }
        }

        for (int i = 0; i < Card.Length; i++)
        {
            if (Card[i] == null) continue;

            Card[i].gameObject.SetActive(true);

            if (augmentPool.Count == 0) 
            {
                Debug.LogWarning("더 이상 뽑을 증강체가 없습니다!");
                break;
            }

            // ✅ 3. 추첨 및 중복 방지 로직
            int randomIndex = Random.Range(0, augmentPool.Count);
            AugmentBase originalAugment = augmentPool[randomIndex];
            
            if (originalAugment is PlayerStatRandomizeAugment)
            {
                // 뽑힌 랜덤 증강체 딱 1개만 제거 (통에 여러 개가 남았으므로 다음 칸에 또 나올 수 있음)
                augmentPool.RemoveAt(randomIndex);
            }
            else
            {
                // 일반 증강체라면, 다른 칸에 또 나오는 것을 막기 위해 통에서 완전히 제거
                augmentPool.RemoveAll(a => a == originalAugment);
            }

            // ✅ 4. 핵심: 클론 생성(Instantiate) 및 초기화(Initialize)
            // 이렇게 해야 원본 ScriptableObject가 오염되지 않고, 
            // 랜덤 증강체가 2개 떴을 때 각각 다른 스탯과 수치를 가지게 됩니다.
            AugmentBase clonedAugment = Instantiate(originalAugment);
            clonedAugment.Initialize(); 

            // 클릭 시 데이터를 넘겨주기 위해 '클론'을 저장
            currentAugmentRewards.Add(clonedAugment);

            // 5. UI 갱신
            RewardCard rewardCardScript = Card[i].GetComponent<RewardCard>();
            if (rewardCardScript != null)
            {
                rewardCardScript.SetupAugmentCard(clonedAugment);
            }

            // 6. 클릭 이벤트 연결
            int index = i;
            UpDownCardClickHandler clickHandler = Card[i].GetComponent<UpDownCardClickHandler>();
            if (clickHandler == null)
            {
                clickHandler = Card[index].gameObject.AddComponent<UpDownCardClickHandler>();
            }

            clickHandler.Init(index, OnCardClicked);
        }
    }

    //카드를 선택한 경우 스테이지 씬으로
    // void OnCardClicked(int index)
    // {
    //     // 1. 선택한 카드의 데이터를 PlayerManager에 적용
    //     var selected = currentCardsInfo[index];
    //     PlayerManager.instance.ApplyCardStats(selected.pos, selected.neg);
    //     StageSaveManager.ClearStage(StageSaveManager.CurrentStageIdx);
    //
    //     SceneManager.LoadScene("StageScene");
    // }
    
    void OnCardClicked(int index)
    {
        // 1. 내가 클릭한 증강체 가져오기
        AugmentBase selectedAugment = currentAugmentRewards[index];
        Debug.Log($"선택한 증강체: {selectedAugment.augmentName}");

        PlayerManager.instance.AcquireAugment(selectedAugment);

        // 3. 씬 전환
        StageSaveManager.ClearStage(StageSaveManager.CurrentStageIdx);
        SceneManager.LoadScene("StageScene");
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


