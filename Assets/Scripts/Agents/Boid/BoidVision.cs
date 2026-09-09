using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BoidVision : MonoBehaviour
{
    private List<SteeringAgent> nearbyAgents = new List<SteeringAgent>();
    private SteeringAgent hunterAgent;

    public List<SteeringAgent> NearbyAgents => nearbyAgents;
    public SteeringAgent HunterAgent => hunterAgent;

    private InterestObject interestObject;
    public InterestObject InterestObject => interestObject;


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

        if (other.gameObject.layer == LayerMask.NameToLayer("Hunter"))
        {
            hunterAgent = other.GetComponent<SteeringAgent>();
        }

        InterestObject objectOnTrigger = other.GetComponent<InterestObject>();

        if (objectOnTrigger != null && objectOnTrigger.GetIsAlive)
        {
            //    Debug.Log("INTEREST OBJECT DETECTED");
            interestObject = objectOnTrigger;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SteeringAgent neighbour = other.GetComponent<SteeringAgent>();
        if (neighbour != null && nearbyAgents.Contains(neighbour))
        {
            nearbyAgents.Remove(neighbour);
        }

        if (other.CompareTag("Hunter"))
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
