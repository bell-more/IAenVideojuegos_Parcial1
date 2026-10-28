using System.Collections.Generic;
using UnityEngine;
public class Hunter : Agent
{
    public List<Transform> targetsInRange = new List<Transform>();
    private Boid currentBoid;
    [SerializeField] private int life = 1;
    private int currentLife;
    private Vector3 initialSpawnPosition;
    public Vector3 InitialSpawnPosition => initialSpawnPosition;
    [Header("FSM")]
    [SerializeField] private float nodeReachedDistance = 0.5f;
    public float NodeReachedDistance => nodeReachedDistance;
    [SerializeField] private float visionRadius = 15f; 
    public float VisionRadius => visionRadius;
    [SerializeField] private float meleeStandbyDistance = 1f;
    public float MeleeStandbyDistance => meleeStandbyDistance;
    [SerializeField] private LayerMask wolfLayer;
    [SerializeField] private LayerMask boidLayer;

    [Header("Attack")]
    [SerializeField] private float tba = 3f;
    [SerializeField] private float rangeAttackRadius = 6f;
    [SerializeField] private float meleeAttackRadius = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float aimTolerance = 0.95f;

    public float AimTolerance => aimTolerance;
    public float RangeAttackRadius => rangeAttackRadius;
    public float MeleeAttackRadius => meleeAttackRadius;
    private float tbaTimer;
    public bool CanAttack { get { return tbaTimer <= 0f; } }
    public int Life => currentLife;

    [Header("Gather")]
    [SerializeField] private float gatherTime = 2f;
    [SerializeField] private int requiredGathers = 3;
    public float GatherTime => gatherTime;
     private int currentGathers = 0;
    public int RequiredGathers => requiredGathers;
    public int CurrentGathers => currentGathers;

    private SteeringAgent agent;
    public SteeringAgent Agent => agent;

    [Header("Interest object")]
    [SerializeField] private GameObject interestObjectPrefab;
    [SerializeField] private int maxInterestObjects = 5;
    [SerializeField] private float spawnTimer = 3f;
    public float SpawnTimer => spawnTimer;
    private int interestObjectCount = 0;

    [SerializeField] private HunterStatusUI statusUI;
    public HunterStatusUI StatusUI => statusUI;

    //
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private float safeZoneWaitTime = 3f;
    private bool isDead = false;
    public bool IsDead { get => isDead; set => isDead = value; }
    //

    public bool IsInSafeZone { get; set; } = false;
    public float SafeZoneWaitTime => safeZoneWaitTime;

    //dead
    public float RespawnTime => respawnTime;

    [Header("Pathfinding")]
    public GraphNode currentNode;
    public GraphNode exitNode;
    public GraphNode initialZoneNode;

    public PathDrawer PathDrawer { get; private set; }
    private Perception perception;
    private HunterVisuals visuals;
    public HunterVisuals Visuals => visuals;
    private void Awake()
    {
        GetComponents();
    }
    private void Start()
    {
        currentLife = life;
        initialSpawnPosition = transform.position;

        if (statusUI != null) statusUI.UpdateGathers(currentGathers, requiredGathers);

    }

    private void Update()
    {
        if (tbaTimer > 0f) tbaTimer -= Time.deltaTime;
    }

    public void GetComponents()
    {
        PathDrawer = GetComponentInChildren<PathDrawer>();
        agent = GetComponent<SteeringAgent>();
        perception = GetComponent<Perception>();
        visuals = GetComponent<HunterVisuals>();    
    }
    public Wolf GetClosestVisibleWolf()
    {
        if (perception == null) return null;

        Transform target = perception.GetClosestVisibleTarget(visionRadius, wolfLayer);
        Wolf wolf = target != null ? target.GetComponent<Wolf>() : null;

        return wolf != null && wolf.gameObject.activeInHierarchy ? wolf : null;
    }


    public bool MeleeAttack(Boid target)
    {
        if (target == null) return false;

        target.TakeDamage(1);

        return true;
    }

    public bool RangeAttack(Boid target)
    {
        if (target == null) return false;

        GameObject newProjectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Projectile projectile = newProjectile.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(target);
        }

        return true;
    }

    public void ResetAttackTimer()
    {
        tbaTimer = tba;
    }
    public void TakeDamage()
    {
        currentLife = 0;
    }
    public Boid GetClosestTarget()
    {
        return GetClosestTarget(true);
    }

    public Boid GetClosestDeadTarget()
    {
        return GetClosestTarget(false);
    }

    private Boid GetClosestTarget(bool alive)
    {
        if (perception == null) return null;

        Transform target = perception.GetClosestVisibleTarget(visionRadius, boidLayer);
        if (target == null) return null;

        Boid boid = target.GetComponentInParent<Boid>();
        if (boid == null || boid.GetIsAlive != alive) return null;

        return boid;
    }

    public void SpawnInterestObject()
    {
        if (interestObjectCount < maxInterestObjects)
        {
            GameObject newObject = Instantiate(interestObjectPrefab, transform.position, Quaternion.identity);
            newObject.GetComponent<InterestObject>().SetHunter(this);

            interestObjectCount++;
        }

    }
    public void RemoveInterestObject()
    {
        if (interestObjectCount > 0) interestObjectCount--;
    }

  
    public void SetCurrentTarget(Boid newTarget)
    {
        if (currentBoid != null)
        {
            currentBoid.SetTargetArrow(false);
        }

        currentBoid = newTarget;

        if (currentBoid != null)
        {
            currentBoid.SetTargetArrow(true);
        }
    }

    public void AddGather()
    {
        currentGathers++;

        if (statusUI != null)
        {
            statusUI.UpdateGathers(currentGathers, requiredGathers);
        }
    }
    public void ResetGathers()
    {
        currentGathers = 0;

        if (statusUI != null)
        {
            statusUI.UpdateGathers(currentGathers, requiredGathers);
        }
    }
    
    public bool HasLineOfSight(Transform target)
    {
        return perception != null && perception.HasLineOfSight(target);
    }

    public void Spawn()
    {
        currentLife = life;
        isDead = false;

        for (int i = 0; i < visuals.AllRenderers.Length; i++)
        {
            visuals.AllRenderers[i].material.color = visuals.InitialColours[i];
        }

        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in childColliders)
        {
            if (col.gameObject != this.gameObject)
            {
                col.enabled = true;
            }
        }
    }
    //path
    public bool NavigatePath(List<GraphNode> path, ref int pathIndex, float reachDistance, bool useArrive, System.Action onPathFinished)
    {
        if (path == null || path.Count == 0) return false;

        if (pathIndex >= path.Count)
        {
            onPathFinished?.Invoke();
            return true;
        }

        if (PathDrawer != null) PathDrawer.UpdateRemainingPath(path, pathIndex);

        GraphNode targetNode = path[pathIndex];
        Vector3 targetPos = targetNode.transform.position;
        targetPos.y = transform.position.y;

        float distance = Vector3.Distance(transform.position, targetPos);
        Debug.Log("Hunter: " + name + " | CurrentNode: " + (currentNode != null ? currentNode.name : "NULL") + " | TargetNode: " + targetNode.name + " | PathIndex: " + pathIndex + "/" + path.Count + " | Distance: " + distance);

        if (distance <= reachDistance)
        {
            currentNode = targetNode;
            pathIndex++;
        }
        else
        {
            Vector3 steering = useArrive ? Agent.Arrive(targetPos) : Agent.Seek(targetPos);
            Agent.ApplySteering(steering);
        }

        return false;
    }
}
