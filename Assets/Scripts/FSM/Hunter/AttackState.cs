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
        //   Debug.Log("Hunter entered attack state");

        hunter.GetStatusUI.SetStatus("Attacking");
        target = hunter.GetClosestTarget();

    }

    public void Update()
    {
        if (target == null)
        {
            fsm.ChangeState(HunterStates.Patrol);
            Debug.Log("No target found or dead");
            return;
        }


        if (!hunter.targetsInRange.Contains(target.transform))
        {
            fsm.ChangeState(HunterStates.Patrol);
            return;
        }


        float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

        if (distance <= hunter.GetMeleeAttackRadius)
        {
            Attack(hunter.MeleeAttack(target));
        }
        else if (distance <= hunter.GetRangeAttackRadius)
        {
            Attack(hunter.RangeAttack(target));
        }
        else
        {
            Vector3 steering = hunter.Agent.Pursuit(target);
            hunter.Agent.ApplySteering(steering);
        }



        //  Debug.Log("Following: " + target.name);
    }

    public void Exit()
    {
        //  Debug.Log("Hunter exit attack state");
        target = null;
    }

    private void Attack(bool attackSuccessful)
    {
        hunter.Agent.Stop();
        Vector3 lookPos = new Vector3(target.transform.position.x, hunter.transform.position.y, target.transform.position.z);
        hunter.transform.LookAt(lookPos);

        if (hunter.CanAttack)
        {
            //Debug.Log("Meelee attack");

            if (attackSuccessful)
            {
                hunter.ResetAttackTimer();
                fsm.ChangeState(HunterStates.Patrol);

                return;
            }
        }
    }
}
