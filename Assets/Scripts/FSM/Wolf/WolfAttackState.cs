using UnityEngine;

public class WolfAttackState : IState
{
    private Wolf wolf;
    private StateMachine fsm;
    private float timer = 0f;

    public WolfAttackState(Wolf wolf, StateMachine fsm)
    {
        this.wolf = wolf;
        this.fsm = fsm;
    }

    public void Enter()
    {
        InitializeAttack();
    }

    public void Update()
    {
        CheckTransitions();
    }

    private void InitializeAttack()
    {
        wolf.UpdateUI("Attacking", "");
        wolf.Agent.Stop();
        timer = wolf.AttackCooldown;

        if (wolf.PathDrawer != null) wolf.PathDrawer.ClearPath();
    }
    public void CheckTransitions()
    {
        if (wolf.HunterInSafeZone)
        {
            wolf.ForgetHunter();
            fsm.ChangeState(WolfStates.Patrol);
            return;
        }

        if (wolf.CurrentHunter == null || wolf.CurrentHunter.Life <= 0)
        {
            wolf.ForgetHunter();
            fsm.ChangeState(WolfStates.Patrol);
            return;
        }

        if (!wolf.IsSeeingHunter)
        {
            fsm.ChangeState(WolfStates.Investigate);
            return;
        }

        float distance = Vector3.Distance(wolf.transform.position, wolf.LastKnownHunterPosition);

        if (distance > wolf.CaptureRadius)
        {
            fsm.ChangeState(WolfStates.Pursuit);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= wolf.AttackCooldown)
        {
            timer = 0f;
            wolf.CurrentHunter.TakeDamage();
            wolf.ForgetHunter();

            fsm.ChangeState(WolfStates.Patrol);
        }
    }

    public void Exit() { }
}