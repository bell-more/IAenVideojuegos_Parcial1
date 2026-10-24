using UnityEngine;

public class AttackState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private Boid target;

    private bool isAimingRange;
    private const float minMagnitudeToNormalize = 0.001f;

    public AttackState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        hunter.StatusUI.SetStatus("Attacking");
        target = hunter.GetClosestTarget();
        hunter.SetCurrentTarget(target);
        isAimingRange = false;
    }

    public void Update()
    {
        if (hunter.Life <= 0)
        {
            fsm.ChangeState(HunterStates.Dead);
            return;
        }
        if (hunter.GetClosestVisibleWolf() != null)
        {
            fsm.ChangeState(HunterStates.Flee);
            return;
        }

        if (target == null || !target.GetIsAlive)
        {
            fsm.ChangeState(HunterStates.Search);
            return;
        }

        if (!hunter.targetsInRange.Contains(target.transform) || !hunter.HasLineOfSight(target.transform))
        {
            fsm.ChangeState(HunterStates.Search);
            return;
        }

        if (isAimingRange)
        {
            AimAndShoot();
            return;
        }

        float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

        if (distance <= hunter.MeleeAttackRadius)
        {
            hunter.StatusUI.ShowAction("Meele Attack");
            ExecuteAttack(hunter.MeleeAttack(target));
        }
        else if (distance <= hunter.RangeAttackRadius && hunter.CanAttack)
        {
            isAimingRange = true;
            hunter.Agent.Stop();
            hunter.StatusUI.ShowAction("Range Attack");
        }
        else
        {
            hunter.StatusUI.ShowAction("Pursuing...");
            Vector3 steering = hunter.Agent.Pursuit(target);
            hunter.Agent.ApplySteering(steering);
        }
    }

    private void AimAndShoot()
    {
        Vector3 direction = target.transform.position - hunter.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > minMagnitudeToNormalize)
        {
            direction.Normalize();

            if (Vector3.Dot(direction, hunter.transform.forward) > hunter.AimTolerance)
            {
                ExecuteAttack(hunter.RangeAttack(target));
                isAimingRange = false;
            }
            else
            {
                hunter.transform.forward = direction;
            }
        }
    }

    private void ExecuteAttack(bool attackSuccessful)
    {
        hunter.Agent.Stop();

        Vector3 lookPos = new Vector3(target.transform.position.x, hunter.transform.position.y, target.transform.position.z);
        hunter.transform.LookAt(lookPos);

        if (attackSuccessful)
        {
            hunter.ResetAttackTimer();
            fsm.ChangeState(HunterStates.Search);
        }
    }

    public void Exit()
    {
        target = null;
        hunter.SetCurrentTarget(null);
        hunter.StatusUI.ShowAction("");
    }
}