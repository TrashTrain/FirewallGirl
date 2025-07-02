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
