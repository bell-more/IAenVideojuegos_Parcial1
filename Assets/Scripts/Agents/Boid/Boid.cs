using UnityEngine;

public class Boid : Agent
{
    private SteeringAgent agent;
    private Flocking flocking;
    public Flocking Flocking => flocking;
    private BoidVision vision;
    private bool isAlive = true;

    [Header("Stats")]
    [SerializeField] private int life = 3;
    public int Life => life;
    private int initialLife;
    public int InitialLife { get => initialLife; set => initialLife = value; }
    public bool GetIsAlive => isAlive;
    private bool isCollected = false;
    [SerializeField] private float respawnRange = 10f;
    [SerializeField] private float respawnDelay = 3f;

    private float eatTimeRemaining;

    [Header("Trap")]
    [SerializeField] private float trapAttackCooldown = 3f;
    [SerializeField] private float trapAttackDistance = 2f;
    [SerializeField] private int trapDamageAmount = 1;
    private float trapAttackTimer;

    [Header("Wandering")]
    [Range(0f, 1f)][SerializeField] private float wanderingMinSpeed = 0.1f;
    [Range(0f, 1f)][SerializeField] private float wanderingPush = 0.5f;
    private InterestObject targetInterestObject;
    public InterestObject TargetInterestObject
    {
        set => targetInterestObject = value;
    }
    //visuals
    private BoidVisuals boidVisuals; 
    public BoidVisuals BoidVisuals  => boidVisuals;

    private void Awake()
    {
        GetComponents();
    }

    private void Start()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        agent.velocity = randomDirection * agent.MaxSpeed;
    }
    private void Update()
    {
        if (!isAlive) return;

        UpdateTimers();
        UpdateTarget();

        if (targetInterestObject != null)
        {
            HandleInterestObject();
            return;
        }

        if (vision.HunterAgent != null)
        {
            HandleHunter();
            return;
        }

        HandleFlocking();
    }
    public void GetComponents()
    {
        agent = GetComponent<SteeringAgent>();
        flocking = GetComponent<Flocking>();
        vision = GetComponent<BoidVision>();
        boidVisuals = GetComponent<BoidVisuals>();  
    }
    private void HandleInterestObject()
    {
        Vector3 targetPos = targetInterestObject.transform.position;
        targetPos.y = transform.position.y;

        float distance = (transform.position - targetPos).sqrMagnitude;

        if (distance <= trapAttackDistance)
        {
            if (eatTimeRemaining <= 0f)
            {
                eatTimeRemaining = (targetInterestObject.Life - 1) * trapAttackCooldown;
            }

            EatInterestObject();
        }
        else
        {
            MoveToInterestObject(targetPos);
        }
    }
    private void MoveToInterestObject(Vector3 targetPos)
    {
        Vector3 arriveForce = agent.Arrive(targetPos);

        if (vision.NearbyAgents.Count > 0)
        {
            arriveForce += flocking.GetFlocking(vision.NearbyAgents);
        }

        arriveForce.y = 0f;
        agent.ApplySteering(arriveForce);

        boidVisuals.UpdateUI("Going to food", "");
    }
    private void EatInterestObject()
    {
        agent.velocity = transform.forward * 0.001f;

        ApplyFlocking();

        boidVisuals.UpdateUI("Eating object", "Time remaining: " + eatTimeRemaining.ToString("F0"));

        if (trapAttackTimer <= 0f)
        {
            targetInterestObject.TakeDamage(trapDamageAmount);
            trapAttackTimer = trapAttackCooldown;
        }
    }

    private void HandleHunter()
    {
        Vector3 evadeForce = agent.Evade(vision.HunterAgent);

        if (vision.NearbyAgents.Count > 0)
        {
            evadeForce += flocking.GetFlocking(vision.NearbyAgents);
        }

        evadeForce.y = 0f;
        agent.ApplySteering(evadeForce);

        boidVisuals.UpdateUI("Evading", "");
    }

    private void HandleFlocking()
    {
        if (vision.NearbyAgents.Count > 0)
        {
            ApplyFlocking();

            boidVisuals.UpdateUI("Flocking", "");
        }
        else
        {
            HandleWandering();
        }
    }
    private void ApplyFlocking()
    {
        Vector3 flockingForce = flocking.GetFlocking(vision.NearbyAgents);
        flockingForce.y = 0f;

        agent.ApplySteering(flockingForce);
    }
    private void HandleWandering()
    {
        if (agent.velocity.magnitude < agent.MaxSpeed * wanderingMinSpeed)
        {
            Vector3 gentlePush = transform.forward * agent.MaxSpeed * wanderingPush;
            gentlePush.y = 0f;

            agent.ApplySteering(gentlePush);
        }

        boidVisuals.UpdateUI("Wandering", "");
       
    }
    private void UpdateTimers()
    {
        if (trapAttackTimer > 0f) trapAttackTimer -= Time.deltaTime;

        if (eatTimeRemaining > 0f)
            eatTimeRemaining -= Time.deltaTime;
    }

    private void UpdateTarget()
    {
        if (targetInterestObject != null && !targetInterestObject.IsAlive)
        {
            ClearTarget();
        }

        if (targetInterestObject == null && vision.InterestObject != null && !vision.InterestObject.Equals(null))
        {
            targetInterestObject = vision.InterestObject;
        }
    }

    private void ClearTarget()
    {
        targetInterestObject = null;
        eatTimeRemaining = 0f;
        agent.velocity = transform.forward * agent.MaxSpeed;

        boidVisuals.SetStatusUI();
    }
    public void TakeDamage(int damage)
    {
        life -= damage;

        boidVisuals.TakeDamageVisual();

        if (life <= 0 && isAlive)
        {
            life = 0;
            isAlive = false;
            isCollected = false;
            agent.Stop();

            boidVisuals.ToggleHealthBar(false);

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

        boidVisuals.SetStatusUI();
        boidVisuals.ToggleVisual(false);

        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        life = initialLife;
        isAlive = true;
        isCollected = false;

        targetInterestObject = null;
        trapAttackTimer = 0f;
        boidVisuals.ToggleHealthBar(true);

        GetComponent<BoidVision>().ClearVision();

        transform.position = new Vector3(Random.Range(-respawnRange, respawnRange), transform.position.y, Random.Range(-respawnRange, respawnRange));

        boidVisuals.ToggleVisual(true);

        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        agent.velocity = randomDirection * agent.MaxSpeed;
    }

}
