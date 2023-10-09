using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : BaseState
{
    Vehicle vehicle;
    public Idle(SoccerState stateMachine,Vehicle _vehicle) : base("Idle",stateMachine) 
    {
        vehicle = _vehicle;
    }
    public float countDown = 3;

    public override void OnEnter()
    {
        base.OnEnter();
        countDown = 3;
        vehicle.aTarget = vehicle.targetBall;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        countDown -= Time.deltaTime;
        if (countDown < 0)
        {
            stateMachine.ChangeState(((SoccerState)stateMachine).inPossesionState);
        }
    }
    public override void OnExit()
    {
        base.OnExit();

    }
}
