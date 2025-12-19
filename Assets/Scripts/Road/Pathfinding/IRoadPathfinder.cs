using System.Collections.Generic;
using UnityEngine;

// Interface for road pathfinding algos
// Strategy pattern

public interface IRoadPathfinder
{
    List<IWaypoint> FindPath(Vector3 start, Vector3 end);
    float CalculatePathCost(List<IWaypoint> path);
}