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

    [ContextMenu("Generate connections")]///////////////////
    public void GenerateConnections()
    {
        allNodes = new List<GraphNode>(GetComponentsInChildren<GraphNode>());

        foreach (GraphNode node in allNodes)
        {
            node.neighbours.Clear();

            foreach (GraphNode potentialNeighbour in allNodes)
            {
                if (node == potentialNeighbour) continue;

                float distance = Vector3.Distance(node.transform.position, potentialNeighbour.transform.position);

                if (distance <= maxConnectionDistance)
                {
                    Vector3 direction = potentialNeighbour.transform.position - node.transform.position;
                    if (!Physics.Raycast(node.transform.position, direction, distance, obstacleLayer))
                    {
                        node.neighbours.Add(potentialNeighbour);
                    }
                }
            }
        }
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