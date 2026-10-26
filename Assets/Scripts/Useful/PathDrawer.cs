using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathDrawer : MonoBehaviour
{
    [SerializeField] private float heightOffset = 0.15f;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.useWorldSpace = true;
    }

    public void UpdateRemainingPath(List<GraphNode> path, int currentIndex)
    {
        if (path == null || currentIndex >= path.Count)
        {
            ClearPath();
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.useWorldSpace = true;

        int remainingCount = path.Count - currentIndex;
        lineRenderer.positionCount = remainingCount + 1;

        Vector3 agentFeet = transform.position;
        agentFeet.y += heightOffset;
        lineRenderer.SetPosition(0, agentFeet);

        for (int i = 0; i < remainingCount; i++)
        {
            Vector3 nodePos = path[currentIndex + i].transform.position;
            nodePos.y += heightOffset;
            lineRenderer.SetPosition(i + 1, nodePos);
        }
    }

    public void DrawNextNode(GraphNode nextNode)
    {
        if (nextNode == null)
        {
            ClearPath();
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;

        Vector3 startPos = transform.position;
        startPos.y += heightOffset;
        lineRenderer.SetPosition(0, startPos);

        Vector3 targetPos = nextNode.transform.position;
        targetPos.y += heightOffset;
        lineRenderer.SetPosition(1, targetPos);
    }
    public void ClearPath()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }
}