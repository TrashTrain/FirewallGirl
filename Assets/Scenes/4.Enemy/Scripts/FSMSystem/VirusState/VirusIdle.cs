// virus의 SO형식을 받아와서 행동하기.
using UnityEngine;

public class VirusIdle : BaseState
{
    public VirusIdle(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("Idle 상태입니다.");
    }

    public override void OnStateUpdate()
    {
        
    }

    public override void OnStateExit()
    {
        
    }
}

public class VirusAtk : BaseState
{
    public VirusAtk(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("ATK 상태입니다.");
    }

    public override void OnStateUpdate()
    {

    }

    public override void OnStateExit()
    {

    }
}

public class VirusDef : BaseState
{
    public VirusDef(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("DEF 상태입니다.");
    }

    public override void OnStateUpdate()
    {

    }

    public override void OnStateExit()
    {

    }
}

public class VirusSup : BaseState
{
    public VirusSup(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("SUP 상태입니다.");
    }

    public override void OnStateUpdate()
    {

    }

    public override void OnStateExit()
    {

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

    }
}