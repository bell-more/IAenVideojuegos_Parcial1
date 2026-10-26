using System.Collections.Generic;
using UnityEngine;

public class WolfInvestigateState : IState
{
    private Wolf wolf;
    private StateMachine fsm;
    private List<GraphNode> path;
    private int pathIndex = 0;
    private float nodeReachedDistance = 1.5f;

    public WolfInvestigateState(Wolf wolf, StateMachine fsm)
    {
        this.wolf = wolf;
        this.fsm = fsm;
    }

    public void Enter()
    {
        wolf.UpdateUI("Investigating", "checking last known position");
        wolf.IsAlerted = false;
        wolf.IsInvestigating = true;

        wolf.currentNode = GraphManager.Instance.GetClosestNode(wolf.transform.position);
        GraphNode targetNode = GraphManager.Instance.GetClosestNode(wolf.LastKnownHunterPosition);

        if (wolf.currentNode != null && targetNode != null)
        {
            path = AStarPathfinding.FindPath(wolf.currentNode, targetNode);
            pathIndex = 0;
        }
    }

    public void Update()
    {
        if (wolf.IsSeeingHunter)
        {
            fsm.ChangeState(WolfStates.Pursuit);
            return;
        }

        if (path == null || pathIndex >= path.Count)
        {
            fsm.ChangeState(WolfStates.Patrol);
            return;
        }

        GraphNode targetNode = path[pathIndex];
        if (wolf.PathDrawer != null) wolf.PathDrawer.DrawNextNode(targetNode);

        Vector3 targetPos = targetNode.transform.position;
        targetPos.y = wolf.transform.position.y;

        float distance = Vector3.Distance(wolf.transform.position, targetPos);

        if (distance <= nodeReachedDistance)
        {
            wolf.currentNode = targetNode;
            pathIndex++;
        }
        else
        {
            Vector3 steering = wolf.Agent.Seek(targetPos);
            wolf.Agent.ApplySteering(steering);
        }
    }

    public void Exit()
    {
        wolf.IsInvestigating = false;
        path = null;
        if (wolf.PathDrawer != null) wolf.PathDrawer.ClearPath();
    }
}