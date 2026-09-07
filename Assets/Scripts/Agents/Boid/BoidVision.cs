using System.Collections.Generic;
using UnityEngine;

public class BoidVision : MonoBehaviour
{
    private List<SteeringAgent> nearbyAgents = new List<SteeringAgent>();
    private SteeringAgent hunterAgent;

    public List<SteeringAgent> NearbyAgents => nearbyAgents;
    public SteeringAgent HunterAgent => hunterAgent;

    private void OnTriggerEnter(Collider other)
    {
        Boid boid = other.GetComponent<Boid>();

        if (boid != null && boid.gameObject != gameObject)
        {
            SteeringAgent neighbour = boid.GetComponent<SteeringAgent>();

            if (!nearbyAgents.Contains(neighbour))
            {
                nearbyAgents.Add(neighbour);
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Hunter"))
        {
            hunterAgent = other.GetComponent<SteeringAgent>();
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
    }
}
