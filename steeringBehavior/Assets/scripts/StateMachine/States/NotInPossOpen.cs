using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************
//
//Not in possesion ball is in the field.
//
//******************************
public class NotInPossOpen : BaseState
{
    Vehicle vehicle;
    public NotInPossOpen(SoccerStateMachine stateMachine, Vehicle _vehicle) : base("NotInPossOpen", stateMachine)
    {
        vehicle = _vehicle;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        vehicle.m_fMaxSpeed = 5;
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
