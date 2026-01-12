//// virus의 SO형식을 받아와서 행동하기.
//using UnityEngine;

//public class VirusIdle : BaseState
//{
//    public VirusIdle(Virus virus) : base(virus) { }

//    public override void OnStateEnter()
//    {
//        Debug.Log("Idle 상태입니다.");
//        _virus.animator.SetBool("isAttack", false);
//        _virus.animator.SetBool("isDef", false);
//        _virus.animator.SetBool("isSup", false);
//        _virus.GetRandState();
//        Debug.Log(_virus.virusObjectSO.name);
//    }

//    public override void OnStateUpdate()
//    {
        
//    }

//    public override void OnStateExit()
//    {
//        Debug.Log("Idle Exit 입니다.");
//    }
//}

//public class VirusAtk : BaseState
//{
//    public VirusAtk(Virus virus) : base(virus) { }

//    public override void OnStateEnter()
//    {
//        Debug.Log("ATK 상태입니다.");
//        //_virus.WaitTime();
//        GameManager.PlayerTurn = false;
//        _virus.animator.SetInteger("AttackIdx", _virus.spawnNum);
//        _virus.animator.SetBool("isAttack", true);
//        Debug.Log(_virus.virusData.AtkDmg);
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

//public class VirusDef : BaseState
//{
//    public VirusDef(Virus virus) : base(virus) { }

//    public override void OnStateEnter()
//    {
//        Debug.Log("DEF 상태입니다.");
//        //_virus.WaitTime();
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

//public class VirusSup : BaseState
//{
//    public VirusSup(Virus virus) : base(virus) { }

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
//    }
//}
//public class VirusDeath : BaseState
//{
//    public VirusDeath(Virus virus) : base(virus) { }

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
//    }
//}