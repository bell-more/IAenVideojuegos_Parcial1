using System.Collections.Generic;

public class ExitState : IState
{
    private Hunter hunter;
    private StateMachine fsm;

    private List<GraphNode> currentPath = new List<GraphNode>();
    private int currentPathIndex = 0;
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

        hunter.NavigatePath(currentPath, ref currentPathIndex, hunter.NodeReachedDistance, true, () => {
            hunter.ResetGathers();

            if (hunter.initialZoneNode != null)
            {
                hunter.transform.position = hunter.InitialSpawnPosition;
                hunter.currentNode = hunter.initialZoneNode;
                hunter.Agent.Stop();
            }

            fsm.ChangeState(HunterStates.Search);
        });
    }

    public void Exit()
    {
        currentPath.Clear();
        hunter.StatusUI.SetStatus("");
        hunter.StatusUI.ShowAction("");
        if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
    }
}