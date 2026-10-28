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
        InitializeAttack();
    }

    public void Update()
    {
        if (CheckTransitions()) return;

        ProcessAttackBehaviour();
    }

    private void InitializeAttack()
    {
        hunter.StatusUI.SetStatus("Attacking");
        target = hunter.GetClosestTarget();
        hunter.SetCurrentTarget(target);
        isAimingRange = false;
    }

    private bool CheckTransitions()
    {
        if (hunter.Life <= 0)
        {
            fsm.ChangeState(HunterStates.Dead);
            return true;
        }

        if (hunter.GetClosestVisibleWolf() != null)
        {
            fsm.ChangeState(HunterStates.Flee);
            return true;
        }

        if (hunter.GetClosestDeadTarget() != null)
        {
            fsm.ChangeState(HunterStates.Gather);
            return true;
        }
        if (target == null || !target.GetIsAlive ||
            (hunter.transform.position - target.transform.position).sqrMagnitude > (hunter.VisionRadius * hunter.VisionRadius) ||
            !hunter.HasLineOfSight(target.transform))
        {
            fsm.ChangeState(HunterStates.Search);
            return true;
        }

        if (isAimingRange)
        {
            AimAndShoot();
            return true;
        }

        return false;
   
    }

    private void ProcessAttackBehaviour()
    {
        float distance = (hunter.transform.position - target.transform.position).sqrMagnitude;

        if (distance <= (hunter.MeleeAttackRadius * hunter.MeleeAttackRadius))
        {
            if (hunter.CanAttack)
            {
                hunter.Agent.Stop();
                hunter.StatusUI.ShowAction("Melee Attack");

                if (hunter.MeleeAttack(target))
                {
                    hunter.ResetAttackTimer();
                }
            }
            else
            {
                hunter.StatusUI.ShowAction("Pursuing...(Cooldown)");

                Vector3 lookPos = new Vector3(target.transform.position.x, hunter.transform.position.y, target.transform.position.z);
                hunter.transform.LookAt(lookPos);

                if (distance > (hunter.MeleeStandbyDistance * hunter.MeleeStandbyDistance))
                {
                    Vector3 steering = hunter.Agent.Pursuit(target);
                    hunter.Agent.ApplySteering(steering);
                }
                else
                {
                    hunter.Agent.Stop();
                }
            }
        }
        else if (distance <= (hunter.RangeAttackRadius * hunter.RangeAttackRadius))
        {
            if (hunter.CanAttack)
            {
                isAimingRange = true;
                hunter.Agent.Stop();
                hunter.StatusUI.ShowAction("Range Attack");
            }
            else
            {
                hunter.StatusUI.ShowAction("Pursuing...(Cooldown)");
                Vector3 steering = hunter.Agent.Pursuit(target);
                hunter.Agent.ApplySteering(steering);
            }
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
                if (hunter.RangeAttack(target))
                {
                    hunter.ResetAttackTimer();
                }
                isAimingRange = false;
            }
            else
            {
                hunter.transform.forward = direction;
            }
        }
    }

    public void Exit()
    {
        target = null;
        hunter.SetCurrentTarget(null);
        hunter.StatusUI.ShowAction("");
    }
}