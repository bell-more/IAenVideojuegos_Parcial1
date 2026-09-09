using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.GraphicsBuffer;

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
        hunter.GetStatusUI.SetStatus("Gathering");
        target = hunter.GetClosestDeadTarget();
        gatherTimer = hunter.GetGatherTime; 
    }

    public void Update()
    {
        if (target == null || target.gameObject.activeInHierarchy == false)
        {
            fsm.ChangeState(HunterStates.Patrol);
            return;
        }

        float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

        if (distance <= hunter.GetMeleeAttackRadius) 
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
    }
}
