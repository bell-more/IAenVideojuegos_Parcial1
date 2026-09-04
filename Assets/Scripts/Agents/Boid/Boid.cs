using UnityEngine;

public class Boid : MonoBehaviour
{
    private SteeringAgent agent;
    private Flocking flocking;
    private BoidVision vision;

    private void Awake()
    {
        agent = GetComponent<SteeringAgent>();
        flocking = GetComponent<Flocking>();
        vision = GetComponent<BoidVision>();
    }

    private void Start()
    {
        
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        agent.velocity = randomDirection * agent.MaxSpeed;
    }

    private void Update()
    {
        if (vision.HunterAgent != null)
        {
            Vector3 evadeForce = agent.Evade(vision.HunterAgent);
            agent.velocity += evadeForce * Time.deltaTime;
            return;
        }

        if (vision.NearbyAgents.Count > 0)
        {
            Vector3 flockingForce = flocking.GetFlocking(vision.NearbyAgents);
            agent.velocity += flockingForce * Time.deltaTime;
        }
    }
}
