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
        
    }

    public override void OnStateExit()
    {
        
    }
}

public class VirusAtk : BaseState
{
    public VirusAtk(Virus virus) : base(virus) { }
    bool inFunc = false;
    Vector2 oriPos;
    float rimitTime = 0.3f;
    float checkTime = 0f;
    float speed = 100f;
    public override void OnStateEnter()
    {
        Debug.Log("ATK 상태입니다.");
        GameManager.PlayerTurn = false;
    }

    public override void OnStateUpdate()
    {
        Vector2 enemyPos = new Vector3(-12, 0);

        if (!inFunc)
        {
            Debug.Log("ATK update 상태입니다.");
            oriPos = _virus.transform.position;
            inFunc = true;
        }
        
        
        if(checkTime <= rimitTime)
        {
            checkTime = checkTime + Time.deltaTime;
            //Debug.Log(checkTime);
            _virus.transform.position = Vector2.MoveTowards(_virus.transform.position, enemyPos, speed * Time.deltaTime);
        }
        else
        {
            _virus.transform.position = Vector2.MoveTowards(_virus.transform.position, oriPos, speed * Time.deltaTime);
            if (_virus.transform.position.x == oriPos.x)
            {
                OnStateExit();
            }
        }
        
        
        
    }

    public override void OnStateExit()
    {
        Debug.Log("ATK exit 상태입니다.");
        GameManager.PlayerTurn = true;
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
        
    }

    public override void OnStateExit()
    {
        Debug.Log("DEF 상태 종료입니다.");
        GameManager.PlayerTurn = true;
    }
}

public class VirusSup : BaseState
{
    public VirusSup(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("SUP 상태입니다.");
        _virus.atkDmg += 3;
        Debug.Log("atkDmg : "+_virus.atkDmg);
        _virus.UpdateData();
        GameManager.PlayerTurn = false;
    }

    public override void OnStateUpdate()
    {

    }

    public override void OnStateExit()
    {
        GameManager.PlayerTurn = true;
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
        GameManager.PlayerTurn = true;
    }
}