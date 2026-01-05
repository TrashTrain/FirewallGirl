//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EliteVirusIdle : BaseState
//{
//    public EliteVirusIdle(Virus virus) : base(virus) { }

//    public override void OnStateEnter()
//    {
//        Debug.Log("Idle 상태입니다.");
//        _virus.animator.SetBool("isAttack", false);
//        _virus.animator.SetBool("isDef", false);
//        _virus.animator.SetBool("isSup", false);
//        Debug.Log(_virus.virusObjectSO.name);
//        _virus.GetRandState();
//    }

//    public override void OnStateUpdate()
//    {
//        OnStateExit();
//    }

//    public override void OnStateExit()
//    {
//        Debug.Log("Idle Exit 입니다.");
//    }
//}

//public class EliteVirusAtk : BaseState
//{
//    public EliteVirusAtk(Virus virus) : base(virus) { }
//    public override void OnStateEnter()
//    {
//        Debug.Log("ATK 상태입니다.");
//        GameManager.PlayerTurn = false;
//        _virus.animator.SetBool("isAttack", true);
//        PlayerManager.instance.TakeDamage(_virus.virusData.AtkDmg);
//    }

//    public override void OnStateUpdate()
//    {

//    }

//    public override void OnStateExit()
//    {

//        Debug.Log("passTurn");
//        //GameManager.PlayerTurn = true;
//        _virus.animator.SetBool("isAttack", false);

//    }
//}

//public class EliteVirusDef : BaseState
//{
//    public EliteVirusDef(Virus virus) : base(virus) { }

//    public override void OnStateEnter()
//    {
//        Debug.Log("DEF 상태입니다.");
//        GameManager.PlayerTurn = false;
//        _virus.animator.SetBool("isDef", true);
//    }

//    public override void OnStateUpdate()
//    {

//    }

//    public override void OnStateExit()
//    {
//        Debug.Log("passTurn");
//        _virus.animator.SetBool("isDef", false);
//        //GameManager.PlayerTurn = true;
//        //Troy.sequenceCheck = 1;
//    }
//}

//public class EliteVirusSup : BaseState
//{
//    public EliteVirusSup(Virus virus) : base(virus) { }

//    public override void OnStateEnter()
//    {
//        Debug.Log("SUP 상태입니다.");
//        //_virus.virusData.AtkDmg += 3;
//        _virus.ChangeAtkValue(3);
//        Debug.Log("atkDmg : " + _virus.virusData.AtkDmg);
//        //_virus.WaitTime();
//        _virus.UpdateData();
//        GameManager.PlayerTurn = false;
//        _virus.animator.SetBool("isSup", true);
//    }

//    public override void OnStateUpdate()
//    {

//    }

//    public override void OnStateExit()
//    {
//        Debug.Log("passTurn");
//        _virus.animator.SetBool("isSup", false);
//        //GameManager.PlayerTurn = true;
//        //Troy.sequenceCheck = 1;
//    }
//}
//public class EliteVirusDeath : BaseState
//{
//    public EliteVirusDeath(Virus virus) : base(virus) { }

//    public override void OnStateEnter()
//    {
//        GameManager.Instance.enemyCount--;
//        if (GameManager.Instance.enemyCount == 0)
//            GameManager.Instance.GameOver();
//        Debug.Log("DEATH 상태입니다.");
//    }

//    public override void OnStateUpdate()
//    {
//    }

//    public override void OnStateExit()
//    {
//        Debug.Log("passTurn");
//        GameManager.PlayerTurn = true;
//        //Troy.sequenceCheck = 1;
//    }
//}