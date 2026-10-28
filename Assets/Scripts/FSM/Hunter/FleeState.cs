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
        UpdateUI();

        hunter.IsInSafeZone = false;
        isSafe = false;
        waitTimer = 0f;

        CalculatePathToClosestSafeZone();
    }

    public void Update()
    {
        CheckTransitions();
        NavigateAlongPath();
    }

    public void CheckTransitions()
    {
        if (hunter.Life <= 0)
        {
            fsm.ChangeState(HunterStates.Dead);
            return;
        }

        if (isSafe)
        {
            hunter.StatusUI.SetStatus("Hiding");
            hunter.StatusUI.ShowAction($"Waiting: {(hunter.SafeZoneWaitTime - waitTimer):F1}s");

            waitTimer += Time.deltaTime;

            if (waitTimer >= hunter.SafeZoneWaitTime)
            {
                if (hunter.CurrentGathers >= hunter.RequiredGathers)
                {
                    fsm.ChangeState(HunterStates.Exit);
                }
                else
                {
                    fsm.ChangeState(HunterStates.Search);
                }
            }
            return;
        }
    }

    public void UpdateUI()
    {
        hunter.StatusUI.SetStatus("Fleeing");
        hunter.StatusUI.ShowAction("");
        hunter.Visuals.ToggleWolfDetection(true);
    }

    private void CalculatePathToClosestSafeZone()
    {
        hunter.currentNode = GraphManager.Instance.GetClosestNode(hunter.transform.position, true);

        List<GraphNode> bestPath = null;
        float shortestDistance = Mathf.Infinity;
        GraphNode bestSafeNode = null;

        foreach (GraphNode safeNode in GraphManager.Instance.safeZoneNodes)
        {
            List<GraphNode> path = AStarPathfinding.FindPath(hunter.currentNode, safeNode);

            if (path != null && path.Count > 0)
            {
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
        }

        if (bestPath != null && bestPath.Count > 0)
        {
            bestPath = AStarPathfinding.EnsurePathContinuity(bestPath, currentPath, currentPathIndex);

            currentPath = bestPath;
            currentPathIndex = 0;

            if (bestSafeNode != null)
            {
                hunter.Visuals.EnableAreaToFleeIndicator(bestSafeNode.transform.position, true);
            }
        }
    }

    private void NavigateAlongPath()
    {
        hunter.NavigatePath(currentPath, ref currentPathIndex, hunter.NodeReachedDistance, true, () => {
            if (isSafe) return;
            isSafe = true;
            hunter.IsInSafeZone = true;
            SafeZoneNotifier.NotifyHunterEnteredSafeZone();
            hunter.Agent.Stop();
            if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
        });
    }

    public void Exit()
    {
        hunter.IsInSafeZone = false;
        hunter.Visuals.EnableAreaToFleeIndicator(new Vector3(0f, 0f, 0f), false);
        hunter.Visuals.ToggleWolfDetection(false);
        currentPath.Clear();

        if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
        isSafe = false;
        waitTimer = 0f;

        hunter.StatusUI.SetStatus("");
        hunter.StatusUI.ShowAction("");
    }
}