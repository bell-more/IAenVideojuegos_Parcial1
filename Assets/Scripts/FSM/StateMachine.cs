using UnityEngine;

public class StateMachine
{
    public State CurrentState { get; private set; }
    public void ChangeState(State NewState)
    {
        CurrentState.Exit();
        CurrentState = NewState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState.Update();
    }
}
