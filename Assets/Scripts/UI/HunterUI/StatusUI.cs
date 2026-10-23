using TMPro;
using UnityEngine;

public class HunterStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text statusInfoText;
    [SerializeField] private TMP_Text gathersText;

    public void SetStatus(string status)
    {
        statusText.text = status;
    }
    public void ShowAction(string message)
    {
        if (statusInfoText == null) return;

        statusInfoText.text = message;
        statusInfoText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideActionText));

        Invoke(nameof(HideActionText), 1f);
    }
    public void UpdateGathers(int current, int required)
    {
        if (gathersText != null)
        {
            gathersText.text = current+"/"+required;
        }
    }

    private void HideActionText()
    {
        statusInfoText.gameObject.SetActive(false);
    }

}
