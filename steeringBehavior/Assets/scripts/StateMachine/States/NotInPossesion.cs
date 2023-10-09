using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotInPossesion : BaseState
{
    Vehicle vehicle;
    public NotInPossesion(SoccerState stateMachine, Vehicle _vehicle) : base("NotInPossesion", stateMachine)
    {
        vehicle = _vehicle;
    }


    public override void OnEnter()
    {
        base.OnEnter();
        vehicle.aTarget = vehicle.gameManager.ballCarrier.gameObject.transform;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        vehicle.MoveRobot();
    }
    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        vehicle.GetVelocity(false,Behaviors.Seek);
    }

    public override void OnExit()
    {
        base.OnExit();

    }
}