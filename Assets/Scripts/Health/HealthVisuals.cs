using System;
using System.Collections.Generic;
using UnityEngine;

public class HealthVisuals : MonoBehaviour
{
    [SerializeField] private List<Material> bodyColours = new List<Material>();
    private Renderer bodyRenderer;

    public void Awake()
    {
        bodyRenderer = GetComponent<Renderer>();
    }
    public void SetColour(int currentLife)
    {
        if (bodyRenderer == null || bodyColours.Count == 0) return;

        int index = Mathf.Clamp(currentLife, 0, bodyColours.Count - 1);
        bodyRenderer.material = bodyColours[index];
    }
}
