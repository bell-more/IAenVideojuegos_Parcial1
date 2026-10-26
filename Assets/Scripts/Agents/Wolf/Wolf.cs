
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Wolf : MonoBehaviour
{
    [SerializeField] private float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    [SerializeField] private LayerMask hunterLayer;
    [SerializeField] private List<Wolf> packMembers = new List<Wolf>();

    [Header("Patrol")]
    [SerializeField] private List<Transform> patrolWaypoints = new List<Transform>();
    [SerializeField] private bool usePingPongPatrol = false;
    public List<Transform> PatrolWaypoints => patrolWaypoints;
    public bool UsePingPongPatrol => usePingPongPatrol;

    private bool hasAlertedPack = false;
    public bool IsInvestigating { get; set; } = false;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI detectionText;
    [SerializeField] private Image feedbackImage;
    [SerializeField] private Sprite detectSprite;
    [SerializeField] private Sprite alertSprite;
    //
    [SerializeField] private GameObject hunterLastSeenPositionArea;
    private static GameObject currentAreaInstance;
    [SerializeField] private float hunterAreaHeight = 3f;

    private AudioSource audioSource;
    private Perception perception;

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
        perception = GetComponent<Perception>();
        HideFeedback();
    }

    private void Update()
    {
        IsSeeingHunter = CanSeeHunter(out Vector3 hunterPosition);

        if (IsSeeingHunter)
        {
            LastKnownHunterPosition = hunterPosition;

            if (!hasAlertedPack)
            {
                ShowFeedback("Hunter detected", detectSprite);
                if (audioSource != null) audioSource.Play();

                Vector3 areaPos = new Vector3(LastKnownHunterPosition.x, hunterAreaHeight, LastKnownHunterPosition.z);

                if (currentAreaInstance == null)
                {
                    if (hunterLastSeenPositionArea != null) currentAreaInstance = Instantiate(hunterLastSeenPositionArea, areaPos, Quaternion.identity);
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

        CurrentHunter = hunter;
        hunterPos = hunter.transform.position;
        return true;
    }

    private void AlertPack(Vector3 detectedPosition)
    {
        foreach (Wolf alliedWolf in packMembers)
        {
            if (alliedWolf != null && alliedWolf != this) alliedWolf.ReceiveAlert(detectedPosition);
        }
    }

    public void ReceiveAlert(Vector3 alertPosition)
    {
        LastKnownHunterPosition = alertPosition;
        hasAlertedPack = true;
        IsAlerted = true;

        if (!IsSeeingHunter) ShowFeedback("Wolf alerted!", alertSprite);
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
            if (alliedWolf != null && alliedWolf != this && (alliedWolf.IsSeeingHunter || alliedWolf.IsAlerted || alliedWolf.IsInvestigating))
            {
                wolfStillInvestigating = true;
                break;
            }
        }

        if (!wolfStillInvestigating && currentAreaInstance != null)
        {
            Destroy(currentAreaInstance);
            currentAreaInstance = null;
        }
    }

    public bool HasLineOfSightTo(Vector3 targetPos)
    {
        return perception != null && perception.HasLineOfSight(targetPos);
    }

    public void UpdateUI(string state, string info)
    {
        if (stateText != null) stateText.text = state;
        if (infoText != null) infoText.text = info;
    }

    private void OnDrawGizmos()
    {
        Vector3 previousPoint = transform.position + DirFromAngle(transform.eulerAngles.y, -viewAngle / 2f) * viewRadius;

        Gizmos.color = Color.yellow;

        for (int i = 1; i <= 30; i++)
        {
            float angle = -viewAngle / 2f + viewAngle * i / 30f;
            Vector3 currentPoint = transform.position + DirFromAngle(transform.eulerAngles.y, angle) * viewRadius;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        Vector3 viewAngleA = DirFromAngle(transform.eulerAngles.y, -viewAngle / 2f);
        Vector3 viewAngleB = DirFromAngle(transform.eulerAngles.y, viewAngle / 2f);

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
