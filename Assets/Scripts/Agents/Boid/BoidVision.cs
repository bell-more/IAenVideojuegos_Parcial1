using System.Collections.Generic;
using UnityEngine;

public class BoidVision : MonoBehaviour
{
    private List<SteeringAgent> nearbyAgents = new List<SteeringAgent>();
    public SteeringAgent hunterAgent;

    public List<SteeringAgent> NearbyAgents => nearbyAgents;
    public SteeringAgent HunterAgent => hunterAgent;

    private InterestObject interestObject;
    public InterestObject InterestObject => interestObject;

    [SerializeField] private LayerMask hunterLayer;

    private void OnTriggerEnter(Collider other)
    {
        Boid boidOnTrigger = other.GetComponent<Boid>();

        if (boidOnTrigger != null && boidOnTrigger.gameObject != gameObject)
        {
            if (!boidOnTrigger.GetIsAlive) return;

            SteeringAgent neighbour = boidOnTrigger.GetComponent<SteeringAgent>();

            if (!nearbyAgents.Contains(neighbour))
            {
                nearbyAgents.Add(neighbour);
            }
        }

        InterestObject objectOnTrigger = other.GetComponent<InterestObject>();

        if (objectOnTrigger != null && objectOnTrigger.IsAlive)
        {
            interestObject = objectOnTrigger;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & hunterLayer) != 0)
        {
            hunterAgent = other.GetComponentInParent<SteeringAgent>();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        SteeringAgent neighbour = other.GetComponent<SteeringAgent>();
        if (neighbour != null && nearbyAgents.Contains(neighbour))
        {
            nearbyAgents.Remove(neighbour);
        }

        if (((1 << other.gameObject.layer) & hunterLayer) != 0)
        {
            hunterAgent = null;
        }

        if (other.GetComponent<InterestObject>() == interestObject)
        {
            interestObject = null;
        }
    }
    public void ClearVision()
    {
        nearbyAgents.Clear();
        hunterAgent = null;
        interestObject = null;
    }

    public void RemoveNeighbour(SteeringAgent agentToRemove)
    {
        if (nearbyAgents.Contains(agentToRemove))
        {
            nearbyAgents.Remove(agentToRemove);
        }
    }
}
