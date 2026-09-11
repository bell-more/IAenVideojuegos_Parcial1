using UnityEngine;

public class GatherState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private Boid target;
    private float gatherTimer;

    public GatherState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        hunter.StatusUI.SetStatus("Gathering");
        target = hunter.GetClosestDeadTarget();
        gatherTimer = hunter.GatherTime;
        hunter.SetCurrentTarget(target);

    }

    public void Update()
    {
        if (target == null || target.gameObject.activeInHierarchy == false || target.GetIsAlive)
        {
            fsm.ChangeState(HunterStates.Patrol);
            return;
        }

        hunter.StatusUI.ShowAction("Time remaining: " + gatherTimer.ToString("F0"));

        float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

        if (distance <= hunter.MeleeAttackRadius) 
        {
            hunter.Agent.Stop(); 
            gatherTimer -= Time.deltaTime;

            if (gatherTimer <= 0f)
            {
                hunter.targetsInRange.Remove(target.transform);
                target.Collect();

                fsm.ChangeState(HunterStates.Patrol);
            }
        }
        else 
        {
            Vector3 targetPos = target.transform.position;
            targetPos.y = hunter.transform.position.y;

            Vector3 steering = hunter.Agent.Arrive(targetPos);
            hunter.Agent.ApplySteering(steering);
        }
    }

    public void Exit()
    {
        target = null;
        hunter.StatusUI.ShowAction("");
        hunter.SetCurrentTarget(null);
    }
}
