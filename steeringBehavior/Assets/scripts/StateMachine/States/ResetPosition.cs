using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************
//
//this state is for resetting the game.
//
//******************************
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
        vehicle.aTarget = vehicle.resetPoint;
        vehicle.m_fMaxSpeed = 5;
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
