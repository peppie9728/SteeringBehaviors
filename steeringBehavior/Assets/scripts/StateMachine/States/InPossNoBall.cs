using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************
//
//In possesion without the ball
//
//******************************
public class InPossNoBall : BaseState
{
    Vehicle vehicle;
    public InPossNoBall(SoccerStateMachine stateMachine, Vehicle _vehicle) : base("InPossNoBall", stateMachine)
    {
        vehicle = _vehicle;
    }


    public override void OnEnter()
    {
        base.OnEnter();
       
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (vehicle.ChaseTarget == null)
        {
            vehicle.ChaseTarget = vehicle.gameManager.ballCarrier;

        }
        vehicle.MoveRobot(vehicle.totalPower);
    }
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        vehicle.GetVelocity(true, Behaviors.Support);
    }
    public override void OnExit()
    {
        base.OnExit();

    }
}
