using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Idle, ATK, DEF, SUP
}

public enum VirusKind
{
    Troy,
    Worm,
    SpyWare
}

[CreateAssetMenu(fileName = "VirusData", menuName = "CreateCardData/VirusData")]
public class VirusObjectSO : ScriptableObject
{
    public int virusIndex;
    public Sprite virusImage;
    public string virusName;

    public int virusAtk;
    public int virusHp;
    
    // 외부 코드로 타입을 수정할 수 있는지 확인할 것.
    public VirusKind AttackType;
}
