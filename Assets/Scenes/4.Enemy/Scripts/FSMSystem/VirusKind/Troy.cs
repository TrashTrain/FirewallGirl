using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troy : Virus
{
    private enum State
    {
        Idle,
        Atk,
        Def,
        Sup,
        Death
    }


    private State _curState;
    private FSM _fsm;

    void Start()
    {
        Debug.Log("InTroy");
        _curState = State.Idle;
        _fsm = new FSM(new VirusIdle(this));
    }

    // Update is called once per frame
    void Update()
    {
        InitData();
        switch (_curState)
        {
            case State.Idle:
                if(hpCnt <= 0)
                {
                    ChangeState(State.Death);
                }
                if (CanMoveVirus())
                {
                    ChangeState((State)ChangeStateRand((int)State.Death));
                }
                break;
            case State.Atk:
                if (hpCnt <= 0)
                {
                    ChangeState(State.Death);
                }
                if (!CanMoveVirus())
                {
                    ChangeState(State.Idle);
                }
                break;
            case State.Def:
                if (hpCnt <= 0)
                {
                    ChangeState(State.Death);
                }
                if (!CanMoveVirus())
                {
                    ChangeState(State.Idle);
                }
                break;
            case State.Sup:
                if (hpCnt <= 0)
                {
                    ChangeState(State.Death);
                }
                if (!CanMoveVirus())
                {
                    ChangeState(State.Idle);
                }
                break;
        }

        _fsm.UpdateState();
    }

    private void ChangeState(State nexState)
    {
        _curState = nexState;
        switch (_curState)
        {
            case State.Idle:
                _fsm.ChangeState(new VirusIdle(this));
                break;
            case State.Atk:
                _fsm.ChangeState(new VirusAtk(this));
                break;
            case State.Def:
                _fsm.ChangeState(new VirusDef(this));
                break;
            case State.Sup:
                _fsm.ChangeState(new VirusSup(this));
                break;
            case State.Death:
                _fsm.ChangeState(new VirusDeath(this));
                break;
        }
    }
    private bool CanMoveVirus()
    {
        // virus의 턴이 왔을 경우 참.
        return !GameManager.PlayerTurn;
    }
}
