
using System.Collections.Generic;
using UnityEngine;

public class SearchState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private List<GraphNode> currentPath = new List<GraphNode>();
    private int currentPathIndex = 0;
    private bool isInCoto = false;
    private bool pathFailed = false;
    private float spawnTimer = 0f;

    public SearchState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        hunter.StatusUI.SetStatus("Searching");
        pathFailed = false;

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

    private void CheckTransitions()
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
        }
    }

    private void ProcessObjectSpawnBehaviour()
    {
        if (!isInCoto || hunter.SpawnTimer <= 0f) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= hunter.SpawnTimer && Bounds.Instance != null && Bounds.Instance.IsInsideBounds(hunter.transform.position))
        {
            hunter.SpawnInterestObject();
            spawnTimer -= hunter.SpawnTimer;
        }
    }

    private void NavigateAlongPath()
    {
        if (pathFailed || currentPath == null || currentPath.Count == 0) return;

        hunter.NavigatePath(currentPath, ref currentPathIndex, hunter.NodeReachedDistance, false, () =>
        {
            if (!isInCoto) isInCoto = true;
            CalculateRandomPathInCoto();
        });
    }

    private void CalculatePathToClosestCotoNode()
    {
        GraphManager graph = GraphManager.Instance;

        currentPath.Clear();
        currentPathIndex = 0;

        if (graph == null || graph.cotoNodes == null || graph.cotoNodes.Count == 0)
        {
            pathFailed = true;
            hunter.Agent.Stop();
            return;
        }

        hunter.currentNode = graph.GetClosestNode(hunter.transform.position);

        if (hunter.currentNode == null)
        {
            pathFailed = true;
            hunter.Agent.Stop();
            return;
        }

        List<GraphNode> bestPath = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GraphNode cotoNode in graph.cotoNodes)
        {
            if (cotoNode == null) continue;

            List<GraphNode> candidatePath = AStarPathfinding.FindPath(hunter.currentNode, cotoNode);

            if (candidatePath == null || candidatePath.Count == 0 || candidatePath[candidatePath.Count - 1] != cotoNode) continue;

            float pathDistance = Vector3.Distance(hunter.transform.position, candidatePath[0].transform.position);

            for (int i = 0; i < candidatePath.Count - 1; i++)
            {
                pathDistance += Vector3.Distance(candidatePath[i].transform.position, candidatePath[i + 1].transform.position);
            }

            if (pathDistance < shortestDistance)
            {
                shortestDistance = pathDistance;
                bestPath = candidatePath;
            }
        }

        if (bestPath == null)
        {
            pathFailed = true;
            hunter.Agent.Stop();
            return;
        }

        currentPath = bestPath;
        currentPathIndex = 0;
        pathFailed = false;
    }

    private void CalculateRandomPathInCoto()
    {
        GraphManager graph = GraphManager.Instance;

        currentPath = new List<GraphNode>();
        currentPathIndex = 0;

        if (graph == null || graph.cotoNodes == null || graph.cotoNodes.Count == 0)
        {
            pathFailed = true;
            hunter.Agent.Stop();
            return;
        }

        hunter.currentNode = graph.GetClosestNode(hunter.transform.position);

        if (hunter.currentNode == null)
        {
            pathFailed = true;
            hunter.Agent.Stop();
            return;
        }

        List<GraphNode> availableNodes = new List<GraphNode>();

        foreach (GraphNode node in graph.cotoNodes)
        {
            if (node != null && node != hunter.currentNode) availableNodes.Add(node);
        }

        GraphNode randomNode;

        if (availableNodes.Count > 0)
        {
            randomNode = availableNodes[Random.Range(0, availableNodes.Count)];
        }
        else
        {
            randomNode = graph.GetRandomCotoNode();
        }

        if (randomNode == null)
        {
            pathFailed = true;
            hunter.Agent.Stop();
            return;
        }

        List<GraphNode> newPath = AStarPathfinding.FindPath(hunter.currentNode, randomNode);

        if (newPath == null || newPath.Count == 0 || newPath[newPath.Count - 1] != randomNode)
        {
            pathFailed = true;
            hunter.Agent.Stop();
            return;
        }

        currentPath = newPath;
        currentPathIndex = 0;
        pathFailed = false;
    }

    public void Exit()
    {
        currentPath.Clear();
        currentPathIndex = 0;
        pathFailed = false;
        hunter.StatusUI.SetStatus("");
        if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
    }
}
