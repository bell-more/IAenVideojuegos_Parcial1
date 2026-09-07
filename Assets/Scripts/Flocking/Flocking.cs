using System.Collections.Generic;
using UnityEngine;

public class Flocking : MonoBehaviour
{

    [Header("Radius")]
    [SerializeField] private float separationRadius = 2f;
    [SerializeField] private float alignmentRadius = 5f;
    [SerializeField] private float cohesionRadius = 5f;

    [Header("Weights")]
    [Range(0, 3)][SerializeField] private float separationWeight = 1.5f;
    [Range(0, 3)][SerializeField] private float alignmentWeight = 1f;
    [Range(0, 3)][SerializeField] private float cohesionWeight = 1f;

    private SteeringAgent agent;

    private void Awake()
    {
        agent = GetComponent<SteeringAgent>();
    }

    public Vector3 GetFlocking(List<SteeringAgent> nearbyBoids)
    {
        Vector3 separation =
            CalculateSeparation(nearbyBoids) * separationWeight;

        Vector3 alignment =
            CalculateAlignment(nearbyBoids) * alignmentWeight;

        Vector3 cohesion =
            CalculateCohesion(nearbyBoids) * cohesionWeight;

        return separation + alignment + cohesion;
    }

    private Vector3 CalculateSeparation(List<SteeringAgent> nearbyBoids)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (SteeringAgent neighbour in nearbyBoids)
        {
            if (neighbour == agent)
                continue;

            Vector3 direction =
                transform.position - neighbour.transform.position;

            float distance = direction.magnitude;

            if (distance <= separationRadius && distance > 0.001f)
            {
                desired += direction.normalized / distance;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        desired /= count;

        desired = desired.normalized * agent.MaxSpeed;

        return agent.CalculateSteering(desired);
    }

    private Vector3 CalculateAlignment(List<SteeringAgent> nearbyBoids)
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;

        foreach (SteeringAgent neighbour in nearbyBoids)
        {
            if (neighbour == agent)
                continue;

            averageVelocity += neighbour.Velocity;
            count++;
        }

        if (count == 0)
            return Vector3.zero;

        averageVelocity /= count;

        Vector3 desired =
            averageVelocity.normalized * agent.MaxSpeed;

        return agent.CalculateSteering(desired);
    }

    private Vector3 CalculateCohesion(List<SteeringAgent> nearbyBoids)
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (SteeringAgent neighbour in nearbyBoids)
        {
            if (neighbour == agent)
                continue;

            center += neighbour.transform.position;
            count++;
        }

        if (count == 0)
            return Vector3.zero;

        center /= count;

        return agent.Seek(center);
    }

}
