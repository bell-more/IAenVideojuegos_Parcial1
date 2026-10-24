using UnityEngine;

public class WolfPursuitState : IState
{
    private Wolf wolf;
    private StateMachine fsm;

    public WolfPursuitState(Wolf wolf, StateMachine fsm)
    {
        this.wolf = wolf;
        this.fsm = fsm;
    }

    public void Enter()
    {
        wolf.IsAlerted = false;
    }

    public void Update()
    {
        if (!wolf.IsSeeingHunter)
        {
            fsm.ChangeState(WolfStates.Investigate);
            return;
        }

        float distance = Vector3.Distance(wolf.transform.position, wolf.LastKnownHunterPosition);
        if (distance <= 2.5f)
        {
            fsm.ChangeState(WolfStates.Attack);
            return;
        }

        Vector3 targetPos = new Vector3(wolf.LastKnownHunterPosition.x, wolf.transform.position.y, wolf.LastKnownHunterPosition.z);
        Vector3 steering = wolf.Agent.Seek(targetPos);
        wolf.Agent.ApplySteering(steering);
    }

    public void Exit() { }
}