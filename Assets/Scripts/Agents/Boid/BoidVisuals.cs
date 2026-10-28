using UnityEngine;

public class BoidVisuals : MonoBehaviour
{
    [SerializeField] private Renderer childMaterial;
    [SerializeField] private HealthVisuals visuals;
    [SerializeField] private GameObject targetAura;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private HunterStatusUI statusUI;
    private Renderer[] allRenderers;
    private Color[] initialColours;

    private Boid boid;

    public void Awake()
    {
        boid = GetComponent<Boid>();
        allRenderers = GetComponentsInChildren<Renderer>(true);
        InitializeLife();
        SaveInitialColours();
    }
    public void InitializeLife()
    {
        boid.InitialLife = boid.Life;

        if (healthBar != null)
        {
            healthBar.Setup(boid.InitialLife);
        }
    }
    public void UpdateUI(string state, string action)
    {
        if (statusUI != null)
        {
            statusUI.SetStatus(state);
            statusUI.ShowAction(action);
        }
    }

    public void SetStatusUI()
    {
        if (statusUI != null)
        {
            statusUI.SetStatus("");
            statusUI.ShowAction("");
        }
    }
    public void ChangeColour(Color newColour)
    {
        if (childMaterial != null)
        {
            childMaterial.material.color = newColour;
        }
    }

    public void TakeDamageVisual()
    {
        if (healthBar != null) healthBar.UpdateHealth(boid.Life);

        if (visuals != null) visuals.SetColour(boid.Life);
    }

    public void ToggleVisual(bool isAlive)
    {
        childMaterial.GetComponent<Collider>().enabled = isAlive;

        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].enabled = isAlive;
            if(isAlive) allRenderers[i].material.color = initialColours[i];
        }
    }
    public void ToggleHealthBar(bool isAlive)
    {
        if (healthBar != null)
        {
            healthBar.SetActive(isAlive);
            if(isAlive) healthBar.UpdateHealth(boid.InitialLife);
        }
    }

    public void SetTargetArrow(bool isActive)
    {
        if (targetAura != null)
        {
            targetAura.SetActive(isActive);
        }
    }

    public void SaveInitialColours()
    {
        initialColours = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            initialColours[i] = allRenderers[i].material.color;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || boid == null) return;

        if (boid.Flocking != null)
        {
            GroundCircleGizmo.Draw(transform.position, boid.Flocking.SeparationRadius, Color.red);
            GroundCircleGizmo.Draw(transform.position, boid.Flocking.AlignmentRadius, Color.yellow);
            GroundCircleGizmo.Draw(transform.position, boid.Flocking.CohesionRadius, Color.cyan);
        }
    }
}
