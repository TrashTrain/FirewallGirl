using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Player info
    public int maxHP;
    public int currentHP;
    public int currentCost;
    public int totalCost;
    public int attackPower;
    public int defensePower;
    
    public int AttackPower => GetFinalStat(StatType.Attack);
    public int DefensePower => GetFinalStat(StatType.Defense);
    public int MaxHP => GetFinalStat(StatType.Health);
    public int TotalCost => GetFinalStat(StatType.Cost);
    public int Evasion => GetFinalStat(StatType.Evasion);
    

    private int[] baseStats; // 턴 종료 전 기본 스탯
    private int[] turnDeltaStats; // 턴 종료 전까지 적용될 임시 스탯

    private Vector3 _originPos;

    public HealthBar hpBar;
    public PowerUI powerUI;
    public CostUI costUI;

    public static PlayerManager instance;
    
    private bool _running = false;

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
            // 씬이 바뀌어도 이 오브젝트는 파괴되지 않습니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
            return;
        }
    }
    
    public int GetBaseStat(StatType type) => baseStats[(int)type];
    public void SetBaseStat(StatType type, int value) => baseStats[(int)type] = value;

    public int GetFinalStat(StatType type)
    {
        int idx = (int)type;
        return Mathf.Max(0, baseStats[idx] + turnDeltaStats[idx]);
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
            case StatType.Attack or StatType.Defense:
                // 공격력은 0보다 작아질 수 없음
                turnDeltaStats[(int)stat] += value;
                break;

            case StatType.Health:
                if (value > 0) // 긍정 효과 (+)
                {
                    // 최대 체력과 현재 체력을 동시에 증가시킴 (예: 80/100 -> 83/103)
                    turnDeltaStats[(int)stat] += value;
                    currentHP += value;
                }
                else // 부정 효과 (-)
                {
                    // 체력 감소 시 0보다 작아질 수 없음
                    currentHP = Mathf.Max(0, currentHP + value);
                    // (선택사항) 최대 체력도 깎고 싶다면: maxHP = Mathf.Max(1, maxHP + amount);
                }
                break;

            case StatType.Cost:
                // 코스트 전체 총량 변경
                turnDeltaStats[(int)stat] += value;
                // 현재 코스트도 0보다 작아질 수 없음
                currentCost = Mathf.Max(0, currentCost + value);
                break;

            case StatType.Evasion:
                // 회피율(필요 시 변수 추가)도 0보다 작아질 수 없음
                // evasionRate = Mathf.Max(0, evasionRate + amount);
                break;
        }

        UpdateUI();
    }

    // 카드의 스탯을 실제로 반영하는 함수 (증강체)
    public void ApplyCardStats(UpDownMgr.GenerateCard pos, UpDownMgr.GenerateCard neg)
    {
        // 긍정 효과 적용
        ModifyStat(pos.stat, pos.valueAmount);
        // 부정 효과 적용 (GenerateCard의 ToString이나 내부 로직에 따라 -값이 필요함)
        ModifyStat(neg.stat, -neg.valueAmount);

        UpdateUI();
    }

    private void ModifyStat(StatType stat, int amount)
    {
        switch (stat)
        {
            case StatType.Attack:
                // 공격력은 0보다 작아질 수 없음
                // attackPower = Mathf.Max(0, attackPower += amount);
                baseStats[(int)stat] = Mathf.Max(0, baseStats[(int)stat] += amount);
                break;

            case StatType.Defense:
                // 방어력은 0보다 작아질 수 없음
                // defensePower = Mathf.Max(0, defensePower += amount);
                baseStats[(int)stat] = Mathf.Max(0, baseStats[(int)stat] += amount);
                break;

            case StatType.Health:
                if (amount > 0) // 긍정 효과 (+)
                {
                    // 최대 체력과 현재 체력을 동시에 증가시킴 (예: 80/100 -> 83/103)
                    baseStats[(int)stat] += amount;
                    // maxHP += amount;
                    currentHP += amount;
                }
                else // 부정 효과 (-)
                {
                    // 체력 감소 시 0보다 작아질 수 없음
                    currentHP = Mathf.Max(0, currentHP + amount);
                    // (선택사항) 최대 체력도 깎고 싶다면: maxHP = Mathf.Max(1, maxHP + amount);
                }
                break;

            case StatType.Cost:
                // 코스트 전체 총량 변경
                baseStats[(int)stat] = Mathf.Max(0, baseStats[(int)stat] + amount);
                // 현재 코스트도 0보다 작아질 수 없음
                currentCost = Mathf.Max(0, currentCost + amount);
                break;

            case StatType.Evasion:
                // 회피율(필요 시 변수 추가)도 0보다 작아질 수 없음
                // evasionRate = Mathf.Max(0, evasionRate + amount);
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        // 방어력을 뺀 최종 데미지 계산 (방어력이 더 높더라도 최소 0)
        int finalDamage = Mathf.Max(0, damage - DefensePower);

        // 체력 감소 (0 미만 제한)
        currentHP = Mathf.Max(0, currentHP - finalDamage);

        Debug.Log($"받은 데미지: {finalDamage}, 남은 체력: {currentHP}");

        UpdateUI();

        if (currentHP <= 0)
        {
            // GameManager.Instance.GameOver(); // 게임 오버 로직 호출
        }
    }

    public void StartPlayerTurn()
    {
        if (_running)
        {
            return;
        }

        if (!GameManager.PlayerTurn)
        {
            return;
        }
        
        // [ToDo] 턴 시작 전 카드 적용으로 증감된 임시 스탯 저장 (턴 종료 후 초기화)

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
            if (remainingDamage <= 0)
            {
                break;
            }
            
            Virus enemy =  enemies[i];
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
        // 목표: 적 위치까지 갔다가 복귀
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
        // 씬이 바뀌면 UI 참조가 끊길 수 있으므로 체크 후 업데이트
        // if (hpBar != null) hpBar.UpdateHPBar(currentHP, maxHP);
        // if (powerUI != null) powerUI.UpdateAttackPowerUI(attackPower);
        // if (powerUI != null) powerUI.UpdateDefensePowerUI(defensePower);
        // if (costUI != null) costUI.UpdateCostUI(currentCost, totalCost);
        
        if (hpBar != null) hpBar.UpdateHPBar(currentHP, MaxHP);
        if (powerUI != null) powerUI.UpdateAttackPowerUI(AttackPower);
        if (powerUI != null) powerUI.UpdateDefensePowerUI(DefensePower);
        if (costUI != null) costUI.UpdateCostUI(currentCost, TotalCost);
    }
}
