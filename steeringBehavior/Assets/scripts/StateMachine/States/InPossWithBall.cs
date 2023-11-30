using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************
//
//In possesion with the ball
//
//******************************
public class InPossWithBall : BaseState
{
    Vehicle vehicle;
    public InPossWithBall(SoccerStateMachine stateMachine, Vehicle _vehicle) : base("InPossWithBall", stateMachine)
    {
        vehicle = _vehicle;
    }
  

    public override void OnEnter()
    {
        base.OnEnter();
        vehicle.aTarget = vehicle.targetGoal;
        vehicle.m_fMaxSpeed = 4f;

    }

    public override void OnUpdate()
    {
        base.OnUpdate();



        vehicle.MoveRobot(vehicle.totalPower);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        vehicle.GetVelocity(true, Behaviors.Seek);
    }
    public override void OnExit()
    {
        base.OnExit();

    }
}
