using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    public void Setup(int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        gameObject.SetActive(true);
    }

    public void UpdateHealth(int currentHealth)
    {
        slider.value = currentHealth;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
