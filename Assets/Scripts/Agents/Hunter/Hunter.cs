using System.Collections.Generic;
using UnityEngine;
public class Hunter : Agent
{
    public List<Transform> targetsInRange = new List<Transform>();
    [SerializeField] private LineRenderer lineRenderer;
    private Boid currentBoid;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float eyeHeight = 1f;
    [SerializeField] private int life = 1;
    private int currentLife;
    private Vector3 initialSpawnPosition;
    public Vector3 InitialSpawnPosition => initialSpawnPosition;
    [SerializeField] private float visionRadius = 15f; 
    [SerializeField] private LayerMask wolfLayer;

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

    
    //
    private SteeringAgent agent;
    public SteeringAgent Agent => agent;

    [SerializeField] private GameObject interestObjectPrefab;
    [SerializeField] private float spawnTimer = 3f;
    private int interestObjectCount = 0;

    [SerializeField] private HunterStatusUI statusUI;
    public HunterStatusUI StatusUI => statusUI;

    //
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private float safeZoneWaitTime = 3f;
    //
   
    public float SafeZoneWaitTime => safeZoneWaitTime;

    //dead
    public float RespawnTime => respawnTime;

    [Header("Pathfinding")]
    public GraphNode currentNode;
    public GraphNode exitNode;
    public GraphNode initialZoneNode;
    private void Awake()
    {
        agent = GetComponent<SteeringAgent>();
    }
    private void Start()
    {
        currentLife = life;
        initialSpawnPosition = transform.position;

        if (statusUI != null)
        {
            statusUI.UpdateGathers(currentGathers, requiredGathers);
        }
    }

    private void Update()
    {
        if (tbaTimer > 0f) tbaTimer -= Time.deltaTime;

       /* if (GetClosestTarget() == null && GetClosestDeadTarget() == null)
        {
            if (spawnTimer > 0f)
            {
                spawnTimer -= Time.deltaTime;
            }
            else
            {
                SpawnInterestObject();
                spawnTimer = 3f;
            }
        }*/

        RenderLine();
    }

    private void OnTriggerEnter(Collider other)
    {
        Boid boid = other.GetComponent<Boid>();

        if (boid != null && boid.gameObject != gameObject)
        {
            if (!targetsInRange.Contains(other.transform))
            {
                targetsInRange.Add(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {
            targetsInRange.Remove(other.transform);
        }
    }

    public Wolf GetClosestVisibleWolf()
    {
        Wolf closestWolf = null;
        float closestDistance = Mathf.Infinity;

        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, visionRadius, wolfLayer);

        foreach (Collider target in targetsInRadius)
        {
            Wolf wolf = target.GetComponent<Wolf>();

            if (wolf == null)
            {
                continue;
            }

            bool hasLOS = HasLineOfSight(target.transform);

            if (hasLOS)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWolf = wolf;
                }
            }
        }

        return closestWolf;
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
        Boid closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform target in targetsInRange)
        {
            if (target == null) continue;

            Boid boid = target.GetComponent<Boid>();

            if (boid == null || boid.GetIsAlive != alive)
                continue;

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = boid;
            }
        }

        return closestTarget;
    }

    public void SpawnInterestObject()
    {
        if (interestObjectCount < 5)
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

        if (targetsInRange.Count > 0)
        {
            lineRenderer.enabled = true;

            lineRenderer.positionCount = targetsInRange.Count * 2;

            for (int i = 0; i < targetsInRange.Count; i++)
            {
                lineRenderer.SetPosition(i * 2, transform.position);

                lineRenderer.SetPosition((i * 2) + 1, targetsInRange[i].position);
            }
        }
        else
        {
            lineRenderer.enabled = false;
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
        Vector3 origin = transform.position + (Vector3.up * eyeHeight);
        Vector3 targetPos = target.position + (Vector3.up * eyeHeight);
        Vector3 direction = targetPos - origin;

        if (Physics.Raycast(origin, direction, direction.magnitude, obstacleLayer))
        {
            return false;
        }

        return true;
    }

    public void ToggleVisibility(bool isVisible)
    {
        currentLife = life;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = isVisible;

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in allRenderers)
        {
            r.enabled = isVisible;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}
