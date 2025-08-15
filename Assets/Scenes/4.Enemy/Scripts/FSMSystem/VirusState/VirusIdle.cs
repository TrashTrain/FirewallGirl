// virus의 SO형식을 받아와서 행동하기.
using UnityEngine;

public class VirusIdle : BaseState
{
    public VirusIdle(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("Idle 상태입니다.");
        Debug.Log(_virus.virusObjectSO.name);
    }

    public override void OnStateUpdate()
    {
        OnStateExit();
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
        GameManager.PlayerTurn = false;
        _virus.animator.SetBool("isAttack", true);
    }

    public override void OnStateUpdate()
    {
        OnStateExit();
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
        Debug.Log("sqcheck : " + SequenceTurn.instance.GetSequenceCheck());
        Debug.Log("ATK exit 상태입니다.");
        if (SequenceTurn.instance.GetSequenceCheck() > VirusSpawn.instance.GetVirusCount())
        {
            Debug.Log("passTurn");
            GameManager.PlayerTurn = true;
            //Troy.sequenceCheck = 1;
        }
        SequenceTurn.instance.SetVirusActionChange();
    }
}

public class VirusDef : BaseState
{
    public VirusDef(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("DEF 상태입니다.");
        GameManager.PlayerTurn = false;
    }

    public override void OnStateUpdate()
    {
        OnStateExit();
    }

    public override void OnStateExit()
    {
        SequenceTurn.instance.SetPlusSequenceCheck();
        Debug.Log("sqcheck : " + SequenceTurn.instance.GetSequenceCheck());
        Debug.Log("def exit 상태입니다.");
        if (SequenceTurn.instance.GetSequenceCheck() > VirusSpawn.instance.GetVirusCount())
        {
            Debug.Log("passTurn");
            GameManager.PlayerTurn = true;
            //Troy.sequenceCheck = 1;
        }
        SequenceTurn.instance.SetVirusActionChange();
    }
}

public class VirusSup : BaseState
{
    public VirusSup(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("SUP 상태입니다.");
        _virus.virusData.AtkDmg += 3;
        Debug.Log("atkDmg : "+ _virus.virusData.AtkDmg);
        _virus.UpdateData();
        GameManager.PlayerTurn = false;
    }

    public override void OnStateUpdate()
    {
        OnStateExit();
    }

    public override void OnStateExit()
    {
        SequenceTurn.instance.SetPlusSequenceCheck();
        Debug.Log("sqcheck : " +  SequenceTurn.instance.GetSequenceCheck());
        Debug.Log("sup exit 상태입니다.");
        if (SequenceTurn.instance.GetSequenceCheck() > VirusSpawn.instance.GetVirusCount())
        {
            Debug.Log("passTurn");
            GameManager.PlayerTurn = true;
            //Troy.sequenceCheck = 1;
        }
        SequenceTurn.instance.SetVirusActionChange();
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
        OnStateExit();
    }

    public override void OnStateExit()
    {
        SequenceTurn.instance.SetPlusSequenceCheck();
        Debug.Log("death exit 상태입니다.");
        if (SequenceTurn.instance.GetSequenceCheck() > VirusSpawn.instance.GetVirusCount())
        {
            Debug.Log("passTurn");
            GameManager.PlayerTurn = true;
            //Troy.sequenceCheck = 1;
        }
        SequenceTurn.instance.SetVirusActionChange();
    }
}