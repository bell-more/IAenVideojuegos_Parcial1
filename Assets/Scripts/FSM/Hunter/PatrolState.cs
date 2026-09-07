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
        Debug.Log("Hunter entered patrol");
        currentWaypoint = 0;
    }

    public void Update()
    {
        if (hunter.CanAttack && hunter.targetsInRange.Count > 0)
        {
            fsm.ChangeState(HunterStates.Attack);
            return;
        }
        Patrol();
       // Debug.Log("Patrolling");
    }


    public void Exit()
    {
        Debug.Log("Hunter never exit patrol");
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
