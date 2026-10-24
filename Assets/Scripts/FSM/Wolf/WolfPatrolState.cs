using System.Collections.Generic;
using UnityEngine;

public class WolfPatrolState : IState
{
    private Wolf wolf;
    private StateMachine fsm;
    private int currentWaypointIndex = 0;
    private int patrolDirection = 1;
    private List<GraphNode> currentAStarPath;
    private int currentPathIndex = 0;
    private float nodeReachedDistance = 1.5f;

    public WolfPatrolState(Wolf wolf, StateMachine fsm)
    {
        this.wolf = wolf;
        this.fsm = fsm;
    }

    public void Enter()
    {
        currentWaypointIndex = 0;
        currentAStarPath = null;
    }

    public void Update()
    {
        if (wolf.IsSeeingHunter)
        {
            fsm.ChangeState(WolfStates.Pursuit);
            return;
        }

        if (wolf.IsAlerted)
        {
            fsm.ChangeState(WolfStates.Investigate);
            return;
        }

        Patrol();
    }

    public void Exit()
    {
        currentAStarPath = null;
    }

    private void Patrol()
    {
        if (wolf.PatrolWaypoints == null || wolf.PatrolWaypoints.Count == 0) return;

        Transform targetWaypoint = wolf.PatrolWaypoints[currentWaypointIndex];

        if (wolf.HasLineOfSightTo(targetWaypoint.position))
        {
            currentAStarPath = null;
            MoveToTarget(targetWaypoint.position);
        }
        else
        {
            if (currentAStarPath == null || currentAStarPath.Count == 0)
            {
                wolf.currentNode = GraphManager.Instance.GetClosestNode(wolf.transform.position);
                GraphNode waypointNode = GraphManager.Instance.GetClosestNode(targetWaypoint.position);

                currentAStarPath = AStarPathfinding.FindPath(wolf.currentNode, waypointNode);
                currentPathIndex = 0;
            }
            NavigateAStar();
        }
    }

    private void MoveToTarget(Vector3 destination)
    {
        destination.y = wolf.transform.position.y;
        float distance = Vector3.Distance(wolf.transform.position, destination);

        if (distance <= nodeReachedDistance)
        {
            CalculateNextWaypoint();
        }
        else
        {
            Vector3 steering = wolf.Agent.Arrive(destination);
            wolf.Agent.ApplySteering(steering);
        }
    }

    private void NavigateAStar()
    {
        if (currentAStarPath == null || currentPathIndex >= currentAStarPath.Count)
        {
            CalculateNextWaypoint();
            currentAStarPath = null;
            return;
        }

        GraphNode targetNode = currentAStarPath[currentPathIndex];
        Vector3 destination = targetNode.transform.position;
        destination.y = wolf.transform.position.y;

        float distance = Vector3.Distance(wolf.transform.position, destination);

        if (distance <= nodeReachedDistance)
        {
            wolf.currentNode = targetNode;
            currentPathIndex++;
        }
        else
        {
            Vector3 steering = wolf.Agent.Arrive(destination);
            wolf.Agent.ApplySteering(steering);
        }
    }

    private void CalculateNextWaypoint()
    {

        if (wolf.UsePingPongPatrol)
        {
            currentWaypointIndex += patrolDirection;
            if (currentWaypointIndex >= wolf.PatrolWaypoints.Count || currentWaypointIndex < 0)
            {
                patrolDirection *= -1;
                currentWaypointIndex += patrolDirection * 2;
            }
        }
        else
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= wolf.PatrolWaypoints.Count)
            {
                currentWaypointIndex = 0;
            }
        }
    }
}