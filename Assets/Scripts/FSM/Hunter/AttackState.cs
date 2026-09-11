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

    }

    public void Update()
    {
        if (target == null || !hunter.targetsInRange.Contains(target.transform) || !target.GetIsAlive)
        {
            fsm.ChangeState(HunterStates.Patrol);
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
            UpdateUI("Range Attack");
            Attack(hunter.RangeAttack(target));
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
