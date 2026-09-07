using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class Boid : Agent
{
    private SteeringAgent agent;
    private Flocking flocking;
    private BoidVision vision;
    private bool isAlive = true;
    [SerializeField] private int life = 3;

    private void Awake()
    {
        agent = GetComponent<SteeringAgent>();
        flocking = GetComponent<Flocking>();
        vision = GetComponent<BoidVision>();

        Vector3 randomDirection = new Vector3(Random.Range(-1, 1), 0f, Random.Range(-1, 1));
        velocity += randomDirection.normalized * agent.MaxSpeed;
}

    private void Start()
    {
        Debug.Log("LIFE: " + life);
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        agent.velocity = randomDirection * agent.MaxSpeed;
    }

    private void Update()
    {
        if (!isAlive) return;
       
        if (vision.HunterAgent != null)
        {
            Vector3 evadeForce = agent.Evade(vision.HunterAgent);
            agent.ApplySteering(evadeForce);
            return;
        }

        if (vision.NearbyAgents.Count > 0)
        {
            Vector3 flockingForce = flocking.GetFlocking(vision.NearbyAgents);
            agent.ApplySteering(flockingForce);
        }
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
        Debug.Log("life: "+life);

        if (life <= 0)
        {

            life = 0;
            isAlive = false;
            agent.Stop();
           
            //die ----> stay still so the hunter can gather it ----after that-----> respawn in a random place after 3* seconds
        }
    }

}
