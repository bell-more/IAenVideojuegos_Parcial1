using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinding
{
    public static List<GraphNode> FindPath(GraphNode start, GraphNode goal, bool excludeSafeZones = false)
    {
        List<GraphNode> path = new List<GraphNode>();

        if (start == null || goal == null) return path;

        GraphManager graph = GraphManager.Instance;
        if (graph == null) return path;

        if (excludeSafeZones && graph.safeZoneNodes.Contains(goal)) return path;
        if (start == goal)
        {
            path.Add(start);
            return path;
        }

        PriorityQueue<GraphNode> frontier = new PriorityQueue<GraphNode>();
        Dictionary<GraphNode, GraphNode> cameFrom = new Dictionary<GraphNode, GraphNode>();
        Dictionary<GraphNode, float> costSoFar = new Dictionary<GraphNode, float>();
        HashSet<GraphNode> processed = new HashSet<GraphNode>();

        frontier.Enqueue(start, 0f);
        costSoFar[start] = 0f;

        while (frontier.Count > 0)
        {
            GraphNode current = frontier.Dequeue();

            if (processed.Contains(current)) continue;
            processed.Add(current);

            if (current == goal) break;

            foreach (GraphNode neighbour in current.neighbours)
            {
                if (neighbour == null) continue;
                if (excludeSafeZones && graph.safeZoneNodes.Contains(neighbour)) continue;
                if (processed.Contains(neighbour)) continue;

                float newCost = costSoFar[current] + Vector3.Distance(current.transform.position, neighbour.transform.position);

                if (!costSoFar.ContainsKey(neighbour) || newCost < costSoFar[neighbour])
                {
                    costSoFar[neighbour] = newCost;
                    cameFrom[neighbour] = current;

                    float heuristic = Vector3.Distance(neighbour.transform.position, goal.transform.position);
                    frontier.Enqueue(neighbour, newCost + heuristic);
                }
            }
        }

        if (!costSoFar.ContainsKey(goal)) return path;

        GraphNode node = goal;
        path.Add(node);

        while (node != start)
        {
            if (!cameFrom.ContainsKey(node)) return new List<GraphNode>();
            node = cameFrom[node];
            path.Add(node);
        }

        path.Reverse();
        return path;
    }
}
