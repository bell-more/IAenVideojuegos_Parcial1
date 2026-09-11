using System.Collections.Generic;
using UnityEngine;
public class Hunter : Agent
{

    public List<Transform> targetsInRange = new List<Transform>();

    [Header("Visuals")]
    [SerializeField] private LineRenderer lineRenderer;
    private Boid currentBoid;

    [Header("Waypoints")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float waypointCheckDistance = 1f;
    public List<Transform> Waypoints => waypoints;
    public float WaypointCheckDistance => waypointCheckDistance;

    [Header("Attack")]
    [SerializeField] private float tba = 3f;
    [SerializeField] private float rangeAttackRadius = 6f;
    [SerializeField] private float meleeAttackRadius = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    public float RangeAttackRadius => rangeAttackRadius;
    public float MeleeAttackRadius => meleeAttackRadius;
    private float tbaTimer;
    public bool CanAttack { get { return tbaTimer <= 0f; } }


    [Header("Gather")]
    [SerializeField] private float gatherTime = 2f;
    public float GatherTime => gatherTime;


    private SteeringAgent agent;
    public SteeringAgent Agent => agent;

    [SerializeField] private GameObject interestObjectPrefab;
    [SerializeField] private float spawnTimer = 3f;
    private int interestObjectCount = 0;

    [SerializeField] private HunterStatusUI statusUI;
    public HunterStatusUI StatusUI => statusUI;

    private void Awake()
    {
        agent = GetComponent<SteeringAgent>();
    }

    private void Update()
    {
        if (tbaTimer > 0f) tbaTimer -= Time.deltaTime;

        if (GetClosestTarget() == null && GetClosestDeadTarget() == null)
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
        }

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
}
