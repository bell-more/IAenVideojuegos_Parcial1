using System.Collections.Generic;
using UnityEngine;

public class ExitState : IState
{
    private Hunter hunter;
    private StateMachine fsm;

    private List<GraphNode> currentPath = new List<GraphNode>();
    private int currentPathIndex = 0;
    private float nodeReachedDistance = 3f;

    public ExitState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        hunter.StatusUI.SetStatus("Exiting level");
        hunter.StatusUI.ShowAction("");

        if (hunter.currentNode != null && hunter.exitNode != null)
        {
            currentPath = AStarPathfinding.FindPath(hunter.currentNode, hunter.exitNode);
            currentPathIndex = 0;
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
            fsm.ChangeState(HunterStates.Flee);
            return;
        }

        if (currentPath == null || currentPath.Count == 0) return;

        if (currentPathIndex >= currentPath.Count)
        {
            hunter.ResetGathers();

            if (hunter.initialZoneNode != null)
            {
                hunter.transform.position = hunter.InitialSpawnPosition;
                hunter.currentNode = hunter.initialZoneNode;
                hunter.Agent.Stop();
            }

            fsm.ChangeState(HunterStates.Search);
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
        }
        else
        {
            Vector3 steering = hunter.Agent.Arrive(targetPos);
            hunter.Agent.ApplySteering(steering);
        }
    }

    public void Exit()
    {
        currentPath.Clear();
        hunter.StatusUI.SetStatus("");
        hunter.StatusUI.ShowAction("");
        if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
    }
}