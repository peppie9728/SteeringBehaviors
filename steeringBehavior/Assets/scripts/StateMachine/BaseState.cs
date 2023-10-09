using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState
{
    public string name;
    public StateMachine stateMachine;

    public BaseState(string _name,StateMachine _stateMachine)
    {
        name = _name;
        stateMachine = _stateMachine;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnExit() { }

}
