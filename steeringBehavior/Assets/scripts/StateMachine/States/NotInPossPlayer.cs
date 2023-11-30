using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************
//
//Not in possesion and an enemy player has the ball
//
//******************************
public class NotInPossPlayer : BaseState
{
    Vehicle vehicle;
    public NotInPossPlayer(SoccerStateMachine stateMachine, Vehicle _vehicle) : base("NotInPossPlayer", stateMachine)
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
        vehicle.MoveRobot(vehicle.totalPower);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        vehicle.GetVelocity(true, Behaviors.Allign);
    }
    public override void OnExit()
    {
        base.OnExit();

    }
}
