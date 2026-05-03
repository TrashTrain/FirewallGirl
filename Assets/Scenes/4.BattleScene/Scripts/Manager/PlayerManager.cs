using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 여러 턴 동안 유지되는 상태 이상을 관리하기 위한 클래스
[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public int amount;
    public int durationTurns; // 남은 유지 턴 수

    [Header("UI Display")]
    public Sprite statusIcon;     // 이 효과에 매핑될 범용 아이콘 (AtkUp, AtkDown 등)
    public bool isBuff = false;     // true면 버프 패널, false면 디버프 패널에 표기
    [TextArea]
    public string descriptionText; // "공격력 감소" 등 플레이어에게 보여줄 실제 설명
}

public class PlayerManager : MonoBehaviour
{
    // Player info
    public int maxHP;
    public int currentHP;
    public int currentCost;
    public int totalCost;
    public int attackPower;
    public int defensePower;

    // GetFinalStat 함수가 업데이트되어 이제 activeModifiers도 반영합니다.
    public int AttackPower => GetFinalStat(StatType.Attack);
    public int DefensePower => GetFinalStat(StatType.Defense);
    public int MaxHP => GetFinalStat(StatType.Health);
    public int TotalCost => GetFinalStat(StatType.Cost);
    public int Evasion => GetFinalStat(StatType.Evasion);

    private int[] baseStats; // 기본 스탯
    private int[] turnDeltaStats; // 1턴(이번 턴) 임시 스탯

    // [추가] 다중 턴 상태이상 리스트 & 방어도 획득 불가 턴 카운터
    public List<StatModifier> activeModifiers = new List<StatModifier>();
    public int cannotGainDefenseTurns = 0;
    
    // 플레이어가 현재 보유 중인 증강체 리스트
    public List<AugmentBase> activeAugments = new List<AugmentBase>();

    [Header("Debuffs")]
    public int currentDotDamage = 0; // 현재 턴당 입는 지속 피해량 (0이면 디버프 없음)

    private Vector3 _originPos;

    public HealthBar hpBar;
    public PowerUI powerUI;
    public CostUI costUI;
    
    [Header("Deck & Card System")]
    public GameObject cardPrefab;      // 생성할 카드 프리팹
    public Transform handContainer;    // 카드가 배치될 UI 부모 (Horizontal Layout Group 권장)
    
    public List<CardObject> masterDeck = new List<CardObject>(); // 전체 덱
    private List<CardObject> drawPile = new List<CardObject>();   // 뽑을 더미
    [SerializeField]
    private List<PlayerCard> handCards = new List<PlayerCard>();  // 현재 손에 든 카드 객체들

    public static PlayerManager instance;

    private bool _running = false;

    [Header("Debuff States")]
    public int lagDebuffTurns = 0; // Lag 디버프 유지 턴 수 (0이면 안 걸린 상태)
    public int lagDebuffValue = 1; // 쿨타임을 얼마나 증가시킬 것인가 (기본 1)
    
    [Header("Sequence Queue System")]
    public Transform sequenceContainer; // 대기열 카드가 표시될 UI 부모
    public List<PlayerCard> sequenceQueue = new List<PlayerCard>(); // 사용 대기열 리스트
    public TextMeshProUGUI sequenceSummaryText;
    public Button executeCombinationBtn; 
    private bool isCombinationUsedThisTurn = false;
    private int previewTotalCost = 0;

