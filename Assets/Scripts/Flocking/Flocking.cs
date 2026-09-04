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

    public Vector3 GetFlocking(List<SteeringAgent> neabyBoids)
    {
        Vector3 separation = CalculateSeparation(neabyBoids, separationRadius) * separationWeight;
        Vector3 alignment = CalculateAlignment(neabyBoids, alignmentRadius) * alignmentWeight;
        Vector3 cohesion = CalculateCohesion(neabyBoids, cohesionRadius) * cohesionWeight;

        return separation + alignment + cohesion;
    }

    public Vector3 CalculateSeparation(List<SteeringAgent> neabyBoids, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (var item in neabyBoids)
        {
            if (item == agent) continue;

            float distance = Vector3.Distance(transform.position, item.transform.position);

            if (distance <= radius && distance > 0.001f)
            {
                desired += item.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        desired /= count;
        desired = -desired;
        desired = desired.normalized * agent.MaxSpeed;

        return agent.CalculateSteering(desired);
    }

    public Vector3 CalculateAlignment(List<SteeringAgent> neighbors, float radius)
    {
        Vector3 desired = Vector3.zero;
        int count = 0;

        foreach (var item in neighbors)
        {
            if (item == agent) continue;

            float distance = Vector3.Distance(transform.position, item.transform.position);

            if (distance <= radius)
            {
                desired += item.Velocity;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        desired /= count;
        desired = desired.normalized * agent.MaxSpeed;

        return agent.CalculateSteering(desired);
    }

    public Vector3 CalculateCohesion(List<SteeringAgent> neighbors, float radius)
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var item in neighbors)
        {
            if (item == agent) continue;

            float distance = Vector3.Distance(transform.position, item.transform.position);

            if (distance <= radius)
            {
                center += item.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        center /= count;
        return agent.Seek(center);
    }

}
