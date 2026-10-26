using System.Collections.Generic;
using UnityEngine;

public class SearchState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private List<GraphNode> currentPath = new List<GraphNode>();
    private int currentPathIndex = 0;
    private float nodeReachedDistance = 1.5f;
    private bool isInCoto = false;
    private float spawnTimer = 3f;

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
        if (hunter.Life <= 0)
        {
            fsm.ChangeState(HunterStates.Dead);
            return;
        }
        if (hunter.GetClosestVisibleWolf() != null)
        {
            { }
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

        if (isInCoto)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                if (Bounds.Instance != null && Bounds.Instance.IsInsideBounds(hunter.transform.position))
                {
                    hunter.SpawnInterestObject();
                }

                spawnTimer = 3f;
            }
        }

        NavigateAlongPath();
    }

    private void NavigateAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            if (isInCoto) CalculateRandomPathInCoto();
            return;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            if (!isInCoto)
            {
                isInCoto = true;
            }
            CalculateRandomPathInCoto();
            return;
        }

        if (hunter.PathDrawer != null) hunter.PathDrawer.UpdateRemainingPath(currentPath, currentPathIndex);
     
        GraphNode targetNode = currentPath[currentPathIndex];
        Vector3 targetPos = targetNode.transform.position;
        targetPos.y = hunter.transform.position.y;

        float distance = Vector3.Distance(hunter.transform.position, targetPos);

        if (distance <= nodeReachedDistance)
        {
            hunter.currentNode = targetNode;
            currentPathIndex++;
        }
        else
        {
            Vector3 steering = hunter.Agent.Seek(targetPos);
            hunter.Agent.ApplySteering(steering);
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
            currentPath = AStarPathfinding.FindPath(hunter.currentNode, randomNode);
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