    private void Awake()
    {
        int statCount = Enum.GetNames(typeof(StatType)).Length;

        baseStats = new int[statCount];
        turnDeltaStats = new int[statCount];

        baseStats[(int)StatType.Attack] = attackPower;
        baseStats[(int)StatType.Defense] = defensePower;
        baseStats[(int)StatType.Cost] = totalCost;
        baseStats[(int)StatType.Health] = maxHP;
        baseStats[(int)StatType.Evasion] = 0;

        currentHP = baseStats[(int)StatType.Health];
        currentCost = baseStats[(int)StatType.Cost];

        ResetTurnDeltaStats();
        UpdateUI();

        _originPos = transform.position;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    public void SetSceneReferences(Transform container)
    {
        this.handContainer = container;
        Debug.Log($"[PlayerManager] 새로운 씬의 HandContainer 연결 완료: {container.name}");
    }
    
    // 프리뷰에 카드를 넣거나 빼는 토글 함수
    public void ToggleCardInPreview(PlayerCard card)
    {
        if (card.IsInPreview)
        {
            // 빼기 (취소)
            sequenceQueue.Remove(card);
            card.SetPreviewState(false);
            Debug.Log($"[미리보기 취소] {card.cardData.cardName} 제거됨");
        }
        else
        {
            // 넣기 (적용)
            if (sequenceQueue.Count >= 3)
            {
                Debug.Log("조합은 최대 3장까지만 가능합니다.");
                return;
            }
            if (isCombinationUsedThisTurn)
            {
                Debug.Log("이번 턴에는 이미 조합 효과를 사용했습니다.");
                return; 
            }

            sequenceQueue.Add(card);
            card.SetPreviewState(true);
            Debug.Log($"[미리보기 등록] {card.cardData.cardName} 추가됨");
        }

        // 큐 변동 시 즉시 UI 업데이트
        UpdateSequenceSummaryUI();
    }
    
    // 각 카드의 배율을 계산하여 반환
    private Dictionary<PlayerCard, float> GetCardMultipliers()
    {
        Dictionary<PlayerCard, float> multipliers = new Dictionary<PlayerCard, float>();
        
        int roots = 0, vaccines = 0, patches = 0, others = 0;
        
        // 1. 속성별 개수 카운트
        foreach (var c in sequenceQueue)
        {
            if (c.cardData.cardType == CardType.Root) roots++;
            else if (c.cardData.cardType == CardType.Vaccine) vaccines++;
            else if (c.cardData.cardType == CardType.Patch) patches++;
            else others++;
        }

        int otherThanRoot = vaccines + patches + others;

        // 2. 규칙에 따른 배율 결정
        foreach (var c in sequenceQueue)
        {
            CardType t = c.cardData.cardType;
            float mult = 1.0f;

            if (roots == 1 && otherThanRoot > 0) {
                mult = 1.0f; // 루트 1장 + 타 속성 : 변화 없음
            }
            else if (roots >= 2 && otherThanRoot > 0) {
                mult = (t == CardType.Root) ? 1.5f : 0.5f; // 루트 2장 + 타 속성
            }
            else if (roots == sequenceQueue.Count && roots > 0) {
                mult = 1.5f; // 루트만 있을 경우
            }
            else if (roots == 0 && vaccines > 0 && patches > 0) {
                mult = 1.5f; // 루트 없이 백신+패치 조합
            }
            else {
                mult = 1.0f; // 그 외 (동일 속성 통일 등)
            }

            multipliers[c] = mult;
        }

        return multipliers;
    }
    
    // 대기열의 모든 효과를 합산하여 텍스트로 표시하는 함수
    private void UpdateSequenceSummaryUI()
    {
        if (sequenceSummaryText == null) return;
        if (sequenceQueue.Count == 0) 
        { 
            sequenceSummaryText.text = "카드를 선택하여 조합을 확인하세요";
            if (executeCombinationBtn != null) executeCombinationBtn.interactable = false;
            previewTotalCost = 0;
            return;
        }

        // 1. 각 카드의 최종 배율 가져오기
        Dictionary<PlayerCard, float> multipliers = GetCardMultipliers();

        List<string> typeStrings = new List<string>();
        Dictionary<StatType, int> totalDeltas = new Dictionary<StatType, int>();
        previewTotalCost = 0;

        foreach (var c in sequenceQueue)
        {
            float mult = multipliers[c];
            string typeName = GetCardTypeNameKorean(c.cardData.cardType);
            string formattedType = typeName; 

            // 배율에 따른 색상 표기
            if (mult == 1.5f) formattedType = $"<color=#55AAFF>{typeName}</color>"; 
            else if (mult == 0.5f) formattedType = $"<color=#FF5555>{typeName}</color>"; 

            typeStrings.Add(formattedType);

            // 배율이 적용된 최종 수치 계산
            int finalPos = Mathf.RoundToInt(c.posValue * mult);
            int finalNeg = Mathf.RoundToInt(c.negValue * mult);
            previewTotalCost += c.cost;

            StatType pType = c.cardData.positiveStatType;
            StatType nType = c.cardData.negativeStatType;
            
            if (!totalDeltas.ContainsKey(pType)) totalDeltas[pType] = 0;
            if (!totalDeltas.ContainsKey(nType)) totalDeltas[nType] = 0;
            
            totalDeltas[pType] += finalPos;
            totalDeltas[nType] += finalNeg;
        }

        // 2. 최종 텍스트 조립
        string comboSequence = string.Join(" - ", typeStrings);
        StringBuilder sb = new StringBuilder();
        
        sb.Append($"<color=#FFFFFF>현재 조합: {comboSequence}</color>\n");
        sb.Append("<color=#FFFF00>[적용 예정 효과]</color>\n");
        
        foreach (var stat in totalDeltas)
        {
            if (stat.Value == 0) continue;
            string sign = stat.Value > 0 ? "+" : "";
            sb.Append($"{GetStatNameKorean(stat.Key)} {sign}{stat.Value}  ");
        }

        string costColor = (currentCost < previewTotalCost) ? "#FF0000" : "#00FF00";
        sb.Append($"\n<color={costColor}>소모 코스트: {previewTotalCost} / {currentCost}</color>");
        
        sequenceSummaryText.text = sb.ToString();

        // [신규] '효과 적용' 확정 버튼 활성화/비활성화 처리
        if (executeCombinationBtn != null)
        {
            // 코스트가 충분하고, 이번 턴에 조합을 아직 쓰지 않았을 때만 활성화
            executeCombinationBtn.interactable = !isCombinationUsedThisTurn && (currentCost >= previewTotalCost);
        }
    }
    
    // '효과 적용' 버튼을 눌렀을 때 1회 한정으로 실제 적용하는 함수
    public void ExecuteCombination()
    {
        if (isCombinationUsedThisTurn || sequenceQueue.Count == 0) return;
        if (currentCost < previewTotalCost) return;

        isCombinationUsedThisTurn = true; // 턴당 1회 제한 발동

        Dictionary<PlayerCard, float> multipliers = GetCardMultipliers();

        // 복사본 리스트를 만들어 순회 (OnCardUsed에서 리스트가 파괴될 수 있으므로)
        List<PlayerCard> cardsToExecute = new List<PlayerCard>(sequenceQueue);

        foreach (var card in cardsToExecute)
        {
            float mult = multipliers[card];
            int finalPos = Mathf.RoundToInt(card.posValue * mult);
            int finalNeg = Mathf.RoundToInt(card.negValue * mult);

            // 실제 스탯 반영
            AddTurnStatDelta(card.cardData.positiveStatType, finalPos);
            AddTurnStatDelta(card.cardData.negativeStatType, finalNeg);
            
            // 카드 소모 처리
            OnCardUsed(card);
        }

        currentCost -= previewTotalCost;
        sequenceQueue.Clear();
        UpdateSequenceSummaryUI();
        UpdateUI();

        Debug.Log("조합 효과 적용 완료!");
    }

    private void Start()
    {
        InitializeBattleDeck();
    }
    
    public void InitializeBattleDeck()
    {
        masterDeck.Clear(); // 기존 리스트 초기화

        // 1. CardDatabaseManager에서 확정된 덱 데이터 가져오기
        if (CardDatabaseManager.instance != null)
        {
            List<CardObject> savedDeck = CardDatabaseManager.instance.GetCurrentDeck();

            if (savedDeck != null && savedDeck.Count > 0)
            {
                foreach (CardObject cardData in savedDeck)
                {
                    // [중요] 원본 보호를 위해 복사본(Clone)을 생성하여 마스터 덱에 추가
                    CardObject clonedCard = Instantiate(cardData);
                    clonedCard.name = cardData.cardName; // 이름 깔끔하게 정리
                    masterDeck.Add(clonedCard);
                }
                Debug.Log($"[PlayerManager] DB로부터 {masterDeck.Count}장의 카드를 로드하고 복제 완료했습니다.");
            }
            else
            {
                Debug.LogWarning("저장된 덱이 비어있습니다. CardDatabaseManager를 확인하세요.");
            }
        }
        else
        {
            Debug.LogError("CardDatabaseManager 인스턴스를 찾을 수 없습니다!");
        }
        
        drawPile.Clear();
        drawPile.AddRange(masterDeck);
    }
    
    // 리스트 셔플 (Fisher-Yates 알고리즘)
    private void Shuffle(List<CardObject> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i + 1);
            CardObject temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    public int GetBaseStat(StatType type) => baseStats[(int)type];
    public void SetBaseStat(StatType type, int value) => baseStats[(int)type] = value;

    public int GetFinalStat(StatType type)
    {
        int idx = (int)type;
        int multiTurnBonus = 0;

        // [추가] 유지 중인 다중 턴 버프/디버프를 모두 합산합니다.
        foreach (var mod in activeModifiers)
        {
            if (mod.statType == type) multiTurnBonus += mod.amount;
        }

        return Mathf.Max(0, baseStats[idx] + turnDeltaStats[idx] + multiTurnBonus);
    }

    // [추가] 외부(ErrorVirus 등)에서 다중 턴 디버프를 걸 때 사용합니다.
    public void AddMultiTurnStat(StatType type, int amount, int duration, string descriptionText)
    {
        activeModifiers.Add(new StatModifier { statType = type, amount = amount, durationTurns = duration, descriptionText = descriptionText });
        UpdateUI();
    }

    // [추가] 턴 종료/시작 시 호출하여 디버프 지속 시간을 깎습니다.
    public void OnTurnEndProcess()
    {
        isCombinationUsedThisTurn = false;
        sequenceQueue.Clear();
        UpdateSequenceSummaryUI();
        
        // 방어 불가 턴 감소
        if (cannotGainDefenseTurns > 0) cannotGainDefenseTurns--;

        // 💡 [추가] 쿨타임 증가(Lag) 디버프 턴 감소
        if (lagDebuffTurns > 0) lagDebuffTurns--;

        // 역순으로 순회하며 기간이 다 된 디버프 제거
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            activeModifiers[i].durationTurns--;
            if (activeModifiers[i].durationTurns <= 0)
            {
                activeModifiers.RemoveAt(i);
            }
        }
        
        // 활성화된 모든 카드의 쿨타임 1씩 감소
        PlayerCard[] activeCards = FindObjectsOfType<PlayerCard>();
        if (lagDebuffTurns <= 0)
        {
            foreach (PlayerCard card in activeCards)
            {
                card.DecreaseCooldown();
            }
        }
        ResetTurnDeltaStats();
        UpdateUI();
    }

