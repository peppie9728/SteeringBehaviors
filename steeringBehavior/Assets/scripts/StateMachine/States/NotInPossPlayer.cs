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
    public NotInPossPlayer(SoccerState stateMachine, Vehicle _vehicle) : base("NotInPossPlayer", stateMachine)
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
        vehicle.MoveRobot();
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
