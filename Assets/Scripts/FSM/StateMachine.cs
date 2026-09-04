using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.VersionControl.Asset;

public class StateMachine
{
    private Dictionary<Enum, IState> states = new Dictionary<Enum, IState>();

    private IState currentState;

    public void RegisterState(Enum key, IState state)
    {
        states[key] = state;
    }

    public void ChangeState(Enum key)
    {
        if (!states.ContainsKey(key))
            return;

        currentState?.Exit();

        currentState = states[key];

        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }

}
