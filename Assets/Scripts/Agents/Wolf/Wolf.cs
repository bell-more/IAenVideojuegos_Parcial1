using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public bool IsInvestigating { get; set; } = false;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI detectionText;
    [SerializeField] private Image feedbackImage;    
    [SerializeField] private Sprite detectSprite;
    [SerializeField] private Sprite alertSprite;

    //visuals
    [SerializeField] private GameObject hunterLastSeenPositionArea;
    private static GameObject currentAreaInstance;
    [SerializeField] private float hunterAreaHeight = 3f;
    private AudioSource audioSource;
    public PathDrawer PathDrawer { get; private set; }

    public SteeringAgent Agent { get; private set; }
    public Vector3 LastKnownHunterPosition { get; private set; }
    public GraphNode currentNode;

    public bool IsAlerted { get; set; } = false;
    public bool IsSeeingHunter { get; private set; }
    public Hunter CurrentHunter { get; private set; }


    private void Awake()
    {
        PathDrawer = GetComponent<PathDrawer>();
        Agent = GetComponent<SteeringAgent>();
        audioSource = GetComponent<AudioSource>();

        HideFeedback();
    }

    private void Update()
    {
        perceptionTimer -= Time.deltaTime;

        if (perceptionTimer <= 0f)
        {
            perceptionTimer = perceptionTickRate;

            IsSeeingHunter = CanSeeHunter(out Vector3 hunterPosition);

            if (IsSeeingHunter)
            {
                LastKnownHunterPosition = hunterPosition;

                if (!hasAlertedPack)
                {
                    ShowFeedback("Hunter detected", detectSprite);
                    audioSource.Play();

                    Vector3 areaPos = new Vector3(LastKnownHunterPosition.x,hunterAreaHeight, LastKnownHunterPosition.z);
                    if (currentAreaInstance == null)
                    {
                        currentAreaInstance = Instantiate(hunterLastSeenPositionArea, areaPos, Quaternion.identity);
                    }
                    else
                    {
                        currentAreaInstance.transform.position = areaPos;
                    }

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

    private bool CanSeeHunter(out Vector3 hunterPos)
    {
        hunterPos = Vector3.zero;
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, hunterLayer);

        foreach (Collider target in targetsInRadius)
        {
            Hunter hunter = target.GetComponentInParent<Hunter>();

            if (hunter == null || hunter.Life <= 0) continue;

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

        if (!IsSeeingHunter)
        {
            ShowFeedback("Wolf alerted!", alertSprite);
        }
    }
    public void ForgetHunter()
    {
        IsSeeingHunter = false;
        CurrentHunter = null;
        hasAlertedPack = false;
        IsAlerted = false;
        IsInvestigating = false;

        HideFeedback();

        bool wolfStillInvestigating = false;

        foreach (Wolf alliedWolf in packMembers)
        {
            if (alliedWolf != null && alliedWolf != this)
            {
                if (alliedWolf.IsSeeingHunter || alliedWolf.IsAlerted || alliedWolf.IsInvestigating)
                {
                    wolfStillInvestigating = true;
                    break;
                }
            }
        }

        if (!wolfStillInvestigating)
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
        Vector3 origin = transform.position + (Vector3.up * eyeHeight);
        Vector3 targetEye = targetPos + (Vector3.up * eyeHeight);
        Vector3 direction = (targetEye - origin);

        return !Physics.Raycast(origin, direction.normalized, direction.magnitude, obstacleLayer);
    }
    public void UpdateUI(string state, string info)
    {
        if (stateText != null) stateText.text = state;
        if (infoText != null) infoText.text = info;
    }

    private void OnDrawGizmos()
    {

        Vector3 viewAngleA = DirFromAngle(transform.eulerAngles.y, -viewAngle / 2f);
        Vector3 viewAngleB = DirFromAngle(transform.eulerAngles.y, viewAngle / 2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    public void ShowFeedback(string text, Sprite icon)
    {
        if (detectionText != null)
        {
            detectionText.text = text;
            detectionText.gameObject.SetActive(true); 
        }

        if (feedbackImage != null && icon != null)
        {
            feedbackImage.sprite = icon;
            feedbackImage.gameObject.SetActive(true);
        }
    }

    public void HideFeedback()
    {
        if (detectionText != null) detectionText.gameObject.SetActive(false); 
        if (feedbackImage != null) feedbackImage.gameObject.SetActive(false); 
    }

    private Vector3 DirFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}