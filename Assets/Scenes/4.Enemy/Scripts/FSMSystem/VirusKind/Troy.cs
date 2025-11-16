using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Troy : Virus
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
    public int sequence = 0;


    void Start()
    {
        Debug.Log("InTroy");
        InitData();

        _fsm = new FSM(new VirusIdle(this));
        ChangeState(State.Idle);
        
    }

    public State GetState()
    {
        return _curState;
    }


    void Update()
    {
        Debug.Log("PlayerTurn : " + GameManager.PlayerTurn);
        if (virusData.HpCnt <= 0)
        {
            Destroy(gameObject);
        }
        enemyUIController.state.UpdateStateImage((State)RandState);
        if (GameManager.PlayerTurn)
            return;
        
        //if (sequence >= 1)
        //    return;

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
    public void ChangeTurn()
    {
        GameManager.PlayerTurn = true;
    }
}
