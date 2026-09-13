using System.Collections.Generic;
using UnityEngine;

public class Flocking : MonoBehaviour
{

    [Header("Radius")]
    private float separationRadius = 2f;
    private float alignmentRadius = 5f;
     private float cohesionRadius = 12f;

    [Header("Weights")]
    [Range(0, 3)][SerializeField] private const float separationWeight = 1.5f;
    [Range(0, 3)][SerializeField] private const float alignmentWeight = 1f;
    [Range(0, 3)][SerializeField] private const float cohesionWeight = 1f;

    private SteeringAgent agent;

    private void Awake()
    {
        agent = GetComponent<SteeringAgent>();
    }

    public Vector3 GetFlocking(List<SteeringAgent> nearbyBoids)
    {
        Vector3 separation = CalculateSeparation(nearbyBoids) * separationWeight;

        Vector3 alignment = CalculateAlignment(nearbyBoids) * alignmentWeight;

        Vector3 cohesion = CalculateCohesion(nearbyBoids) * cohesionWeight;

        return separation + alignment + cohesion;
    }

    public Vector3 CalculateSeparation(List<SteeringAgent> nearbyBoids)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (SteeringAgent neighbour in nearbyBoids)
        {
            if (neighbour == agent) continue;

            Vector3 direction = transform.position - neighbour.transform.position;

            float distance = direction.magnitude;

            if (distance <= separationRadius && distance > 0.001f)
            {
                desired += direction.normalized / distance;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

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
            if (neighbour == agent) continue;

            float distance = Vector3.Distance(transform.position,neighbour.transform.position);

            if (distance <= alignmentRadius)
            {
                averageVelocity += neighbour.Velocity;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        averageVelocity /= count;

        Vector3 desired = averageVelocity.normalized * agent.MaxSpeed;

        return agent.CalculateSteering(desired);
    }

    private Vector3 CalculateCohesion(List<SteeringAgent> nearbyBoids)
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (SteeringAgent neighbour in nearbyBoids)
        {
            if (neighbour == agent) continue;

            float distance = Vector3.Distance(transform.position, neighbour.transform.position);

            if (distance <= cohesionRadius)
            {
                center += neighbour.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        center /= count;

        return agent.Seek(center);
    }

}
