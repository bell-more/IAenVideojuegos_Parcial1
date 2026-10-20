using System.Collections.Generic;
using UnityEngine;

public class GraphNode : MonoBehaviour
{
    public List<GraphNode> neighbors = new List<GraphNode>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        Gizmos.color = Color.yellow;
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}