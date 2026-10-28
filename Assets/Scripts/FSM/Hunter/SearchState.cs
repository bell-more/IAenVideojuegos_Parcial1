using System.Collections.Generic;
using UnityEngine;

public class SearchState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private List<GraphNode> currentPath = new List<GraphNode>();
    private int currentPathIndex = 0;
    private bool isInCoto = false;
    private float spawnTimer = 0f;

    public SearchState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        hunter.StatusUI.SetStatus("Searching");

        if (!isInCoto)
        {
            CalculatePathToClosestCotoNode();
        }
        else if (currentPath == null || currentPath.Count == 0)
        {
            CalculateRandomPathInCoto();
        }
    }

    public void Update()
    {
        CheckTransitions();

        ProcessObjectSpawnBehaviour();

        NavigateAlongPath();
    }

    public void CheckTransitions()
    {
        if (hunter.Life <= 0)
        {
            fsm.ChangeState(HunterStates.Dead);
            return;
        }
        if (hunter.GetClosestVisibleWolf() != null)
        {
            fsm.ChangeState(HunterStates.Flee);
            return;
        }

        Boid deadTarget = hunter.GetClosestDeadTarget();
        if (deadTarget != null && hunter.HasLineOfSight(deadTarget.transform))
        {
            fsm.ChangeState(HunterStates.Gather);
            return;
        }

        Boid aliveTarget = hunter.GetClosestTarget();
        if (hunter.CanAttack && aliveTarget != null && hunter.HasLineOfSight(aliveTarget.transform))
        {
            fsm.ChangeState(HunterStates.Attack);
            return;
        }
    }

    public void ProcessObjectSpawnBehaviour()
    {
        if (isInCoto)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= hunter.SpawnTimer)
            {
                if (Bounds.Instance != null && Bounds.Instance.IsInsideBounds(hunter.transform.position))
                {
                    hunter.SpawnInterestObject();
                    spawnTimer -= hunter.SpawnTimer;
                }
            }
        }
    }
    private void NavigateAlongPath()
    {
        hunter.NavigatePath(currentPath, ref currentPathIndex, hunter.NodeReachedDistance, false, () => {
            if (!isInCoto)
            {
                isInCoto = true;
            }
            CalculateRandomPathInCoto();
        });

        if (currentPath == null || currentPath.Count == 0 && isInCoto)
        {
            CalculateRandomPathInCoto();
        }
    }

    private void CalculatePathToClosestCotoNode()
    {
        hunter.currentNode = GraphManager.Instance.GetClosestNode(hunter.transform.position);

        List<GraphNode> bestPath = null;
        int shortestPathLength = int.MaxValue;

        foreach (GraphNode cotoNode in GraphManager.Instance.cotoNodes)
        {
            List<GraphNode> path = AStarPathfinding.FindPath(hunter.currentNode, cotoNode);
            if (path != null && path.Count > 0 && path.Count < shortestPathLength)
            {
                shortestPathLength = path.Count;
                bestPath = path;
            }
        }

        if (bestPath != null)
        {
            bestPath = AStarPathfinding.EnsurePathContinuity(bestPath, currentPath, currentPathIndex);

            currentPath = bestPath;
            currentPathIndex = 0;
        }
    }

    private void CalculateRandomPathInCoto()
    {
        hunter.currentNode = GraphManager.Instance.GetClosestNode(hunter.transform.position);
        GraphNode randomNode = GraphManager.Instance.GetRandomCotoNode();

        if (hunter.currentNode != null && randomNode != null)
        {
            List<GraphNode> newPath = AStarPathfinding.FindPath(hunter.currentNode, randomNode);

            newPath = AStarPathfinding.EnsurePathContinuity(newPath, currentPath, currentPathIndex);

            currentPath = newPath;
            currentPathIndex = 0;
        }
    }

    public void Exit()
    {
        currentPath.Clear();
        hunter.StatusUI.SetStatus("");
        if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
    }
}