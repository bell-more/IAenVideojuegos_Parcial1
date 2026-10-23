using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinding
{
    private class PathRecord
    {
        public GraphNode node;
        public GraphNode parent;
        public float gCost;
        public float fCost; 
    }

    public static List<GraphNode> FindPath(GraphNode startNode, GraphNode targetNode)
    {
        if (startNode == null || targetNode == null) return new List<GraphNode>();

        PriorityQueue<PathRecord> frontier = new PriorityQueue<PathRecord>();
        Dictionary<GraphNode, PathRecord> costSoFar = new Dictionary<GraphNode, PathRecord>();

        PathRecord startRecord = new PathRecord { node = startNode, parent = null, gCost = 0, fCost = 0 };
        frontier.Enqueue(startRecord, 0);
        costSoFar[startNode] = startRecord;

        while (frontier.Count > 0)
        {
            PathRecord currentRecord = frontier.Dequeue();
            GraphNode current = currentRecord.node;

            if (current == targetNode)
            {
                return RetracePath(currentRecord, costSoFar);
            }

            foreach (GraphNode neighbor in current.neighbours)
            {
                float stepCost = Vector3.Distance(current.transform.position, neighbor.transform.position);
                float newCost = currentRecord.gCost + stepCost;

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor].gCost)
                {
                    float heuristic = Vector3.Distance(neighbor.transform.position, targetNode.transform.position);
                    float priority = newCost + heuristic;

                    PathRecord neighborRecord = new PathRecord
                    {
                        node = neighbor,
                        parent = current,
                        gCost = newCost,
                        fCost = priority
                    };

                    costSoFar[neighbor] = neighborRecord;
                    frontier.Enqueue(neighborRecord, priority);
                }
            }
        }

        return new List<GraphNode>();
    }

    private static List<GraphNode> RetracePath(PathRecord endRecord, Dictionary<GraphNode, PathRecord> records)
    {
        List<GraphNode> path = new List<GraphNode>();
        PathRecord current = endRecord;

        while (current != null && current.parent != null)
        {
            path.Add(current.node);
            current = records[current.parent];
        }

        path.Reverse();
        return path;
    }
}