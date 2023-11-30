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
    public InPossWithBall(SoccerState stateMachine, Vehicle _vehicle) : base("InPossWithBall", stateMachine)
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

        vehicle.GetVelocity(true, Behaviors.Seek);
        vehicle.MoveRobot();
    }
    public override void OnExit()
    {
        base.OnExit();

    }
}
