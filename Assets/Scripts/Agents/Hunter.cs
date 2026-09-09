using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Hunter : Agent
{
    [SerializeField] private LayerMask boidLayer;

    public List<Transform> targetsInRange = new List<Transform>();

    private StateMachine stateMachine;

    [Header("Waypoints")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float waypointCheckDistance = 1f;
    public List<Transform> Waypoints => waypoints;
    public float WaypointCheckDistance => waypointCheckDistance;

    [Header("Attack")]
    [SerializeField] private float tba = 3f;
    [SerializeField] private float rangeAttackRadius = 6f;
    [SerializeField] private float meleeAttackRadius = 2f;

    [Header("Gather")]
    [SerializeField] private float gatherTime = 2f;
    public float GetGatherTime => gatherTime;

    public float GetTBA => tba;
    public float GetRangeAttackRadius => rangeAttackRadius;
    public float GetMeleeAttackRadius => meleeAttackRadius;
    private float tbaTimer;
    public bool CanAttack { get { return tbaTimer <= 0f; } }

    private SteeringAgent agent;
    public SteeringAgent Agent => agent;

    [SerializeField] private GameObject interestObjectPrefab;
    [SerializeField] private float spawnTimer = 3f;
    private int interestObjectCount = 0;

    [SerializeField] private HunterStatusUI statusUI;
    public HunterStatusUI GetStatusUI => statusUI;

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

        Debug.Log("Hunter attacked a boid");
        target.TakeDamage(1);

        return true;
    }

    public void ResetAttackTimer()
    {
        tbaTimer = tba;
    }

    public Boid GetClosestTarget()
    {
        Boid closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform target in targetsInRange)
        {
            if (target == null) continue;

            Boid boid = target.GetComponent<Boid>();

            if (boid == null || !boid.GetIsAlive) continue;

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = boid;
            }
        }

        return closestTarget;
    }

    public Boid GetClosestDeadTarget()
    {
        Boid closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform target in targetsInRange)
        {
            if (target == null) continue;

            Boid boid = target.GetComponent<Boid>();

            if (boid == null || boid.GetIsAlive) continue; //ignore if it's null or alive

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
}
