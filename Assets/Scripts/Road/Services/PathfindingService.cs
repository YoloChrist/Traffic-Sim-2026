using System;
using System.Collections.Generic;
using UnityEngine;

// Wrapper for the pathfinding algorithms
public class PathfindingService
{
    private readonly IRoadPathfinder pathfinder;

    public PathfindingService(IRoadPathfinder pathfinderImplementation)
    {
        pathfinder = pathfinderImplementation ?? throw new ArgumentNullException(nameof(pathfinderImplementation));
    }

    // Find path between two points
    public List<IWaypoint> FindPath(Vector3 start, Vector3 end)
    {
        var path = pathfinder.FindPath(start, end);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"PathfindingService: No path found from {start} to {end}.");
            return new List<IWaypoint>();
        }

        return path;
    }

    // Calculate path cost
    public float CalculatePathCost(List<IWaypoint> path)
    {
        return pathfinder.CalculatePathCost(path);
    }
}