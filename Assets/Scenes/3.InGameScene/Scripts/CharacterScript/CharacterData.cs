using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    public int attackPower = 10;  // 공격력
    public int defensePower = 5; // 방어력
    public int cost = 100;       // 코스트
}

public class CharacterData : MonoBehaviour
{
    public CharacterStats stats;

    private void OnValidate()
    {   
        // 하이어라키에서 값이 변경되면 자동으로 업데이트
        stats.attackPower = Mathf.Max(0, stats.attackPower);
        stats.defensePower = Mathf.Max(0, stats.defensePower);
        stats.cost = Mathf.Max(0, stats.cost);
    }
}