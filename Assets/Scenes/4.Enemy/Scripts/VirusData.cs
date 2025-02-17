using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VirusData
{
    private int virusNum;
    private Sprite virusImage;
    private string virusName;

    private int atkDmg;
    private int hpCnt;

    public VirusData(int virusNum, Sprite virusImage, string virusName, int atkDmg, int hpCnt)
    {
        this.VirusNum = virusNum;
        this.VirusImage = virusImage;
        this.VirusName = virusName;
        this.AtkDmg = atkDmg;
        this.HpCnt = hpCnt;
    }

    public int VirusNum { get => virusNum; set => virusNum = value; }
    public Sprite VirusImage { get => virusImage; set => virusImage = value; }
    public string VirusName { get => virusName; set => virusName = value; }
    public int AtkDmg { get => atkDmg; set => atkDmg = value; }
    public int HpCnt { get => hpCnt; set => hpCnt = value; }
}
