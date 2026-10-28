using System.Collections.Generic;
public class ExitState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private List<GraphNode> currentPath = new List<GraphNode>();
    private int currentPathIndex = 0;
    private bool pathFailed = false;

    public ExitState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        InitializeExit();
        CheckTransitions();
        CalculateExitPath();
    }

    public void Update()
    {
        CheckTransitions();

        if (pathFailed || currentPath == null || currentPath.Count == 0) return;

        hunter.NavigatePath(currentPath, ref currentPathIndex, hunter.NodeReachedDistance, true, CompleteExit);
    }

    private void InitializeExit()
    {
        UpdateUI();
        currentPath.Clear();
        currentPathIndex = 0;
        pathFailed = false;
    }
    private void UpdateUI()
    {
        hunter.StatusUI.SetStatus("Exiting level");
        hunter.StatusUI.ShowAction("");
    }

    private void CheckTransitions()
    {
        if (hunter.Life <= 0)
        {
            fsm.ChangeState(HunterStates.Dead);
            return;
        }

        if (hunter.GetClosestVisibleWolf() != null) fsm.ChangeState(HunterStates.Flee);
    }

    private void CalculateExitPath()
    {
        GraphManager graph = GraphManager.Instance;

        if (hunter.exitNode == null || graph == null)
        {
            FailPath();
            return;
        }

        hunter.currentNode = graph.GetClosestNode(hunter.transform.position);

        if (hunter.currentNode == null)
        {
            FailPath();
            return;
        }

        currentPath = AStarPathfinding.FindPath(hunter.currentNode, hunter.exitNode);

        if (currentPath == null || currentPath.Count == 0 || currentPath[currentPath.Count - 1] != hunter.exitNode) FailPath();
    }

    private void FailPath()
    {
        pathFailed = true;
        hunter.Agent.Stop();
    }

    private void CompleteExit()
    {
        if (pathFailed) return;

        pathFailed = true;
        hunter.Agent.Stop();
        hunter.ResetGathers();

        if (hunter.initialZoneNode != null)
        {
            hunter.transform.position = hunter.InitialSpawnPosition;
            hunter.currentNode = hunter.initialZoneNode;
            hunter.Agent.Stop();
        }

        fsm.ChangeState(HunterStates.Search);
    }

    public void Exit()
    {
        currentPath.Clear();
        currentPathIndex = 0;
        pathFailed = false;
        hunter.StatusUI.SetStatus("");
        hunter.StatusUI.ShowAction("");

        if (hunter.PathDrawer != null) hunter.PathDrawer.ClearPath();
    }
}