using System.Collections.Generic;
using UnityEngine;

public class Wolf : MonoBehaviour
{
    [Header("FSM")]
    [SerializeField] private float nodeReachedDistance = 0.5f;
    public float NodeReachedDistance => nodeReachedDistance;
    [SerializeField] private float viewRadius = 10f;
    public float ViewRadius => viewRadius;
    [Range(0, 360)] public float viewAngle = 90f;
    public float ViewAngle => viewAngle;
    [SerializeField] private float captureRadius = 2.5f;
    public float CaptureRadius => captureRadius;
    [SerializeField] private LayerMask hunterLayer;
    [SerializeField] private float inspectDuration = 3f;
    public float InspectDuration => inspectDuration;
    [SerializeField] private float attackCooldown = 1.5f;
    public float AttackCooldown => attackCooldown;
    [SerializeField] private float targetMoveTolerance = 0.5f;
    public float TargetMoveTolerance => targetMoveTolerance;
    private FSMWolf wolfFSM;

    [Header("Patrol")]
    [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();
    [SerializeField] private bool usePingPongPatrol = false;
    public List<Transform> PatrolWaypoints => patrolWaypoints;
    public bool UsePingPongPatrol => usePingPongPatrol;

    private bool hasAlertedPack = false;
    public bool IsInvestigating { get; set; } = false;
    private static GameObject currentAreaInstance;
    public GameObject CurrentAreaInstance { get => currentAreaInstance; set => currentAreaInstance = value; }
    [SerializeField] private float hunterAreaHeight = 3f;

    private AudioSource audioSource;
    private Perception perception;
    private Hunter hunterTarget;

    public PathDrawer PathDrawer { get; private set; }
    public SteeringAgent Agent { get; private set; }
    public Vector3 LastKnownHunterPosition { get; private set; }
    public GraphNode currentNode;

    public bool IsAlerted { get; set; } = false;
    public bool IsSeeingHunter { get; private set; }
    public Hunter CurrentHunter { get; private set; }
    public bool HunterInSafeZone { get; private set; } = false;

    //visuals
    private WolfVisuals wolfVisuals;
    public WolfVisuals WolfVisuals => wolfVisuals;
                               

    private void Awake()
    {
        GetComponents();
    }

    private void OnEnable()
    {
        if (WolfManager.Instance != null)
        {
            WolfManager.Instance.RegisterWolf(this);
            WolfManager.Instance.OnHunterDetected += HandleHunterDetected;
            WolfManager.Instance.OnSafeZoneReached += HandleSafeZoneReached;
        }
    }

    private void Start()
    {
        if (WolfManager.Instance != null)
        {
            WolfManager.Instance.RegisterWolf(this);

            WolfManager.Instance.OnHunterDetected += HandleHunterDetected;
            WolfManager.Instance.OnSafeZoneReached -= HandleSafeZoneReached;
            WolfManager.Instance.OnSafeZoneReached += HandleSafeZoneReached;
        }
    }

    private void OnDisable()
    {
        if (WolfManager.Instance != null)
        {
            WolfManager.Instance.UnregisterWolf(this);
            WolfManager.Instance.OnHunterDetected -= HandleHunterDetected;
            WolfManager.Instance.OnSafeZoneReached -= HandleSafeZoneReached;
        }
    }

    private void Update()
    {
        HunterInSafeZone = hunterTarget != null && hunterTarget.IsInSafeZone;

        if (CheckHunterInSafeZone()) return;

        IsSeeingHunter = CanSeeHunter(out Vector3 hunterPosition);
        HandleHunterDetection(hunterPosition);
    }

    private void GetComponents()
    {
        PathDrawer = GetComponent<PathDrawer>();
        Agent = GetComponent<SteeringAgent>();
        audioSource = GetComponent<AudioSource>();
        perception = GetComponent<Perception>();
        wolfFSM = GetComponent<FSMWolf>();
        wolfVisuals = GetComponent<WolfVisuals>();  
    }
    private void HandleHunterDetection(Vector3 hunterPosition)
    {
        if (IsSeeingHunter)
        {
            LastKnownHunterPosition = hunterPosition;

            if (!hasAlertedPack)
            {
                wolfVisuals.ShowFeedback("Hunter detected", true);
                if (audioSource != null) audioSource.Play();

                Vector3 areaPos = new Vector3(LastKnownHunterPosition.x, hunterAreaHeight, LastKnownHunterPosition.z);

                if (currentAreaInstance == null)
                {
                    wolfVisuals.InstantiateHunterLastPositionSquare(areaPos);
                }
                else
                {
                    currentAreaInstance.transform.position = areaPos;
                }

                if (WolfManager.Instance != null)
                {
                    WolfManager.Instance.NotifyHunterDetected(LastKnownHunterPosition, this);
                }
                hasAlertedPack = true;
            }
        }
        else
        {
            hasAlertedPack = false;
        }
    }
    private bool CanSeeHunter(out Vector3 hunterPos)
    {
        hunterPos = Vector3.zero;

        if (perception == null) return false;

        Transform target = perception.GetClosestVisibleTarget(viewRadius, hunterLayer, true, viewAngle);
        Hunter hunter = target != null ? target.GetComponent<Hunter>() : null;

        if (hunter == null || hunter.Life <= 0 || hunter.IsInSafeZone)
        {
            CurrentHunter = null;
            return false;
        }

        hunterTarget = hunter;


        CurrentHunter = hunter;
        hunterPos = hunter.transform.position;
        return true;
    }

    private bool CheckHunterInSafeZone()
    {
        if (HunterInSafeZone)
        {
            IsSeeingHunter = false;
            CurrentHunter = null;
            hasAlertedPack = false;
            return true;
        }
        return false;
    }
    private void HandleHunterDetected(Vector3 alertPosition, Wolf sender)
    {
        if (sender == this) return;
        if (HunterInSafeZone) return;

        LastKnownHunterPosition = alertPosition;
        hasAlertedPack = true;
        IsAlerted = true;

        if (!IsSeeingHunter) wolfVisuals.ShowFeedback("Wolf alerted!", false);
    }

    private void HandleSafeZoneReached()
    {
        ForgetHunter();
        hunterTarget = null;
        if (wolfFSM != null) wolfFSM.ReturnToPatrol();
    }

    public void ForgetHunter()
    {
        IsSeeingHunter = false;
        CurrentHunter = null;
        hasAlertedPack = false;
        IsAlerted = false;
        IsInvestigating = false;
        LastKnownHunterPosition = Vector3.zero;

        wolfVisuals.HideFeedback();

        if (WolfManager.Instance != null && !WolfManager.Instance.IsAnyWolfActive())
        {
            if (currentAreaInstance != null)
            {
                Destroy(currentAreaInstance);
                currentAreaInstance = null;
            }
        }
    }

    public bool HasLineOfSightTo(Vector3 targetPos)
    {
        return perception != null && perception.HasLineOfSight(targetPos);
    }

    //path
    public bool NavigatePath(List<GraphNode> path, ref int pathIndex, float reachDistance, System.Action onPathFinished)
    {
        if (path == null || pathIndex >= path.Count)
        {
            onPathFinished?.Invoke();
            return true;
        }

        GraphNode targetNode = path[pathIndex];
        if (PathDrawer != null) PathDrawer.DrawNextNode(targetNode);

        Vector3 targetPos = targetNode.transform.position;
        targetPos.y = transform.position.y;

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= reachDistance)
        {
            currentNode = targetNode;
            pathIndex++;
        }
        else
        {
            Vector3 steering = Agent.Seek(targetPos);
            Agent.ApplySteering(steering);
        }

        return false;
    }
}