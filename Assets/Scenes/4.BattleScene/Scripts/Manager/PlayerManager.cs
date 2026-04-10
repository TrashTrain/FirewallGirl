using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [추가] 여러 턴 동안 유지되는 상태 이상을 관리하기 위한 클래스
[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public int amount;
    public int durationTurns; // 남은 유지 턴 수
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
    public List<CardObject> masterDeck = new List<CardObject>();

    private Vector3 _originPos;

    public HealthBar hpBar;
    public PowerUI powerUI;
    public CostUI costUI;

    public static PlayerManager instance;

    private bool _running = false;

    [Header("Debuff States")]
    public int lagDebuffTurns = 0; // Lag 디버프 유지 턴 수 (0이면 안 걸린 상태)
    public int lagDebuffValue = 1; // 쿨타임을 얼마나 증가시킬 것인가 (기본 1)

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
    public void AddMultiTurnStat(StatType type, int amount, int duration)
    {
        activeModifiers.Add(new StatModifier { statType = type, amount = amount, durationTurns = duration });
        UpdateUI();
    }

    // [추가] 턴 종료/시작 시 호출하여 디버프 지속 시간을 깎습니다.
    public void OnTurnEndProcess()
    {
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
            // GameManager.Instance.GameOver(); 
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

    public void StartPlayerTurn()
    {
        if (_running) return;
        if (!GameManager.PlayerTurn) return;

        StartCoroutine(CoPlayerTurnSequence());
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
}