using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Augments/PlayerStatRandomize")]
public class PlayerStatRandomizeAugment : AugmentBase
{
    private StatType savedIncreaseStat;
    private StatType savedDecreaseStat;
    private int savedIncreaseValue;
    private int savedDecreaseValue;
    
    public override void Initialize()
    {
        savedIncreaseStat = GetRandomStatType(null);         
        savedDecreaseStat = GetRandomStatType(savedIncreaseStat); 
        
        savedIncreaseValue = GetRandomValue(savedIncreaseStat, true);
        savedDecreaseValue = GetRandomValue(savedDecreaseStat, false);

        string incStatName = GetStatNameKorean(savedIncreaseStat);
        string decStatName = GetStatNameKorean(savedDecreaseStat);
        
        description = $"{incStatName} {savedIncreaseValue} 증가, {decStatName} {Mathf.Abs(savedDecreaseValue)} 감소";
    }
    
    public override void OnEquip(BattleContext context)
    {
        PlayerManager player = context.player;
        if (player == null) return;

        player.AddPermanentStat(savedIncreaseStat, savedIncreaseValue);
        player.AddPermanentStat(savedDecreaseStat, savedDecreaseValue);

        Debug.Log($"[랜덤 증강체 발동] {description}");
        player.UpdateUI();
    }
    
    // 🎯 확률 가중치를 적용하여 무작위 스탯을 뽑는 함수
    private StatType GetRandomStatType(StatType? excludeStat)
    {
        List<StatType> pool = new List<StatType>();

        // 통에 넣을 가중치 (공격/방어/체력 = 3개씩, 코스트 = 1개씩)
        AddStatToPool(pool, StatType.Attack, 3, excludeStat);
        AddStatToPool(pool, StatType.Defense, 3, excludeStat);
        AddStatToPool(pool, StatType.Health, 3, excludeStat);
        AddStatToPool(pool, StatType.Cost, 1, excludeStat);

        // 통에서 무작위로 하나 뽑기
        if (pool.Count > 0)
        {
            int randomIndex = Random.Range(0, pool.Count);
            return pool[randomIndex];
        }
        
        return StatType.Attack; // 예외 상황용 기본값
    }
    
    // 제비뽑기 통에 가중치만큼 스탯을 넣는 함수
    private void AddStatToPool(List<StatType> pool, StatType type, int weight, StatType? exclude)
    {
        // 이미 다른 쪽에서 뽑힌 스탯이면 통에 넣지 않음
        if (exclude.HasValue && type == exclude.Value) return;

        for (int i = 0; i < weight; i++)
        {
            pool.Add(type);
        }
    }
    
    // 🎯 조건에 맞춰 0이 아닌 랜덤 증감 수치를 반환하는 함수
    private int GetRandomValue(StatType type, bool isIncrease)
    {
        // 참고: Random.Range(min, max)에서 정수(int)를 사용할 때 max 값은 제외됩니다.
        switch (type)
        {
            case StatType.Attack:
            case StatType.Defense:
                // 증가: 1 또는 2 / 감소: -2 또는 -1
                return isIncrease ? Random.Range(1, 3) : Random.Range(-2, 0);

            case StatType.Health:
                // 증가: 1 ~ 5 / 감소: -5 ~ -1
                return isIncrease ? Random.Range(1, 6) : Random.Range(-5, 0);

            case StatType.Cost:
                // 코스트는 증감 무조건 +1 / -1
                return isIncrease ? 1 : -1;

            default:
                return isIncrease ? 1 : -1;
        }
    }
    
    private string GetStatNameKorean(StatType type)
    {
        switch (type)
        {
            case StatType.Attack: return "공격력";
            case StatType.Defense: return "방어력";
            case StatType.Health: return "최대 체력";
            case StatType.Cost: return "코스트";
            case StatType.Evasion: return "회피율";
            default: return type.ToString();
        }
    }
}
