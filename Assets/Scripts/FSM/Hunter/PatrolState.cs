using UnityEngine;

public class PatrolState : IState
{
    private Hunter hunter;
    private int currentWaypoint;

    public PatrolState(Hunter hunter)
    {
        this.hunter = hunter;
    }

    public void Enter()
    {
        Debug.Log("Hunter entered patrol");
        currentWaypoint = 0;
    }

    public void Update()
    {

        if (hunter.Waypoints.Count == 0)
            return;

        Transform waypoint = hunter.Waypoints[currentWaypoint];

        float distance = Vector3.Distance(
            hunter.transform.position,
            waypoint.position
        );

        if (distance <= hunter.WaypointCheckDistance)
        {
            currentWaypoint++;

            if (currentWaypoint >= hunter.Waypoints.Count)
                currentWaypoint = 0;

            return;
        }

        Vector3 targetPosition = waypoint.position;
        targetPosition.y = hunter.transform.position.y;

        Vector3 steering = hunter.Agent.Seek(targetPosition);

        hunter.Agent.ApplySteering(steering);
    }


    public void Exit()
    {
        Debug.Log("Hunter never exit patrol");
    }
}
