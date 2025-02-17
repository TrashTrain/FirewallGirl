using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    ATK, DEF, SUP
}

[CreateAssetMenu(fileName = "VirusData", menuName = "CreateCardData/VirusData")]
public class VirusObjectSO : ScriptableObject
{
    public int virusIndex;
    public Sprite virusImage;
    public string virusName;

    public int virusAtk;
    public int virusHp;
}
