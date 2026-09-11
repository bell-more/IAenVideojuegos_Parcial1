using UnityEngine;

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
    [SerializeField] private HealthVisuals visuals;
    [SerializeField] private GameObject targetAura;

    [Header("UI")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private HunterStatusUI statusUI;

    [Header("Trap")]
    [SerializeField] private float trapAttackCooldown = 3f;
    [SerializeField] private float trapAttackDistance = 2f;
    private float totalEatTime;
    private float trapAttackTimer;
    private InterestObject targetInterestObject;
    public InterestObject TargetInterestObject
    {
        set => targetInterestObject = value;
    }

    //colours
    private Renderer[] allRenderers;
    private Color[] initialColours;

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

        allRenderers = GetComponentsInChildren<Renderer>(true);

        initialColours = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            initialColours[i] = allRenderers[i].material.color;
        }
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
    private float eatTimeRemaining;
    private void HandleInterestObject()
    {
        Vector3 targetPos = targetInterestObject.transform.position;
        targetPos.y = transform.position.y;

        float distance = Vector3.Distance(transform.position, targetPos);

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

        UpdateUI("Going to food", "");
    }
    private void EatInterestObject()
    {
        agent.velocity = transform.forward * 0.001f;

        ApplyFlocking();

        UpdateUI("Eating object", "Time remaining: " + eatTimeRemaining.ToString("F0"));

        if (trapAttackTimer <= 0f)
        {
            targetInterestObject.TakeDamage(1);
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

        UpdateUI("Evading", "");
    }

    private void HandleFlocking()
    {
        if (vision.NearbyAgents.Count > 0)
        {
            ApplyFlocking();

            UpdateUI("Flocking","");
        }
        else
        {
            HandleWandering();
        }
    }

    private void UpdateUI(string state, string action)
    {
        if (statusUI != null)
        {
            statusUI.SetStatus(state);
            statusUI.ShowAction(action);
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
        if (agent.velocity.magnitude < agent.MaxSpeed * 0.1f)
        {
            Vector3 gentlePush = transform.forward * agent.MaxSpeed * 0.5f;
            gentlePush.y = 0f;

            agent.ApplySteering(gentlePush);
        }

        UpdateUI("Wandering", "");
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

        if (statusUI != null)
        {
            statusUI.SetStatus("");
            statusUI.ShowAction("");
        }
    }
    public void TakeDamage(int damage)
    {
        life -= damage;
        if (healthBar != null) healthBar.UpdateHealth(life);

        if (visuals != null) visuals.SetColour(life);

        if (life <= 0 && isAlive)
        {
            life = 0;
            isAlive = false;
            isCollected = false;
            agent.Stop();

            if (healthBar != null) healthBar.SetActive(false);

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

        if (statusUI != null)
        {
            statusUI.SetStatus("");
            statusUI.ShowAction("");
        }

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

        targetInterestObject = null;
        trapAttackTimer = 0f;

        if (healthBar != null)
        {
            healthBar.SetActive(true);
            healthBar.UpdateHealth(initialLife);
        }

        GetComponent<BoidVision>().ClearVision();

        float range = 10f;
        transform.position = new Vector3(Random.Range(-range, range), transform.position.y, Random.Range(-range, range));

        childMaterial.GetComponent<Collider>().enabled = true;

        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].enabled = true;
            allRenderers[i].material.color = initialColours[i];
        }

        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        agent.velocity = randomDirection * agent.MaxSpeed;
    }

    public void ChangeColour(Color newColour)
    {
        if (childMaterial != null)
        {
            childMaterial.material.color = newColour;
        }
    }

    public void SetTargetArrow(bool isActive)
    {
        if (targetAura != null)
        {
            targetAura.SetActive(isActive);
        }
    }

}
