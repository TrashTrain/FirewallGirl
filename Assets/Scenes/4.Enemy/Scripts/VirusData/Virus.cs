using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ���̷��� �������� ���̰� ��ġ���̶�� ���� �������� FSM�� ������ ���� Virus�� ���ĵ� ��.
public class Virus : MonoBehaviour
{
    [Header("SO������")]
    public VirusObjectSO virusObjectSO;

    [Header("���� ������")]
    private int indexNum;
    public SpriteRenderer image;

    public TextMeshPro atkDmgText;
    public TextMeshPro hpCntText;

    [Header("���� ������")]
    public int atkDmg;
    public int hpCnt;

    public void InitData()
    {
        // ���� ������Ʈ�� �Ҵ��� �ʿ�� ����.
        indexNum = virusObjectSO.virusIndex;
        image.sprite = virusObjectSO.virusImage;
        atkDmg = virusObjectSO.virusAtk;
        hpCnt = virusObjectSO.virusHp;
        atkDmgText.text = atkDmg.ToString();
        hpCntText.text = hpCnt.ToString();
    }

    public void UpdateData()
    {
        atkDmgText.text = atkDmg.ToString();
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
            Debug.LogError("CardMgr.instance�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
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
