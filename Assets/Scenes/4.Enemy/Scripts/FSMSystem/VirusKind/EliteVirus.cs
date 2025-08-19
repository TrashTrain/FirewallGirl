using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteVirus : Virus
{
    public enum State
    {
        Idle,
        Atk,
        Def,
        Sup,
        Death
    }


    private State _curState;
    private FSM _fsm;
    public float sequence;


    void Start()
    {
        Debug.Log("InTroy");
        InitData();
        
        _fsm = new FSM(new EliteVirusIdle(this));
        ChangeState(State.Idle);
    }

    public State GetState()
    {
        return _curState;
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log("PlayerTurn : " + GameManager.PlayerTurn);
        if (GameManager.PlayerTurn)
            return;
        if (virusData.HpCnt <= 0)
        {
            Destroy(gameObject);
        }


        switch (_curState)
        {
            case State.Idle:
                if (CanMoveVirus())
                {
                    ChangeState((State)RandState);
                }
                break;
            case State.Atk:
                if (CanMoveVirus())
                {
                    ChangeState(State.Idle);
                }
                break;
            case State.Def:
                if (CanMoveVirus())
                {
                    ChangeState(State.Idle);
                }
                break;
            case State.Sup:
                if (CanMoveVirus())
                {
                    ChangeState(State.Idle);
                }
                break;
        }

        _fsm.UpdateState();

    }

    private void ChangeState(State nexState)
    {
        Debug.Log("ChangeState");
        _curState = nexState;
        switch (_curState)
        {
            case State.Idle:
                _fsm.ChangeState(new EliteVirusIdle(this));
                break;
            case State.Atk:
                _fsm.ChangeState(new EliteVirusAtk(this));
                break;
            case State.Def:
                _fsm.ChangeState(new EliteVirusDef(this));
                break;
            case State.Sup:
                _fsm.ChangeState(new EliteVirusSup(this));
                break;
            case State.Death:
                _fsm.ChangeState(new EliteVirusDeath(this));
                break;
        }

    }
    private bool CanMoveVirus()
    {
        // virus의 턴이 왔을 경우 참.
        return !GameManager.PlayerTurn;
    }
    public void ChangeTurn()
    {
        GameManager.PlayerTurn = true;
    }
}
