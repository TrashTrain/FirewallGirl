// virus�� SO������ �޾ƿͼ� �ൿ�ϱ�.
using UnityEngine;

public class VirusIdle : BaseState
{
    public VirusIdle(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("Idle �����Դϴ�.");
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
        Debug.Log("ATK �����Դϴ�.");
        GameManager.PlayerTurn = false;
    }

    public override void OnStateUpdate()
    {
        Vector2 enemyPos = new Vector3(-12, 0);

        if (!inFunc)
        {
            Debug.Log("ATK update �����Դϴ�.");
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
        Debug.Log("ATK exit �����Դϴ�.");
        GameManager.PlayerTurn = true;
    }
}

public class VirusDef : BaseState
{
    public VirusDef(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("DEF �����Դϴ�.");
        GameManager.PlayerTurn = false;
    }

    public override void OnStateUpdate()
    {
        
    }

    public override void OnStateExit()
    {
        Debug.Log("DEF ���� �����Դϴ�.");
        GameManager.PlayerTurn = true;
    }
}

public class VirusSup : BaseState
{
    public VirusSup(Virus virus) : base(virus) { }

    public override void OnStateEnter()
    {
        Debug.Log("SUP �����Դϴ�.");
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
        Debug.Log("DEATH �����Դϴ�.");
    }

    public override void OnStateUpdate()
    {

    }

    public override void OnStateExit()
    {
        GameManager.PlayerTurn = true;
    }
}