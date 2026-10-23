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

        if (target != null)
        {
            gatherTimer = hunter.GatherTime;
            hunter.SetCurrentTarget(target);
        }
        else
        {
            fsm.ChangeState(HunterStates.Search);
        }
    }

    public void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy || target.GetIsAlive)
        {
            fsm.ChangeState(HunterStates.Search);
            return;
        }

        //behaviour
        float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

        if (distance <= hunter.MeleeAttackRadius)
        {
            hunter.Agent.Stop();
            hunter.StatusUI.ShowAction("Time remaining: " + gatherTimer.ToString("F0"));

            gatherTimer -= Time.deltaTime;

            if (gatherTimer <= 0f)
            {
                hunter.targetsInRange.Remove(target.transform);
                target.Collect(); 
                hunter.AddGather();

                if (hunter.CurrentGathers >= hunter.RequiredGathers)
                {
                    fsm.ChangeState(HunterStates.Exit);
                }
                else
                {
                    fsm.ChangeState(HunterStates.Search);
                }
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