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
    private int defCnt;
    private int hpCnt;
    private int curHpCnt;

    public VirusData(int virusNum, Sprite virusImage, string virusName, int atkDmg, int defCnt, int hpCnt, int curHpCnt)
    {
        this.VirusNum = virusNum;
        this.VirusImage = virusImage;
        this.VirusName = virusName;
        this.AtkDmg = atkDmg;
        this.DefCnt = defCnt;
        this.HpCnt = hpCnt;
        this.curHpCnt = curHpCnt;
    }

    public int VirusNum { get => virusNum; set => virusNum = value; }
    public Sprite VirusImage { get => virusImage; set => virusImage = value; }
    public string VirusName { get => virusName; set => virusName = value; }
    public int AtkDmg { get => atkDmg; set => atkDmg = value; }
    public int DefCnt { get => defCnt; set => defCnt = value; }
    public int HpCnt { get => hpCnt; set => hpCnt = value; }
    public int CurHpCnt { get => curHpCnt; set => curHpCnt = value; }
}
