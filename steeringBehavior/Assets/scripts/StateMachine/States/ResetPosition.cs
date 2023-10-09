using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : BaseState
{
    Vehicle vehicle;
    bool waiting;
    public ResetPosition(SoccerState stateMachine, Vehicle _vehicle) : base("ResetPosition", stateMachine) 
    {
        vehicle = _vehicle;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        vehicle.waiting = false;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        vehicle.MoveRobot();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
       Vector2 g = vehicle.GetVelocity(true,Behaviors.Seek);
        Debug.Log(vehicle.name + ": " + (g.magnitude));
        if (Vector2.Distance(vehicle.targetPos2,vehicle.m_vPos)< 1)
        {
            vehicle.waiting = true;
            if (vehicle.gameManager.checkReady())
            {
                stateMachine.ChangeState(((SoccerState)stateMachine).idleState);
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
