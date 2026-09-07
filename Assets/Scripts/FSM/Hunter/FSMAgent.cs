using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class FSMAgent : MonoBehaviour
{
    private StateMachine fsm;
    private Hunter hunter;

    private void Awake()
    {
        fsm = new StateMachine();
        hunter = GetComponent<Hunter>();

        PatrolState patrolState = new PatrolState(hunter, fsm);
        fsm.RegisterState(HunterStates.Patrol, patrolState);
        fsm.ChangeState(HunterStates.Patrol);

        AttackState attackState = new AttackState(hunter, fsm);
        fsm.RegisterState(HunterStates.Attack, attackState);
    }

    private void Update()
    {
        fsm.Update();
    }
}

