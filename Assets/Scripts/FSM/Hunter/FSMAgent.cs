using UnityEngine;

public class FSMAgent : MonoBehaviour
{
    private StateMachine fsm;
    private Hunter hunter;

    private void Awake()
    {
        fsm = new StateMachine();
        hunter = GetComponent<Hunter>();

        SearchState searchState = new SearchState(hunter, fsm);
        fsm.RegisterState(HunterStates.Search, searchState);

        AttackState attackState = new AttackState(hunter, fsm);
        fsm.RegisterState(HunterStates.Attack, attackState);

        GatherState gatherState = new GatherState(hunter, fsm);
        fsm.RegisterState(HunterStates.Gather, gatherState);

        ExitState exitState = new ExitState(hunter, fsm);
        fsm.RegisterState(HunterStates.Exit, exitState);

        FleeState fleeState = new FleeState(hunter, fsm);
        fsm.RegisterState(HunterStates.Flee, fleeState);

        DeadState deadState = new DeadState(hunter, fsm);
        fsm.RegisterState(HunterStates.Dead, deadState);

    }

    private void Start()
    {
        fsm.ChangeState(HunterStates.Search);
    }

    private void Update()
    {
        fsm.Update();
    }
}

