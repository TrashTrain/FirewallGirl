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
    private int indexNum;
    public SpriteRenderer image;

    public TextMeshPro atkDmgText;
    public TextMeshPro hpCntText;

    [Header("실제 데이터")]
    public int atkDmg;
    public int hpCnt;

    public void InitData()
    {
        // 전부 업데이트에 할당할 필요는 없음.
        indexNum = virusObjectSO.virusIndex;
        image.sprite = virusObjectSO.virusImage;
        atkDmg = virusObjectSO.virusAtk;
        hpCnt = virusObjectSO.virusHp;
        atkDmgText.text = atkDmg.ToString();
        hpCntText.text = hpCnt.ToString();
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
