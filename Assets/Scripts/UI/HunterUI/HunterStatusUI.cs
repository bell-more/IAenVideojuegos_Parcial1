using TMPro;
using UnityEngine;

public class HunterStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    public void SetStatus(string status)
    {
        statusText.text = status;
    }
}
