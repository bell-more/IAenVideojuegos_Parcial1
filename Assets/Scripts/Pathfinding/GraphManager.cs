using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    public static GraphManager Instance { get; private set; }

    [Header("Nodes")]
    [SerializeField] private List<GraphNode> allNodes = new List<GraphNode>();
    public List<GraphNode> cotoNodes = new List<GraphNode>(); 
    public List<GraphNode> safeZoneNodes = new List<GraphNode>(); 

    [Header("config")]
    [SerializeField] private float maxConnectionDistance = 5f;
    [SerializeField] private LayerMask obstacleLayer; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GraphNode GetClosestNode(Vector3 position, bool excludeSafeZones = false)
    {
        GraphNode closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GraphNode node in allNodes)
        {
            if (excludeSafeZones && safeZoneNodes.Contains(node)) continue;

            float dist = Vector3.Distance(position, node.transform.position);

            if (dist < minDistance)
            {
                Vector3 dir = node.transform.position - position;
                if (!Physics.Raycast(position, dir, dist, obstacleLayer))
                {
                    minDistance = dist;
                    closest = node;
                }
            }
        }
        return closest;
    }

    public GraphNode GetRandomCotoNode()
    {
        if (cotoNodes.Count == 0) return null;
        return cotoNodes[Random.Range(0, cotoNodes.Count)];
    }
}