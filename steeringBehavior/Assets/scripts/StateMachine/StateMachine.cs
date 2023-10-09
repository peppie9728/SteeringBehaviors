using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    BaseState currentState;

    // Start is called before the first frame update
    void Start()
    {
        currentState = GetInitialState();
        if (currentState != null) currentState.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != null) currentState.OnUpdate();
    }

    private void FixedUpdate()
    {
        if (currentState != null) currentState.OnFixedUpdate();
    }

    public void ChangeState(BaseState newState)
    {
        currentState.OnExit();
        currentState = newState;
        newState.OnEnter();
    }

    protected virtual BaseState GetInitialState()
    {
        return null;
    }
}
