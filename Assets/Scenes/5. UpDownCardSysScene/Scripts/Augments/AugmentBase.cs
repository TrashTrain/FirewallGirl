using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleContext
{
    public PlayerManager player;
    public List<PlayerCard> cards;
    public List<Virus> viruses;
}

public abstract class AugmentBase : ScriptableObject
{
    public string augmentName;
    [TextArea] public string description;
    public Sprite icon;
    
    public virtual void Initialize() { }

    public virtual void OnEquip(BattleContext context) { } // 증강체 획득 시 발동
    public virtual void OnBattleStart(BattleContext context) { } // 전투 시작 시 발동
    public virtual void OnVirusKilled(BattleContext context) { } // 몬스터 처치 시 발동
    
}
