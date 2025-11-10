// virus의 SO형식을 받아와서 행동하기.
using UnityEngine;

public class VirusIdle : BaseState
{
    public VirusIdle(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("Idle 상태입니다.");
        _virus.animator.SetBool("isAttack", false);
        _virus.animator.SetBool("isDef", false);
        _virus.animator.SetBool("isSup", false);
        _virus.GetRandState();
        Debug.Log(_virus.virusObjectSO.name);
    }

    public override void OnStateUpdate()
    {
        
    }

    public override void OnStateExit()
    {
        Debug.Log("Idle Exit 입니다.");
    }
}

public class VirusAtk : BaseState
{
    public VirusAtk(Virus virus) : base(virus) { }
    //bool inFunc = false;
    //Vector2 oriPos;
    //float rimitTime = 0.3f;
    //float checkTime = 0f;
    //float speed = 100f;
    ////Animator animator;

    public override void OnStateEnter()
    {
        Debug.Log("ATK 상태입니다.");
        //_virus.WaitTime();
        GameManager.PlayerTurn = false;
        _virus.animator.SetInteger("AttackIdx", _virus.spawnNum);
        _virus.animator.SetBool("isAttack", true);
        Debug.Log(_virus.virusData.AtkDmg);
        PlayerManager.instance.TakeDamage(_virus.virusData.AtkDmg);
    }

    public override void OnStateUpdate()
    {
        

        //Vector2 enemyPos = new Vector3(-12, 0);

        //if (!inFunc)
        //{
        //    Debug.Log("ATK update 상태입니다.");
        //    oriPos = _virus.transform.position;
        //    inFunc = true;
        //}


        //if (checkTime <= rimitTime)
        //{
        //    checkTime = checkTime + Time.deltaTime;
        //    //Debug.Log(checkTime);
        //    //_virus.transform.position = Vector2.MoveTowards(_virus.transform.position, enemyPos, speed * Time.deltaTime);
        //}
        //else
        //{
        //    _virus.transform.position = Vector2.MoveTowards(_virus.transform.position, oriPos, speed * Time.deltaTime);
        //    if (_virus.transform.position.x == oriPos.x)
        //    {
        //        SequenceTurn.instance.SetPlusSequenceCheck();
        //        OnStateExit();
        //    }
        //}



    }

    public override void OnStateExit()
    {

        Debug.Log("passTurn");
        //GameManager.PlayerTurn = true;
        _virus.animator.SetBool("isAttack", false);

    }
}

public class VirusDef : BaseState
{
    public VirusDef(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("DEF 상태입니다.");
        //_virus.WaitTime();
        GameManager.PlayerTurn = false;
        
        _virus.animator.SetBool("isDef", true);
    }

    public override void OnStateUpdate()
    {
        
    }

    public override void OnStateExit()
    {
        Debug.Log("passTurn");
        _virus.animator.SetBool("isDef", false);
        //GameManager.PlayerTurn = true;
        //Troy.sequenceCheck = 1;
    }
}

public class VirusSup : BaseState
{
    public VirusSup(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("SUP 상태입니다.");
        _virus.virusData.AtkDmg += 3;
        Debug.Log("atkDmg : " + _virus.virusData.AtkDmg);
        //_virus.WaitTime();
        _virus.UpdateData();
        GameManager.PlayerTurn = false;
        _virus.animator.SetBool("isSup", true);
    }

    public override void OnStateUpdate()
    {
        
    }

    public override void OnStateExit()
    {
        Debug.Log("passTurn");
        _virus.animator.SetBool("isSup", false);
        //GameManager.PlayerTurn = true;
        //Troy.sequenceCheck = 1;
    }
}
public class VirusDeath : BaseState
{
    public VirusDeath(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("DEATH 상태입니다.");
    }

    public override void OnStateUpdate()
    {
        
    }

    public override void OnStateExit()
    {
        Debug.Log("passTurn");
        //GameManager.PlayerTurn = true;
        //Troy.sequenceCheck = 1;
    }
}