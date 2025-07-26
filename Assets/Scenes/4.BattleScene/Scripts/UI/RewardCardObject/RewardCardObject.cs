using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    ATK, // 공격력
    DEF, // 방어력
    HP, // 체력
    AVO, // 회피율
    COS // 코스트
}

[CreateAssetMenu(fileName = "RewardCardData", menuName = "CreateCardData/RewardCardData")]
public class RewardCardObject : ScriptableObject
{
    public int cardIndex;
    public Sprite cardImage;
    public string cardName;

    public int effectNum;
    
    [TextArea]
    public string description;

    public EffectType effectType;
}