    public void ResetTurnDeltaStats()
    {
        for (int i = 0; i < turnDeltaStats.Length; i++)
        {
            turnDeltaStats[i] = 0;
        }
    }

    // 플레이어 카드의 스탯 임시 적용
    public void AddTurnStatDelta(StatType stat, int value)
    {
        switch (stat)
        {
            case StatType.Attack:
                turnDeltaStats[(int)stat] += value;
                break;

            case StatType.Defense:
                // [수정] 방어력 획득 불가 상태라면 긍정적인(증가) 효과를 무시합니다.
                if (value > 0 && cannotGainDefenseTurns > 0)
                {
                    Debug.Log("방어력 획득 불가 상태로 인해 방어도 증가가 차단되었습니다.");
                    break;
                }
                turnDeltaStats[(int)stat] += value;
                break;

            case StatType.Health:
                if (value > 0)
                {
                    turnDeltaStats[(int)stat] += value;
                    currentHP += value;
                }
                else
                {
                    currentHP = Mathf.Max(0, currentHP + value);
                }
                break;

            case StatType.Cost:
                turnDeltaStats[(int)stat] += value;
                currentCost = Mathf.Max(0, currentCost + value);
                break;

            case StatType.Evasion:
                break;
        }

        UpdateUI();
    }

    public void ApplyCardStats(UpDownMgr.GenerateCard pos, UpDownMgr.GenerateCard neg)
    {
        ModifyStat(pos.stat, pos.valueAmount);
        ModifyStat(neg.stat, -neg.valueAmount);
        UpdateUI();
    }

