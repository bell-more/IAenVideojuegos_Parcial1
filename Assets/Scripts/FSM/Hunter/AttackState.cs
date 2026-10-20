using UnityEngine;

public class AttackState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private Boid target;
    private bool rangeAttack;

    public AttackState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        target = hunter.GetClosestTarget();
        hunter.SetCurrentTarget(target);
        rangeAttack = false;
    }

    public void Update()
    {
        if (target == null || !target.GetIsAlive || !hunter.targetsInRange.Contains(target.transform))
        {
            fsm.ChangeState(HunterStates.Patrol);
            hunter.ResetAttackTimer();
            return;
        }

        if (rangeAttack)
        {
            Vector3 direction = target.transform.position - hunter.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                direction.Normalize();

                if (Vector3.Dot(direction, hunter.transform.forward) > 1f - 0.05f)
                {
                    hunter.RangeAttack(target);
                    hunter.ResetAttackTimer();
                    rangeAttack = false;
                }
                else
                {
                    hunter.transform.forward = direction;
                }
            }

            return;
        }

        float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

        if (distance <= hunter.MeleeAttackRadius)
        {
            UpdateUI("Melee Attack");
            Attack(hunter.MeleeAttack(target));
        }
        else if (distance <= hunter.RangeAttackRadius && hunter.CanAttack)
        {
            rangeAttack = true;
            hunter.Agent.Stop();
            UpdateUI("Range Attack");
        }
        else
        {
            UpdateUI();
            Vector3 steering = hunter.Agent.Pursuit(target);
            hunter.Agent.ApplySteering(steering);
        }
    }

    public void Exit()
    {
        target = null;
        hunter.SetCurrentTarget(null);
    }

    private void UpdateUI(string action = "")
    {
        hunter.StatusUI.SetStatus("Pursuing Boid");
        hunter.StatusUI.ShowAction(action);
    }

    private void Attack(bool attackSuccessful)
    {
        hunter.Agent.Stop();

        Vector3 lookPos = new Vector3(target.transform.position.x, hunter.transform.position.y, target.transform.position.z);
        hunter.transform.LookAt(lookPos);

        if (hunter.CanAttack && attackSuccessful)
        {
            hunter.ResetAttackTimer();
        }
    }
}