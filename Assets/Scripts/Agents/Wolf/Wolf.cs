using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Wolf : MonoBehaviour
{
    [SerializeField] private float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    [SerializeField] private float captureRadius = 2.5f;
    public float CaptureRadius => captureRadius;
    [SerializeField] private LayerMask hunterLayer;

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

    [SerializeField] private GameObject hunterLastSeenPositionArea;
    private static GameObject currentAreaInstance;
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

    private void Awake()
    {
        PathDrawer = GetComponent<PathDrawer>();
        Agent = GetComponent<SteeringAgent>();
        audioSource = GetComponent<AudioSource>();
        perception = GetComponent<Perception>();
        HideFeedback();
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
        if (hunterTarget == null) hunterTarget = FindFirstObjectByType<Hunter>();
        HunterInSafeZone = hunterTarget != null && hunterTarget.IsInSafeZone;

        if (HunterInSafeZone)
        {
            IsSeeingHunter = false;
            CurrentHunter = null;
            hasAlertedPack = false;
            return;
        }

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

        CurrentHunter = hunter;
        hunterPos = hunter.transform.position;
        return true;
    }

    private void HandleHunterDetected(Vector3 alertPosition, Wolf sender)
    {
        if (sender == this) return;
        if (HunterInSafeZone) return;

        LastKnownHunterPosition = alertPosition;
        hasAlertedPack = true;
        IsAlerted = true;

        if (!IsSeeingHunter) ShowFeedback("Wolf alerted!", alertSprite);
    }
    private void HandleSafeZoneReached()
    {
        ForgetHunter();
    }

    public void ForgetHunter()
    {
        IsSeeingHunter = false;
        CurrentHunter = null;
        hasAlertedPack = false;
        IsAlerted = false;
        IsInvestigating = false;
        LastKnownHunterPosition = Vector3.zero;

        HideFeedback();

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

    public void UpdateUI(string state, string info)
    {
        if (stateText != null) stateText.text = state;
        if (infoText != null) infoText.text = info;
    }

    private void OnDrawGizmos()
    {
        GroundCircleGizmo.DrawVisionCone(transform.position, transform.eulerAngles.y, viewRadius, viewAngle, Color.yellow);
        GroundCircleGizmo.DrawArc(transform.position, transform.eulerAngles.y, CaptureRadius, viewAngle, Color.red);
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
}