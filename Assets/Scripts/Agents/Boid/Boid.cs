using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class Boid : Agent
{
    private SteeringAgent agent;
    private Flocking flocking;
    private BoidVision vision;
    private bool isAlive = true;

    [Header("Stats")]
    [SerializeField] private int life = 3;
    private int initialLife;
    public bool GetIsAlive => isAlive;
    private bool isCollected = false;

    [Header("Visuals")]
    [SerializeField] private Renderer childMaterial;

    [Header("UI")]
    [SerializeField] private HealthBar healthBar;

    [Header("Trap")]
    [SerializeField] private float trapAttackCooldown = 3f;
    [SerializeField] private float trapAttackDistance = 2f;
    private float trapAttackTimer;
    private InterestObject targetInterestObject;
    public InterestObject TargetInterestObject
    {
        set => targetInterestObject = value;
    }

    //colours
    private Renderer[] allRenderers;
    private Color[] initialColors;

    private void Awake()
    {
        initialLife = life;

        if (healthBar != null)
        {
            healthBar.Setup(initialLife);
        }

        agent = GetComponent<SteeringAgent>();
        flocking = GetComponent<Flocking>();
        vision = GetComponent<BoidVision>();

        Vector3 randomDirection = new Vector3(Random.Range(-1, 1), 0f, Random.Range(-1, 1));
        velocity += randomDirection.normalized * agent.MaxSpeed;

        allRenderers = GetComponentsInChildren<Renderer>(true);

        initialColors = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            initialColors[i] = allRenderers[i].material.color;
        }
    }

    private void Start()
    {
      //  Debug.Log("LIFE: " + life);
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        agent.velocity = randomDirection * agent.MaxSpeed;
    }

    private void Update()
    {
        if (!isAlive) return;

        if (trapAttackTimer > 0f) trapAttackTimer -= Time.deltaTime;

        if (targetInterestObject != null && (targetInterestObject.Equals(null) || !targetInterestObject.GetIsAlive))
        {
            targetInterestObject = null;
            agent.velocity = transform.forward * agent.MaxSpeed;
        }

        if (targetInterestObject == null && vision.InterestObject != null && !vision.InterestObject.Equals(null))
        {
            targetInterestObject = vision.InterestObject;
        }

        if (targetInterestObject != null)
        {
            Vector3 targetPos = targetInterestObject.transform.position;
            targetPos.y = transform.position.y;

            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance <= trapAttackDistance)
            {
                agent.velocity = transform.forward * 0.001f;

                if (vision.NearbyAgents.Count > 0)
                {
                    Vector3 flockingForce = flocking.GetFlocking(vision.NearbyAgents);
                    flockingForce.y = 0f;
                    agent.ApplySteering(flockingForce);
                }

                if (trapAttackTimer <= 0f)
                {
                    targetInterestObject.TakeDamage(1);
                    trapAttackTimer = trapAttackCooldown;
                }
            }
            else
            {
                Vector3 arriveForce = agent.Arrive(targetPos);

                if (vision.NearbyAgents.Count > 0)
                {
                    Vector3 flockingForce = flocking.GetFlocking(vision.NearbyAgents);
                    arriveForce += flockingForce;
                }

                arriveForce.y = 0f;
                agent.ApplySteering(arriveForce);
            }
            return;
        }

        if (agent.velocity.magnitude < agent.MaxSpeed * 0.1f)
        {
            agent.velocity = transform.forward * agent.MaxSpeed;
        }

        if (vision.HunterAgent != null)
        {
            Vector3 evadeForce = agent.Evade(vision.HunterAgent);
            evadeForce.y = 0f;
            agent.ApplySteering(evadeForce);
            return;
        }

        if (vision.NearbyAgents.Count > 0)
        {
            Vector3 flockingForce = flocking.GetFlocking(vision.NearbyAgents);
            flockingForce.y = 0f;
            agent.ApplySteering(flockingForce);
        }
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
        if (healthBar != null) healthBar.UpdateHealth(life);

        //Debug.Log("life: " + life);

            if (life <= 0 && isAlive)
        {
            life = 0;
            isAlive = false;
            isCollected = false;
            agent.Stop();
            ChangeColor(Color.red);

            if (healthBar != null) healthBar.Toggle(false);

            foreach (SteeringAgent neighbor in vision.NearbyAgents)
            {
                BoidVision neighborVision = neighbor.GetComponent<BoidVision>();
                if (neighborVision != null)
                {
                    neighborVision.RemoveNeighbour(this.agent);
                }
            }
        }
    }

    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;
        CancelInvoke(nameof(Respawn));

        childMaterial.GetComponent<Collider>().enabled = false;

        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].enabled = false;
        }

        Invoke(nameof(Respawn), 3f);
    }

    private void Respawn()
    {
        life = initialLife;
        isAlive = true;
        isCollected = false;

        if (healthBar != null)
        {
            healthBar.Toggle(true);
            healthBar.UpdateHealth(initialLife);
        }

        GetComponent<BoidVision>().ClearVision();

        float range = 10f;
        transform.position = new Vector3(Random.Range(-range, range), transform.position.y, Random.Range(-range, range));

        childMaterial.GetComponent<Collider>().enabled = true;

        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].enabled = true;
            allRenderers[i].material.color = initialColors[i];
        }


        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        agent.velocity = randomDirection * agent.MaxSpeed;
    }

    public void ChangeColor(Color newColor)
    {
        if (childMaterial != null)
        {
            childMaterial.material.color = newColor;
        }
    }

}
