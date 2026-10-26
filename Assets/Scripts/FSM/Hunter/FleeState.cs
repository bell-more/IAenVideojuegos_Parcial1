using System.Collections.Generic;
using UnityEngine;

public class FleeState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private List<GraphNode> currentPath = new List<GraphNode>();
    private int currentPathIndex = 0;
    private float nodeReachedDistance = 3f;

    private bool isSafe = false;
    private float waitTimer = 0f;

    public FleeState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        hunter.StatusUI.SetStatus("Fleeing");
        hunter.StatusUI.ShowAction("");
        hunter.ToggleWolfDetection(true);

        isSafe = false;
        waitTimer = 0f;

        CalculatePathToClosestSafeZone();
    }

    public void Update()
    {
        if (hunter.Life <= 0)
        {
            fsm.ChangeState(HunterStates.Dead);
            return;
        }

        //safe zone
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

        NavigateAlongPath();
    }

    private void CalculatePathToClosestSafeZone()
    {
        hunter.currentNode = GraphManager.Instance.GetClosestNode(hunter.transform.position);

        List<GraphNode> bestPath = null;
        int shortestPathLength = int.MaxValue;
        GraphNode bestSafeNode = null;

        foreach (GraphNode safeNode in GraphManager.Instance.safeZoneNodes)
        {
            List<GraphNode> path = AStarPathfinding.FindPath(hunter.currentNode, safeNode);

            if (path != null && path.Count > 0 && path.Count < shortestPathLength)
            {
                shortestPathLength = path.Count;
                bestPath = path;
                bestSafeNode = safeNode;
            }
        }

        if (bestPath != null)
        {
            currentPath = bestPath;
            currentPathIndex = 0;

            if (bestSafeNode != null)
            {
                hunter.EnableAreaToFleeIndicator(bestSafeNode.transform.position, true);
            }
        }
    }

    private void NavigateAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        if (currentPathIndex >= currentPath.Count)
        {
            isSafe = true;
            hunter.Agent.Stop();
            if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
            return;
        }

        if (hunter.PathDrawer != null) hunter.PathDrawer.UpdateRemainingPath(currentPath, currentPathIndex);

        GraphNode targetNode = currentPath[currentPathIndex];
        Vector3 targetPos = targetNode.transform.position;
        targetPos.y = hunter.transform.position.y;

        float distance = Vector3.Distance(hunter.transform.position, targetPos);

        if (distance <= nodeReachedDistance)
        {
            currentPathIndex++;
            hunter.currentNode = targetNode; 
        }
        else
        {
           
            hunter.StatusUI.ShowAction("Going to safe zone");
            Vector3 steering = hunter.Agent.Arrive(targetPos);
            hunter.Agent.ApplySteering(steering);
        }
    }

    public void Exit()
    {
        hunter.EnableAreaToFleeIndicator(new Vector3(0f,0f,0f), false);
        hunter.ToggleWolfDetection(false);
        currentPath.Clear();

        if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
        isSafe = false;
        waitTimer = 0f;

        hunter.StatusUI.SetStatus("");
        hunter.StatusUI.ShowAction("");
    }
}