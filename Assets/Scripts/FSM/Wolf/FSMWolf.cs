using UnityEngine;

public class FSMWolf : MonoBehaviour
{
    private StateMachine fsm;
    private Wolf wolf;

    private void Awake()
    {
        fsm = new StateMachine();
        wolf = GetComponent<Wolf>();

        WolfPatrolState patrolState = new WolfPatrolState(wolf, fsm);
        fsm.RegisterState(WolfStates.Patrol, patrolState);

        WolfPursuitState pursuitState = new WolfPursuitState(wolf, fsm);
        fsm.RegisterState(WolfStates.Pursuit, pursuitState);

        WolfInvestigateState investigateState = new WolfInvestigateState(wolf, fsm);
        fsm.RegisterState(WolfStates.Investigate, investigateState);

        WolfAttackState attackState = new WolfAttackState(wolf, fsm);
        fsm.RegisterState(WolfStates.Attack, attackState);

    }

    private void Start()
    {
        fsm.ChangeState(WolfStates.Patrol);
    }

    private void Update()
    {
        fsm.Update();
    }

    public void ReturnToPatrol()
    {
        fsm.ChangeState(WolfStates.Patrol);
    }
}
