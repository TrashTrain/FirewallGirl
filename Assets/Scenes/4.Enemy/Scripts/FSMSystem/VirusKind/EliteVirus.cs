using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteVirus : Virus
{

    private State _curState;
    private FSM _fsm;
    public float sequence;


    void Start()
    {
        Debug.Log("InTroy");
        InitData();
        
    }

    public State GetState()
    {
        return _curState;
    }
    // Update is called once per frame
    void Update()
    {

        enemyUIController.state.UpdateStateImage((Troy.State)RandState);
        Debug.Log("PlayerTurn : " + GameManager.PlayerTurn);
        if (GameManager.PlayerTurn)
            return;
        if (virusData.HpCnt <= 0)
        {
            Destroy(gameObject);
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
