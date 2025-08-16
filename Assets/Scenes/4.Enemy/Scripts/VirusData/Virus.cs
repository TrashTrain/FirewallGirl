using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 바이러스 종류간의 차이가 수치뿐이라면 굳이 종류별로 FSM을 나누지 말고 Virus에 합쳐도 됨.
public class Virus : MonoBehaviour
{
    [Header("SO데이터")]
    public VirusObjectSO virusObjectSO;

    [Header("내부 데이터")]
    public SpriteRenderer image;

    public TextMeshPro atkDmgText;
    public TextMeshPro hpCntText;

    [HideInInspector]
    public VirusData virusData;

    public void InitData()
    {
        virusData = new VirusData(virusObjectSO.virusIndex, virusObjectSO.virusImage, virusObjectSO.virusName, virusObjectSO.virusAtk, virusObjectSO.virusHp);
        gameObject.GetComponent<SpriteRenderer>().sprite = virusData.VirusImage;
        atkDmgText.text = virusData.AtkDmg.ToString();
        hpCntText.text = virusData.HpCnt.ToString();
    }

    public void UpdateData()
    {
        atkDmgText.text = virusData.AtkDmg.ToString();
    }
    private enum State
    {
        Idle,
        Atk,
        Def,
        Sup,
        Death
    }

    private State _state;

    private void Start()
    {
        if (VirusMgr.instance == null)
        {
            Debug.LogError("CardMgr.instance가 초기화되지 않았습니다.");
            return;
        }
        _state = State.Idle;

        InitData();
    }

    private void Update()
    {
        //InitData();
        switch (_state)
        {
            case State.Idle:
                break;
            case State.Atk:
                break; 
            case State.Def:
                break;
            case State.Sup:
                break;
            case State.Death:
                break;
            default:
                break;
        }
    }
    public int ChangeStateRand(int endNum)
    {
        int check = Random.Range(1, endNum);
        return check;
    }
}
