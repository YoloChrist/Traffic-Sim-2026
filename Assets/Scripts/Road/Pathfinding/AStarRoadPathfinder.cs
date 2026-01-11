using System.Collections.Generic;
using UnityEngine;

// A* pathfinding algorithm
public class AStarPathfinder : IRoadPathfinder
{
    private const int MAX_PATHFINDING_ITERATIONS = 1000;

    private readonly RoadNetworkManager networkManager;

    public AStarPathfinder(RoadNetworkManager manager)
    {
        networkManager = manager;
    }

    public List<IWaypoint> FindPath(Vector3 start, Vector3 end)
    {
        IWaypoint startWaypoint = networkManager.GetNearestWaypoint(start);
        IWaypoint endWaypoint = networkManager.GetNearestWaypoint(end);

        if (startWaypoint == null || endWaypoint == null)
        {
            Debug.LogWarning("[A*] Pathfinding failed: Start or end waypoint is null");
            return new List<IWaypoint>();
        }

        if (startWaypoint == endWaypoint)
        {
            return new List<IWaypoint> { startWaypoint };
        }

        return ExecuteAStar(startWaypoint, endWaypoint);
    }

    public float CalculatePathCost(List<IWaypoint> path)
    {
        if (path == null || path.Count < 2)
        {
            return 0f;
        }

        float cost = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            cost += Vector3.Distance(path[i].Position, path[i + 1].Position);
        }

        return cost;
    }

    private List<IWaypoint> ExecuteAStar(IWaypoint start, IWaypoint goal)
    {
        var openQueue = new PathNodePriorityQueue(); // waypoints to explore, sorted by estimated total cost
        var closedSet = new HashSet<IWaypoint>(); // waypoints already evaluated
        var nodesByWaypoint = new Dictionary<IWaypoint, PathNode>(); // mapping of waypoints to their corresponding PathNode

        PathNode startNode = new PathNode(start, null, 0f, CalculateHeuristic(start, goal));
        nodesByWaypoint[start] = startNode;
        openQueue.Enqueue(startNode);

        int iterations = 0;

        while (openQueue.Count > 0 && iterations++ < MAX_PATHFINDING_ITERATIONS)
        {
            PathNode current = openQueue.Dequeue();
            if (current == null)
            {
                break;
            }

            if (closedSet.Contains(current.Waypoint))
            {
                continue;
            }

            if (current.Waypoint == goal)
            {
                return ReconstructPath(current);
            }

            closedSet.Add(current.Waypoint);

            foreach (IWaypoint neighbor in current.Waypoint.GetNeighbours())
            {
                if (neighbor == null || closedSet.Contains(neighbor))
                {
                    continue;
                }

                float tentativeG = current.GScore + Vector3.Distance(current.Waypoint.Position, neighbor.Position);

                if (!nodesByWaypoint.TryGetValue(neighbor, out PathNode neighborNode))
                {
                    neighborNode = new PathNode(neighbor, current, tentativeG, CalculateHeuristic(neighbor, goal));
                    nodesByWaypoint[neighbor] = neighborNode;
                }
                else if (tentativeG < neighborNode.GScore)
                {
                    neighborNode.Parent = current;
                    neighborNode.GScore = tentativeG;
                }

                openQueue.Enqueue(neighborNode);
            }
        }

        if (iterations >= MAX_PATHFINDING_ITERATIONS)
        {
            Debug.LogWarning($"[A*] Max iterations ({MAX_PATHFINDING_ITERATIONS}) reached");
        }
        else
        {
            Debug.LogWarning($"[A*] No path found from {start.Position} to {goal.Position} after {iterations} iterations");
        }

        return new List<IWaypoint>();
    }

    private static List<IWaypoint> ReconstructPath(PathNode goalNode)
    {
        var path = new List<IWaypoint>();
        PathNode current = goalNode;

        while (current != null)
        {
            path.Add(current.Waypoint);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }

    private static float CalculateHeuristic(IWaypoint from, IWaypoint to)
    {
        return Vector3.Distance(from.Position, to.Position);
    }
}