using TMPro;
using UnityEngine;

public class HunterStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text statusInfoText;

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

    private void HideActionText()
    {
        statusInfoText.gameObject.SetActive(false);
    }

}
