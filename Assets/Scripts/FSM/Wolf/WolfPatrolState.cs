
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
    private bool pathFailed = false;

    public WolfPatrolState(Wolf wolf, StateMachine fsm)
    {
        this.wolf = wolf;
        this.fsm = fsm;
    }

    public void Enter()
    {
        InitializePatrol();
    }

    public void Update()
    {
        CheckTransitions();
        Patrol();
    }

    public void InitializePatrol()
    {
        wolf.ForgetHunter();
        currentWaypointIndex = 0;
        patrolDirection = 1;
        currentAStarPath = null;
        currentPathIndex = 0;
        pathFailed = false;
        wolf.WolfVisuals.UpdateUI("Patrolling", "");
    }

    public void CheckTransitions()
    {
        if (wolf.IsSeeingHunter && wolf.CurrentHunter != null && wolf.CurrentHunter.Life > 0)
        {
            fsm.ChangeState(WolfStates.Pursuit);
            return;
        }

        if (wolf.IsAlerted)
        {
            fsm.ChangeState(WolfStates.Investigate);
            return;
        }
    }

    private void Patrol()
    {
        if (wolf.PatrolWaypoints == null || wolf.PatrolWaypoints.Count == 0 || GraphManager.Instance == null) return;

        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, wolf.PatrolWaypoints.Count - 1);
        Transform targetWaypoint = wolf.PatrolWaypoints[currentWaypointIndex];

        if (targetWaypoint == null) return;

        if (wolf.HasLineOfSightTo(targetWaypoint.position))
        {
            if (currentAStarPath != null)
            {
                currentAStarPath = null;
                currentPathIndex = 0;
                pathFailed = false;
                if (wolf.PathDrawer != null) wolf.PathDrawer.ClearPath();
            }

            MoveToTarget(targetWaypoint.position);
            return;
        }

        if (currentAStarPath == null && !pathFailed)
        {
            GraphManager graph = GraphManager.Instance;
            wolf.currentNode = graph.GetClosestNode(wolf.transform.position, true);
            GraphNode waypointNode = graph.GetClosestNode(targetWaypoint.position, true);

            if (wolf.currentNode == null || waypointNode == null)
            {
                pathFailed = true;
                wolf.Agent.Stop();
                return;
            }

            currentAStarPath = AStarPathfinding.FindPath(wolf.currentNode, waypointNode, true);
            currentPathIndex = 0;

            if (currentAStarPath == null || currentAStarPath.Count == 0)
            {
                currentAStarPath = null;
                pathFailed = true;
                wolf.Agent.Stop();
                if (wolf.PathDrawer != null) wolf.PathDrawer.ClearPath();
                return;
            }
        }

        if (currentAStarPath != null) NavigateAStar();
    }

    private void MoveToTarget(Vector3 destination)
    {
        destination.y = wolf.transform.position.y;
        float distance = (wolf.transform.position - destination).sqrMagnitude;

        if (distance <= (wolf.NodeReachedDistance * wolf.NodeReachedDistance))
        {
            wolf.Agent.Stop();
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
        wolf.NavigatePath(currentAStarPath, ref currentPathIndex, wolf.NodeReachedDistance, () =>
        {
            wolf.Agent.Stop();
            CalculateNextWaypoint();
            currentAStarPath = null;
            currentPathIndex = 0;
            pathFailed = false;
            if (wolf.PathDrawer != null) wolf.PathDrawer.ClearPath();
        });
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
            if (currentWaypointIndex >= wolf.PatrolWaypoints.Count) currentWaypointIndex = 0;
        }

        currentAStarPath = null;
        currentPathIndex = 0;
        pathFailed = false;
    }

    public void Exit()
    {
        currentAStarPath = null;
        currentPathIndex = 0;
        pathFailed = false;
        if (wolf.PathDrawer != null) wolf.PathDrawer.ClearPath();
    }
}
