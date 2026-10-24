using System.Collections.Generic;
using UnityEngine;

public class Wolf : MonoBehaviour
{
    [SerializeField] private float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    [SerializeField] private float eyeHeight = 1f;
    [SerializeField] private LayerMask hunterLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private List<Wolf> packMembers = new List<Wolf>();

    [Header("Patrol")]
    [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();
    [SerializeField] private bool usePingPongPatrol = false;
    public List<Transform> PatrolWaypoints => patrolWaypoints;
    public bool UsePingPongPatrol => usePingPongPatrol;

    private bool hasAlertedPack = false;
    private float perceptionTimer = 0f;
    private float perceptionTickRate = 0.2f;

    public SteeringAgent Agent { get; private set; }
    public Vector3 LastKnownHunterPosition { get; private set; }
    public GraphNode currentNode;

    public bool IsAlerted { get; set; } = false;
    public bool IsSeeingHunter { get; private set; }
    public Hunter CurrentHunter { get; private set; }

    private void Awake()
    {
        Agent = GetComponent<SteeringAgent>();
    }

    private void Update()
    {
        perceptionTimer -= Time.deltaTime;

        if (perceptionTimer <= 0f)
        {
            perceptionTimer = perceptionTickRate;

            IsSeeingHunter = CheckCanSeeHunter(out Vector3 hunterPosition);

            if (IsSeeingHunter)
            {
                LastKnownHunterPosition = hunterPosition;

                if (!hasAlertedPack)
                {
                    AlertPack(LastKnownHunterPosition);
                    hasAlertedPack = true;
                }
            }
            else
            {
                hasAlertedPack = false;
            }
        }
    }

    private bool CheckCanSeeHunter(out Vector3 hunterPos)
    {
        hunterPos = Vector3.zero;
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, hunterLayer);

        foreach (Collider target in targetsInRadius)
        {
            Hunter hunter = target.GetComponent<Hunter>();

            if (hunter == null) continue;

            Vector3 dirToTarget = (hunter.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle <= viewAngle / 2f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, hunter.transform.position);
                Vector3 origin = transform.position + (Vector3.up * eyeHeight);
                Vector3 targetEye = hunter.transform.position + (Vector3.up * eyeHeight);
                Vector3 dirToEye = targetEye - origin;

                if (!Physics.Raycast(origin, dirToEye.normalized, distanceToTarget, obstacleLayer))
                {
                    hunterPos = hunter.transform.position;
                    CurrentHunter = hunter; 
                    return true;
                }
            }
        }

        CurrentHunter = null;
        return false;
    }

    private void AlertPack(Vector3 detectedPosition)
    {
        foreach (Wolf alliedWolf in packMembers)
        {
            if (alliedWolf != null && alliedWolf != this)
            {
                alliedWolf.ReceiveAlert(detectedPosition);
            }
        }
    }

    public void ReceiveAlert(Vector3 alertPosition)
    {
        LastKnownHunterPosition = alertPosition;
        hasAlertedPack = true;
        IsAlerted = true;
    }

    public bool HasLineOfSightTo(Vector3 targetPos)
    {
        Vector3 origin = transform.position + (Vector3.up * eyeHeight);
        Vector3 targetEye = targetPos + (Vector3.up * eyeHeight);
        Vector3 direction = (targetEye - origin);

        return !Physics.Raycast(origin, direction.normalized, direction.magnitude, obstacleLayer);
    }

    private void OnDrawGizmos()
    {

        Vector3 viewAngleA = DirFromAngle(transform.eulerAngles.y, -viewAngle / 2f);
        Vector3 viewAngleB = DirFromAngle(transform.eulerAngles.y, viewAngle / 2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    private Vector3 DirFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}