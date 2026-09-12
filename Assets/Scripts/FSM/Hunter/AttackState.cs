using UnityEngine;

public class AttackState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private Boid target;

    public AttackState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        target = hunter.GetClosestTarget();
        hunter.SetCurrentTarget(target);

        hasShot = false;
    }
    private bool rangeAttack = false;
    private float timeSinceLastShoot = 20f;
    private float lastShootTime = -20f;
    private bool hasShot = false;
    public void Update()
    {
        timeSinceLastShoot = Time.time - lastShootTime;

        if (rangeAttack)
        {
            if (Vector3.Dot((target.transform.position - hunter.transform.position).normalized, hunter.transform.forward) > 1 - 0.05)
            {
                hunter.RangeAttack(target);
                rangeAttack = false;
                hasShot = true;
                lastShootTime = Time.time;
            }
            else
            {
                hunter.transform.LookAt(target.transform.position);
            }

            return;
        }

        if (timeSinceLastShoot < 1.5f)
        {
            return;
        }

        if (target == null || !hunter.targetsInRange.Contains(target.transform) || !target.GetIsAlive || hasShot)
        {
            fsm.ChangeState(HunterStates.Patrol);
            hunter.ResetAttackTimer();
            return;
        }

        float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

        if (distance <= hunter.MeleeAttackRadius)
        {
            UpdateUI("Melee Attack");
            Attack(hunter.MeleeAttack(target));
        }
        else if (distance <= hunter.RangeAttackRadius)
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

        if (hunter.CanAttack)
        {
            if (attackSuccessful)
            {
                hunter.ResetAttackTimer();
                fsm.ChangeState(HunterStates.Patrol);

                return;
            }
        }
    }
}
