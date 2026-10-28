using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WolfVisuals : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI detectionText;
    [SerializeField] private GameObject hunterLastSeenPositionArea;
    [SerializeField] private Image feedbackImage;
    [SerializeField] private Sprite detectSprite;
    [SerializeField] private Sprite alertSprite;
    [SerializeField] private Sprite hunterLostSprite;
    public Sprite HunterLostSprite => hunterLostSprite;
    private Wolf wolf;
    private void Awake() 
    {
        wolf = GetComponent<Wolf>();    
        HideFeedback();
    }
    public void UpdateUI(string state, string info)
    {
        if (stateText != null) stateText.text = state;
        if (infoText != null) infoText.text = info;
    }

    private void OnDrawGizmos()
    {
        if (wolf == null) return;
        GroundCircleGizmo.DrawVisionCone(transform.position, transform.eulerAngles.y, wolf.ViewRadius, wolf.ViewAngle, Color.yellow);
        GroundCircleGizmo.DrawArc(transform.position, transform.eulerAngles.y, wolf.CaptureRadius, wolf.ViewAngle, Color.red);
    }

    public void ShowFeedback(string text, bool hasAlerted, bool lostHunter = false)
    {
        if (detectionText != null)
        {
            detectionText.text = text;
            detectionText.gameObject.SetActive(true);
        }

        if (feedbackImage != null)
        {
            feedbackImage.gameObject.SetActive(true);

            if (hasAlerted) feedbackImage.sprite = detectSprite;
            else feedbackImage.sprite = alertSprite;
            
        }
        if (lostHunter) feedbackImage.sprite = hunterLostSprite;
    }

    public void HideFeedback()
    {
        if (detectionText != null) detectionText.gameObject.SetActive(false);
        if (feedbackImage != null) feedbackImage.gameObject.SetActive(false);
    }

    public void InstantiateHunterLastPositionSquare(Vector3 areaPos)
    {
        if (hunterLastSeenPositionArea != null)
        {
            wolf.CurrentAreaInstance = Instantiate(hunterLastSeenPositionArea, areaPos, Quaternion.identity);
        }
    }
}
