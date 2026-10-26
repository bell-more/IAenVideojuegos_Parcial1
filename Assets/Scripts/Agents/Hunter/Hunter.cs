using System.Collections.Generic;
using UnityEngine;
public class Hunter : Agent
{
    public List<Transform> targetsInRange = new List<Transform>();
    private readonly List<Transform> visibleTargets = new List<Transform>();
    [SerializeField] private LineRenderer lineRenderer;
    private Boid currentBoid;
    [SerializeField] private int life = 1;
    private int currentLife;
    private Vector3 initialSpawnPosition;
    public Vector3 InitialSpawnPosition => initialSpawnPosition;
    [SerializeField] private float visionRadius = 15f; 
    public float VisionRadius => visionRadius;
    [SerializeField] private LayerMask wolfLayer;
    [SerializeField] private LayerMask boidLayer;

    //colour
    private Renderer[] allRenderers;
    private Color[] initialColours;

    //safe area visual
    [SerializeField] private GameObject areaToFleeIndicator;
    private GameObject activeAreaToFleeIndicator;

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

    [SerializeField] private GameObject wolfDetectedImage;
    //
    private SteeringAgent agent;
    public SteeringAgent Agent => agent;

    [Header("Interest object")]
    [SerializeField] private GameObject interestObjectPrefab;
    [SerializeField] private int maxInterestObjects = 5;
    [SerializeField] private float spawnTimer = 3f;
    public float SpawnTimer { get; set; }
    private int interestObjectCount = 0;

    [SerializeField] private HunterStatusUI statusUI;
    public HunterStatusUI StatusUI => statusUI;

    //
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private float safeZoneWaitTime = 3f;
    private bool isDead = false;
    //
    
    public bool IsDead => isDead;
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

    private void Awake()
    {
        PathDrawer = GetComponentInChildren<PathDrawer>();
        agent = GetComponent<SteeringAgent>();
        allRenderers = GetComponentsInChildren<Renderer>(true);
        perception = GetComponent<Perception>();
    }
    private void Start()
    {
        currentLife = life;
        initialSpawnPosition = transform.position;

        if (statusUI != null)
        {
            statusUI.UpdateGathers(currentGathers, requiredGathers);
        }

        initialColours = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            initialColours[i] = allRenderers[i].material.color;
        }
    }

    private void Update()
    {
        if (tbaTimer > 0f) tbaTimer -= Time.deltaTime;

        RenderLine();
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

    public void RenderLine()
    {
        if (lineRenderer == null) return;

        targetsInRange.RemoveAll(t => t == null || !t.gameObject.activeInHierarchy);
        visibleTargets.Clear();

        foreach (Transform target in targetsInRange)
        {
            if (Vector3.Distance(transform.position, target.position) <= visionRadius && HasLineOfSight(target))
            {
                visibleTargets.Add(target);
            }
        }

        if (visibleTargets.Count == 0)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = visibleTargets.Count * 2;

        for (int i = 0; i < visibleTargets.Count; i++)
        {
            lineRenderer.SetPosition(i * 2, transform.position);
            lineRenderer.SetPosition(i * 2 + 1, visibleTargets[i].position);
        }
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
    public void TakeDamage()
    {
        currentLife = 0;
    }
    public bool HasLineOfSight(Transform target)
    {
        return perception != null && perception.HasLineOfSight(target);
    }

    public void ToggleVisibility()
    {
        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].material.color = Color.red;
        }

        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in childColliders)
        {
            if (col.gameObject != this.gameObject)
            {
                col.enabled = false;
            }
        }

        isDead = true;
    }

    public void ToggleWolfDetection(bool wasDetected)
    {
        wolfDetectedImage.SetActive(wasDetected);
    }

    public void EnableAreaToFleeIndicator(Vector3 safeZonePos, bool enable)
    {
        if (enable)
        {
            Vector3 spawnPos = new Vector3(safeZonePos.x, 5f, safeZonePos.z);
            activeAreaToFleeIndicator = Instantiate(areaToFleeIndicator, spawnPos, Quaternion.identity);
        }
        else Destroy(activeAreaToFleeIndicator);
    }

    public void Spawn()
    {
        currentLife = life;
        isDead = false;

        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].material.color = initialColours[i];
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

    //visuals
    private void OnDrawGizmos()
    {
        GroundCircleGizmo.Draw(transform.position, visionRadius, Color.yellow);
        GroundCircleGizmo.Draw(transform.position, rangeAttackRadius, Color.red);
        GroundCircleGizmo.Draw(transform.position, meleeAttackRadius, Color.green);
    }
}
