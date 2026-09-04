using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    [SerializeField] private LayerMask boidLayer;

    public List<Transform> targetsInRange = new List<Transform>();

    private StateMachine stateMachine;

    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float waypointCheckDistance = 1f;

    private SteeringAgent agent;

    public List<Transform> Waypoints => waypoints;
    public float WaypointCheckDistance => waypointCheckDistance;
    public SteeringAgent Agent => agent;

    private void Awake()
    {
        agent = GetComponent<SteeringAgent>();

        stateMachine = new StateMachine();

        PatrolState patrolState = new PatrolState(this);

        stateMachine.RegisterState(
            HunterStates.Patrol,
            patrolState
        );

        stateMachine.ChangeState(HunterStates.Patrol);
        stateMachine.ChangeState(HunterStates.Patrol);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {
            if (!targetsInRange.Contains(other.transform))
            {
                targetsInRange.Add(other.transform);
            }

            // Después:
            // stateMachine.ChangeState(HunterStates.Pursuit);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {
            targetsInRange.Remove(other.transform);

            // Después:
            // stateMachine.ChangeState(HunterStates.Patrol);
        }
    }
}
