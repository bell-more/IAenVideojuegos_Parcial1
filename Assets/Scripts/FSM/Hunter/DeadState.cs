using UnityEngine;

public class DeadState : IState
{
    private Hunter hunter;
    private StateMachine fsm;
    private float timer;


    public DeadState(Hunter hunter, StateMachine fsm)
    {
        this.hunter = hunter;
        this.fsm = fsm;
    }

    public void Enter()
    {
        hunter.StatusUI.SetStatus("Dead");
        hunter.StatusUI.ShowAction("");

        hunter.Agent.Stop();
        hunter.ResetGathers();
        hunter.ToggleVisibility();
        timer = hunter.RespawnTime;
    }

    public void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (hunter.initialZoneNode != null)
            {
                hunter.transform.position = hunter.InitialSpawnPosition;
                hunter.currentNode = hunter.initialZoneNode;
            }

            hunter.Spawn();

            fsm.ChangeState(HunterStates.Search);
        }
    }

    public void Exit()
    {
        
        hunter.StatusUI.SetStatus("");
        hunter.StatusUI.ShowAction("");
    }
}