using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InPossesion : BaseState
{
    Vehicle vehicle;
    public InPossesion(SoccerState stateMachine, Vehicle _vehicle) : base("InPossesion", stateMachine) 
    {
        vehicle = _vehicle;
    }


    public override void OnEnter()
    {
        base.OnEnter();

        if (vehicle.hasBall)
        {

        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        
        vehicle.MoveRobot();
    }
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        vehicle.GetVelocity(true,Behaviors.Seek);
    }
    public override void OnExit()
    {
        base.OnExit();

    }
}