    private void ModifyStat(StatType stat, int amount)
    {
        switch (stat)
        {
            case StatType.Attack:
                baseStats[(int)stat] = Mathf.Max(0, baseStats[(int)stat] += amount);
                break;

            case StatType.Defense:
                // [수정] 영구 스탯 변경 시에도 방어력 획득 불가 플래그를 체크합니다.
                if (amount > 0 && cannotGainDefenseTurns > 0)
                {
                    Debug.Log("방어력 획득 불가 상태입니다.");
                    break;
                }
                baseStats[(int)stat] = Mathf.Max(0, baseStats[(int)stat] += amount);
                break;

            case StatType.Health:
                if (amount > 0)
                {
                    baseStats[(int)stat] += amount;
                    currentHP += amount;
                }
                else
                {
                    currentHP = Mathf.Max(0, currentHP + amount);
                }
                break;

            case StatType.Cost:
                baseStats[(int)stat] = Mathf.Max(0, baseStats[(int)stat] + amount);
                currentCost = Mathf.Max(0, currentCost + amount);
                break;

            case StatType.Evasion:
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        // int finalDamage = Mathf.Max(0, damage - DefensePower);
        // currentHP = Mathf.Max(0, currentHP - finalDamage);
        //
        // Debug.Log($"받은 데미지: {finalDamage}, 남은 체력: {currentHP}");
        //
        // UpdateUI();
        //
        // if (currentHP <= 0)
        // {
        //     // GameManager.Instance.GameOver(); 
        // }
        
        // 1. 현재 총 방어력 가져오기
        int currentDef = DefensePower;
        
        // 2. 방어력으로 막을 수 있는 데미지와 실제 체력에 들어갈 데미지 계산
        int blockedDamage = Mathf.Min(damage, currentDef);
        int finalDamage = damage - blockedDamage;

        // 3. 방어력 차감 (새로 추가한 ConsumeDefense 로직 호출)
        if (blockedDamage > 0)
        {
            ConsumeDefense(blockedDamage);
        }

        // 4. 남은 데미지만큼 체력 차감
        currentHP = Mathf.Max(0, currentHP - finalDamage);

        Debug.Log($"적 공격: {damage} / 방어됨: {blockedDamage} / 실제 받은 데미지: {finalDamage} / 남은 체력: {currentHP}");

        UpdateUI();

        if (currentHP <= 0)
        {
            GameManager.Instance.GameOver(); 
        }
    }
    
    private void ConsumeDefense(int amount)
    {
        int defIndex = (int)StatType.Defense;

        // 1순위: 이번 턴 임시 방어도 (turnDeltaStats) 먼저 소모
        if (turnDeltaStats[defIndex] > 0)
        {
            int consume = Mathf.Min(amount, turnDeltaStats[defIndex]);
            turnDeltaStats[defIndex] -= consume;
            amount -= consume;
        }

        if (amount <= 0) return;

        // 2순위: 다중 턴 유지 방어도 버프 (activeModifiers) 소모
        for (int i = 0; i < activeModifiers.Count; i++)
        {
            if (activeModifiers[i].statType == StatType.Defense && activeModifiers[i].amount > 0)
            {
                int consume = Mathf.Min(amount, activeModifiers[i].amount);
                activeModifiers[i].amount -= consume;
                amount -= consume;

                if (amount <= 0) return;
            }
        }

        // 3순위: 영구 기본 방어도 (baseStats) 소모
        if (baseStats[defIndex] > 0)
        {
            int consume = Mathf.Min(amount, baseStats[defIndex]);
            baseStats[defIndex] -= consume;
            amount -= consume;
        }
    }
    
    public void PreparePlayerTurn()
    {
        isCombinationUsedThisTurn = false; // 턴 시작 시 사용 제한 초기화
        sequenceQueue.Clear();
        UpdateSequenceSummaryUI();
        
        // 2. 패 버리기, 셔플, 3장 드로우
        ClearHand();
        Shuffle(drawPile);
        DrawCards(3);

        UpdateUI();
    }

    public void StartPlayerTurn()
    {
        if (_running) return;
        if (!GameManager.PlayerTurn) return;

        UpdateUI();

        StartCoroutine(CoPlayerTurnSequence());
    }
    
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 뽑을 카드가 없으면 버린 더미를 다시 섞음
            if (drawPile.Count == 0) return;

            // 카드 생성 및 데이터 할당
            CardObject data = drawPile[0];
            drawPile.RemoveAt(0);

            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            PlayerCard pCard = cardObj.GetComponent<PlayerCard>();
            
            if (pCard != null)
            {
                pCard.SetCardData(data); // 데이터 주입 함수 필요
                handCards.Add(pCard);
            }
        }
        
