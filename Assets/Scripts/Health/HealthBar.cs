using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    public void Setup(int maxHealth)
    {
        Debug.Log("Setting health bar");
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        gameObject.SetActive(true);
    }

    public void UpdateHealth(int currentHealth)
    {
        slider.value = currentHealth;
    }

    public void Toggle(bool state)
    {
        gameObject.SetActive(state);
    }
}
