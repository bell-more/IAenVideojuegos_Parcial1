
using System.Collections.Generic;
using UnityEngine;

public class WolfInvestigateState : IState
{
    private Wolf wolf;
    private StateMachine fsm;
    private List<GraphNode> path;
    private int pathIndex = 0;
    private float searchTimer = 0f;
    private bool isSearchingLocally = false;
    private Vector3 currentInvestigateTarget;

    public WolfInvestigateState(Wolf wolf, StateMachine fsm)
    {
        this.wolf = wolf;
        this.fsm = fsm;
    }

    public void Enter()
    {
        InitializeInvestigation();
        RecalculatePath();
    }

    public void Update()
    {
        CheckTransitions();

        if (fsm == null || path == null || path.Count == 0)
        {
            if (!isSearchingLocally && !wolf.IsSeeingHunter && !wolf.HunterInSafeZone)
            {
                isSearchingLocally = true;
                searchTimer = wolf.InspectDuration;
                wolf.Agent.Stop();
            }

            if (isSearchingLocally) SearchLocally();
            return;
        }

        if (!isSearchingLocally)
        {
            wolf.NavigatePath(path, ref pathIndex, wolf.NodeReachedDistance, () =>
            {
                isSearchingLocally = true;
                searchTimer = wolf.InspectDuration;
                wolf.Agent.Stop();
                if (wolf.PathDrawer != null) wolf.PathDrawer.UpdateRemainingPath(path, pathIndex);
            });
        }
        else
        {
            SearchLocally();
        }
    }

    private void InitializeInvestigation()
    {
        wolf.WolfVisuals.UpdateUI("Investigating", "checking last known position");
        wolf.IsAlerted = false;
        wolf.IsInvestigating = true;
        currentInvestigateTarget = wolf.LastKnownHunterPosition;
        isSearchingLocally = false;
        searchTimer = wolf.InspectDuration;
    }

    private void CheckTransitions()
    {
        if (wolf.HunterInSafeZone)
        {
            wolf.ForgetHunter();
            fsm.ChangeState(WolfStates.Patrol);
            return;
        }

        if (wolf.IsSeeingHunter)
        {
            fsm.ChangeState(WolfStates.Pursuit);
            return;
        }

        if (Vector3.Distance(currentInvestigateTarget, wolf.LastKnownHunterPosition) > wolf.TargetMoveTolerance)
        {
            currentInvestigateTarget = wolf.LastKnownHunterPosition;
            RecalculatePath();
            return;
        }
    }

    private void RecalculatePath()
    {
        isSearchingLocally = false;
        searchTimer = wolf.InspectDuration;
        path = null;
        pathIndex = 0;

        if (GraphManager.Instance == null)
        {
            wolf.Agent.Stop();
            return;
        }

        GraphManager graph = GraphManager.Instance;
        wolf.currentNode = graph.GetClosestNode(wolf.transform.position, true);
        GraphNode targetNode = graph.GetClosestNode(currentInvestigateTarget, true);

        if (wolf.currentNode == null || targetNode == null)
        {
            wolf.Agent.Stop();
            return;
        }

        path = AStarPathfinding.FindPath(wolf.currentNode, targetNode, true);

        if (path == null || path.Count == 0)
        {
            path = null;
            wolf.Agent.Stop();
            return;
        }
    }

    private void SearchLocally()
    {
        searchTimer -= Time.deltaTime;

        wolf.WolfVisuals.ShowFeedback("Hunter lost!", wolf.WolfVisuals.HunterLostSprite);

        float rotationAngle = Mathf.Sin(Time.time * 3f) * 30f;
        wolf.transform.rotation = Quaternion.Euler(0f, wolf.transform.eulerAngles.y + (rotationAngle * Time.deltaTime), 0f);

        if (searchTimer <= 0f)
        {
            fsm.ChangeState(WolfStates.Patrol);
        }
    }

    public void Exit()
    {
        wolf.IsInvestigating = false;
        path = null;
        pathIndex = 0;
        isSearchingLocally = false;
        if (wolf.PathDrawer != null) wolf.PathDrawer.ClearPath();
    }
}
