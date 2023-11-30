using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerState : StateMachine
{
    
    public Vehicle vehicle;
    [HideInInspector]
    public Idle idleState;
    [HideInInspector]
    public InPossWithBall inPossWithBallState;
    [HideInInspector]
    public InPossNoBall inPossNoBallState;
    [HideInInspector]
    public ResetPosition resetPositionState;
    [HideInInspector]
    public NotInPossOpen notInPossOpenState;
    [HideInInspector]
    public NotInPossPlayer notInPossPlayerState;
    private void Awake()
    {
        //initiate all the different states.
        idleState = new Idle(this,vehicle);
        resetPositionState = new ResetPosition(this,vehicle);
        inPossWithBallState = new InPossWithBall(this, vehicle);
        inPossNoBallState = new InPossNoBall(this, vehicle);
        notInPossOpenState = new NotInPossOpen(this, vehicle);
        notInPossPlayerState = new NotInPossPlayer(this, vehicle);
    }


    protected override BaseState GetInitialState()
    {
        return idleState;
    }
}
