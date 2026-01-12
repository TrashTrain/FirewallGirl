using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Troy : Virus
{
    void Start()
    {
        Debug.Log("InTroy");
        InitData();

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
        
    }

}