        if (CardDeckController.instance != null)
        {
            CardDeckController.instance.RefreshHandLayout(handCards);
            CardDeckController.instance.UpdateDeckUI(drawPile.Count, 0); // 버린 더미는 없으므로 0
        }
    }

    // 카드를 사용하거나 턴이 끝날 때 호출
    public void OnCardUsed(PlayerCard card)
    {
        if (handCards.Contains(card))
        {
            // [핵심] 사용한 카드 데이터를 버리지 않고 덱에 바로 다시 넣습니다.
            drawPile.Add(card.cardData); 
            
            handCards.Remove(card);
            Destroy(card.gameObject);

            // 레이아웃 갱신
            if (CardDeckController.instance != null)
            {
                CardDeckController.instance.RefreshHandLayout(handCards);
                CardDeckController.instance.UpdateDeckUI(drawPile.Count, 0);
            }
        }
    }

    private void ClearHand()
    {
        // 리스트를 역순으로 순회하며 오브젝트 파괴 및 덱 복구
        for (int i = handCards.Count - 1; i >= 0; i--)
        {
            if (handCards[i] != null)
            {
                // 사용하지 않은 카드 데이터를 다시 덱에 넣음
                drawPile.Add(handCards[i].cardData);
                Destroy(handCards[i].gameObject);
            }
        }
        handCards.Clear();

        // [중요] 패가 비었음을 레이아웃 컨트롤러에 알림
        if (CardDeckController.instance != null)
        {
            CardDeckController.instance.RefreshHandLayout(handCards);
        }
    }

    private IEnumerator CoPlayerTurnSequence()
    {
        Debug.Log("플레이어 턴 시작");
        _running = true;

        int remainingDamage = AttackPower;

        Virus[] enemies = FindObjectsOfType<Virus>();
        System.Array.Sort(enemies, (a, b) => a.spawnNum.CompareTo(b.spawnNum));

        for (int i = 0; i < enemies.Length; i++)
        {
            if (remainingDamage <= 0) break;

            Virus enemy = enemies[i];
            if (enemy == null) continue;
            if (enemy.virusData.CurHpCnt <= 0) continue;

            yield return StartCoroutine(CoAttack(enemy));

            remainingDamage = enemy.ApplyDamage(remainingDamage);
        }

        _running = false;
        GameManager.PlayerTurn = false;

        Debug.Log("플레이어 턴 종료");
    }

    protected IEnumerator CoAttack(Virus enemy)
    {
        Transform enemyTr = enemy.transform;
        if (enemyTr == null) yield break;

        Vector3 start = _originPos;
        Vector3 target = enemyTr.position;
        target.y = start.y;

        yield return LerpPos(start, target, 0.3f);
        yield return new WaitForSeconds(0.1f);
        transform.position = start;
    }

    private IEnumerator LerpPos(Vector3 start, Vector3 target, float dur)
    {
        float t = 0f;
        dur = Mathf.Max(0.0001f, dur);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(start, target, easedT);
            yield return null;
        }
        transform.position = target;
    }

    public void UpdateUI()
    {
        if (hpBar != null) hpBar.UpdateHPBar(currentHP, MaxHP);
        if (powerUI != null) powerUI.UpdateAttackPowerUI(AttackPower);
        if (powerUI != null) powerUI.UpdateDefensePowerUI(DefensePower);
        if (costUI != null) costUI.UpdateCostUI(currentCost, TotalCost);

        // 💡 [추가] 턴 종료/시작 등으로 스탯 변경이 끝난 후 상태 아이콘을 1번만 갱신합니다.
        if (PlayerStatusUI.instance != null) PlayerStatusUI.instance.RefreshStatusUI();
    }
    
    // 1. 증강체를 새로 획득했을 때 호출할 함수
    public void AcquireAugment(AugmentBase newAugment)
    {
        activeAugments.Add(newAugment);
        Debug.Log($"증강체 획득 완료: {newAugment.augmentName}");

        // 획득 즉시 발동해야 하는 효과(OnEquip) 처리
        BattleContext context = new BattleContext 
        {
            player = this,
            // 주의: 실제 게임 덱을 관리하는 리스트를 넘겨야 합니다.
            cards = new List<PlayerCard>(FindObjectsOfType<PlayerCard>()), 
            viruses = null 
        };
        
        newAugment.OnEquip(context);
    }
    
    // 2. 전투 시작 시 호출할 함수 (GameManager 등에서 호출)
    public void TriggerBattleStart(List<PlayerCard> currentHand, List<Virus> currentMonsters)
    {
        BattleContext context = new BattleContext 
        {
            player = this,
            cards = currentHand,
            viruses = currentMonsters
        };

        foreach (var augment in activeAugments)
        {
            augment.OnBattleStart(context);
        }
    }
    
    // 3. 몬스터 처치 시 호출할 함수 (Virus 스크립트에서 호출)
    public void TriggerVirusKilled(Virus killedVirus)
    {
        BattleContext context = new BattleContext 
        {
            player = this,
            cards = null, // 처치 시점엔 카드가 필요 없을 수 있음
            viruses = new List<Virus> { killedVirus } 
        };

        foreach (var augment in activeAugments)
        {
            augment.OnVirusKilled(context);
        }
    }
    
    public void AddCardToDeck(CardObject originalCardData)
    {
        CardObject clonedCard = Instantiate(originalCardData);
        clonedCard.name = originalCardData.cardName; 
        masterDeck.Add(clonedCard);
    }
    
    // ✅ 외부(증강체 등)에서 플레이어의 영구 스탯을 올리고 내릴 때 사용하는 함수
    public void AddPermanentStat(StatType type, int amount)
    {
        // 1. baseStats 배열에 적용 
        int index = (int)type;
        if (baseStats != null && index >= 0 && index < baseStats.Length)
        {
            baseStats[index] += amount;
        }

        // 2. Inspector에 노출된 기본 변수들도 함께 동기화
        switch (type)
        {
            case StatType.Attack:
                attackPower = Mathf.Max(0, attackPower + amount);
                break;
            case StatType.Defense:
                defensePower = Mathf.Max(0, defensePower + amount);
                break;
            case StatType.Health:
                maxHP = Mathf.Max(1, maxHP + amount); // 최대 체력은 1 밑으로 내려가지 않음
                currentHP = Mathf.Clamp(currentHP + amount, 1, maxHP); // 최대 체력이 깎이면 현재 체력도 동기화
                break;
            case StatType.Cost:
                totalCost = Mathf.Max(0, totalCost + amount);
                currentCost = Mathf.Clamp(currentCost + amount, 0, totalCost);
                break;
        }
    }
    
    private string GetStatNameKorean(StatType type)
    {
        switch (type)
        {
            case StatType.Attack: return "공격력";
            case StatType.Defense: return "방어력";
            case StatType.Health: return "체력";
            case StatType.Cost: return "코스트";
            default: return type.ToString();
        }
    }
    
    private string GetCardTypeNameKorean(CardType type)
    {
        switch (type)
        {
            case CardType.Vaccine: return "백신";
            case CardType.Patch: return "패치";
            case CardType.Root: return "루트";
            default: return "일반"; // CardType.None 등
        }
    }
}