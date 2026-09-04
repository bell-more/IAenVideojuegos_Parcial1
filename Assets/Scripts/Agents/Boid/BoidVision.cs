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
        SteeringAgent neighbour = other.GetComponent<SteeringAgent>();
        if (neighbour != null && neighbour.gameObject != gameObject)
        {
            if (!nearbyAgents.Contains(neighbour))
            {
                nearbyAgents.Add(neighbour);
            }
        }

        if (other.CompareTag("Hunter"))
        {
            hunterAgent = other.GetComponent<SteeringAgent>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SteeringAgent neighbor = other.GetComponent<SteeringAgent>();
        if (neighbor != null && nearbyAgents.Contains(neighbor))
        {
            nearbyAgents.Remove(neighbor);
        }

        if (other.CompareTag("Hunter"))
        {
            hunterAgent = null;
        }
    }
}
