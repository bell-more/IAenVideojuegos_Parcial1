using UnityEngine;

public class PatrolState : IState
{
    private Hunter hunter;
    private int currentWaypoint;
    private StateMachine fsm;

    public PatrolState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        currentWaypoint = 0;
        hunter.StatusUI.SetStatus("Patrolling");
    }

    public void Update()
    {
        if (hunter.GetClosestDeadTarget() != null)
        {
            fsm.ChangeState(HunterStates.Gather);
            return;
        }

        if (hunter.CanAttack && hunter.GetClosestTarget() != null)
        {
            fsm.ChangeState(HunterStates.Attack);
            return;
        }

        Patrol();
    }


    public void Exit()
    {
        //Debug.Log("Hunter never exit patrol");
    }

    private void Patrol()
    {
        if (hunter.Waypoints.Count == 0) return;

        
        Transform waypoint = hunter.Waypoints[currentWaypoint];

        float distance = Vector3.Distance(hunter.transform.position, waypoint.position);

        if (distance <= hunter.WaypointCheckDistance)
        {
            currentWaypoint++;

            if (currentWaypoint >= hunter.Waypoints.Count) currentWaypoint = 0;

            return;
        }

        Vector3 targetPos = waypoint.position;
        targetPos.y = hunter.transform.position.y;

        Vector3 steering = hunter.Agent.Arrive(targetPos);

        hunter.Agent.ApplySteering(steering);
    }
}
