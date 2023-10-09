using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerState : StateMachine
{

    public Vehicle vehicle;
    [HideInInspector]
    public Idle idleState;
    [HideInInspector]
    public InPossesion inPossesionState;
    [HideInInspector]
    public NotInPossesion notInPossesionState;
    [HideInInspector]
    public ResetPosition resetPositionState;

    private void Awake()
    {
        idleState = new Idle(this,vehicle);
        inPossesionState = new InPossesion(this,vehicle);
        notInPossesionState = new NotInPossesion(this,vehicle);
        resetPositionState = new ResetPosition(this,vehicle);
    }

    protected override BaseState GetInitialState()
    {
        return idleState;
    }
}
