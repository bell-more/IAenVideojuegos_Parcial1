using System.Collections.Generic;
using UnityEngine;

public class HunterVisuals : MonoBehaviour
{
    private Hunter hunter;
    private readonly List<Transform> visibleTargets = new List<Transform>();
    private Renderer[] allRenderers;
    public Renderer[] AllRenderers => allRenderers;
    private Color[] initialColours;
    public Color[] InitialColours => initialColours;        
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject areaToFleeIndicator;
    private GameObject activeAreaToFleeIndicator;
    [SerializeField] private GameObject wolfDetectedImage;

    public void Start()
    {
        allRenderers = GetComponentsInChildren<Renderer>(true);
        hunter = GetComponent<Hunter>();
        SaveInitialColours();
    }

    public void Update()
    {
        RenderLine();
    }
    public void ToggleWolfDetection(bool wasDetected)
    {
        wolfDetectedImage.SetActive(wasDetected);
    }
    public void ToggleVisibility()
    {
        for (int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].material.color = Color.red;
        }

        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in childColliders)
        {
            if (col.gameObject != this.gameObject)
            {
                col.enabled = false;
            }
        }

        hunter.IsDead = true;
    }
    public void EnableAreaToFleeIndicator(Vector3 safeZonePos, bool enable)
    {
        if (enable)
        {
            Vector3 spawnPos = new Vector3(safeZonePos.x, 5f, safeZonePos.z);
            activeAreaToFleeIndicator = Instantiate(areaToFleeIndicator, spawnPos, Quaternion.identity);
        }
        else Destroy(activeAreaToFleeIndicator);
    }

    private void SaveInitialColours()
    {
        initialColours = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            initialColours[i] = allRenderers[i].material.color;
        }
    }
    private void OnDrawGizmos()
    {
        if (hunter == null) return;
        GroundCircleGizmo.Draw(transform.position, hunter.VisionRadius, Color.yellow);
        GroundCircleGizmo.Draw(transform.position, hunter.RangeAttackRadius, Color.red);
        GroundCircleGizmo.Draw(transform.position, hunter.MeleeAttackRadius, Color.red);
    }


    public void RenderLine()
    {
        if (lineRenderer == null) return;

        hunter.targetsInRange.RemoveAll(t => t == null || !t.gameObject.activeInHierarchy);
        visibleTargets.Clear();

        foreach (Transform target in hunter.targetsInRange)
        {
            if (Vector3.Distance(transform.position, target.position) <= hunter.VisionRadius && hunter.HasLineOfSight(target))
            {
                visibleTargets.Add(target);
            }
        }

        if (visibleTargets.Count == 0)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = visibleTargets.Count * 2;

        for (int i = 0; i < visibleTargets.Count; i++)
        {
            lineRenderer.SetPosition(i * 2, transform.position);
            lineRenderer.SetPosition(i * 2 + 1, visibleTargets[i].position);
        }
    }
}
