
using System.Collections.Generic;
using UnityEngine;

public class FleeState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private List<GraphNode> currentPath = new List<GraphNode>();
    private int currentPathIndex = 0;
    private bool isSafe = false;
    private float waitTimer = 0f;

    public FleeState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        InitializeFlee();
    }

    public void Update()
    {
        CheckTransitions();
        if (!isSafe) NavigateAlongPath();
    }

    private void InitializeFlee()
    {
        UpdateUI();
        hunter.IsInSafeZone = false;
        isSafe = false;
        waitTimer = 0f;
        currentPath.Clear();
        currentPathIndex = 0;
        hunter.Agent.Stop();
        CalculatePathToClosestSafeZone();
    }

    private void CheckTransitions()
    {
        if (hunter.Life <= 0)
        {
            fsm.ChangeState(HunterStates.Dead);
            return;
        }

        if (isSafe)
        {
            hunter.StatusUI.SetStatus("Hiding");
            hunter.StatusUI.ShowAction($"Waiting: {Mathf.Max(0f, hunter.SafeZoneWaitTime - waitTimer):F1}s");
            waitTimer += Time.deltaTime;

            if (waitTimer >= hunter.SafeZoneWaitTime)
            {
                if (hunter.CurrentGathers >= hunter.RequiredGathers) fsm.ChangeState(HunterStates.Exit);
                else fsm.ChangeState(HunterStates.Search);
            }
        }
    }

    private void UpdateUI()
    {
        hunter.StatusUI.SetStatus("Fleeing");
        hunter.StatusUI.ShowAction("");
        hunter.Visuals.ToggleWolfDetection(true);
    }

    private void CalculatePathToClosestSafeZone()
    {
        GraphManager graph = GraphManager.Instance;
        if (graph == null)
        {
            hunter.Agent.Stop();
            return;
        }

        hunter.currentNode = graph.GetClosestNode(hunter.transform.position, true);
        if (hunter.currentNode == null || graph.safeZoneNodes == null || graph.safeZoneNodes.Count == 0)
        {
            hunter.Agent.Stop();
            return;
        }

        List<GraphNode> bestPath = null;
        float shortestDistance = Mathf.Infinity;
        GraphNode bestSafeNode = null;

        foreach (GraphNode safeNode in graph.safeZoneNodes)
        {
            if (safeNode == null) continue;

            List<GraphNode> path = AStarPathfinding.FindPath(hunter.currentNode, safeNode);

            if (path == null || path.Count == 0 || path[path.Count - 1] != safeNode) continue;

            float totalPathDistance = Vector3.Distance(hunter.transform.position, path[0].transform.position);

            for (int i = 0; i < path.Count - 1; i++)
            {
                totalPathDistance += Vector3.Distance(path[i].transform.position, path[i + 1].transform.position);
            }

            if (totalPathDistance < shortestDistance)
            {
                shortestDistance = totalPathDistance;
                bestPath = path;
                bestSafeNode = safeNode;
            }
        }

        if (bestPath == null || bestSafeNode == null)
        {
            currentPath.Clear();
            hunter.Agent.Stop();
            if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
            return;
        }

        currentPath = bestPath;
        currentPathIndex = 0;
        hunter.Visuals.EnableAreaToFleeIndicator(bestSafeNode.transform.position, true);
    }

    private void NavigateAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        hunter.NavigatePath(currentPath, ref currentPathIndex, hunter.NodeReachedDistance, true, () =>
        {
            if (isSafe) return;

            isSafe = true;
            hunter.IsInSafeZone = true;
            hunter.Agent.Stop();

            if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();

            SafeZoneNotifier.NotifyHunterEnteredSafeZone();
        });
    }

    public void Exit()
    {
        hunter.IsInSafeZone = false;
        hunter.Visuals.EnableAreaToFleeIndicator(Vector3.zero, false);
        hunter.Visuals.ToggleWolfDetection(false);
        currentPath.Clear();

        if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();

        hunter.Agent.Stop();
        isSafe = false;
        waitTimer = 0f;

        hunter.StatusUI.SetStatus("");
        hunter.StatusUI.ShowAction("");
    }
}
