using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    protected Virus _virus;
    protected Vector3 originPos;
    protected Animator _animator;
    protected BaseState(Virus virus)
    {
        _virus = virus;
        originPos = virus.transform.position;
        _animator = virus.animator;
        Debug.Log(virus.transform.position);
    }

    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
}
