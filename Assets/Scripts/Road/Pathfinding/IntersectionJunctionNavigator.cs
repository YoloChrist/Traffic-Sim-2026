using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// handles navigation through intersections
public class IntersectionJunctionNavigator : IJunctionNavigator
{
    private const float INTERSECTION_SEARCH_RADIUS = 110f;
    private const float WAYPOINT_NEAR_INTERSECTION_THRESHOLD = 75f;

    private readonly RoadNetworkManager networkManager;

    public IntersectionJunctionNavigator(RoadNetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }

    public IEnumerable<IWaypoint> GetExitWaypoints(IWaypoint junctionWaypoint)
    {
        if (junctionWaypoint == null)
        {
            Debug.LogWarning("[JunctionNav] Junction waypoint is null");
            return new List<IWaypoint>();
        }

        Intersection nearestIntersection = FindNearestIntersection(junctionWaypoint.Position);
        if (nearestIntersection == null)
        {
            Debug.LogWarning($"[JunctionNav] No intersection found near junction at {junctionWaypoint.Position} within {INTERSECTION_SEARCH_RADIUS}m");
            return new List<IWaypoint>();
        }

        IRoadElement currentRoad = networkManager.GetRoadElementAtPosition(junctionWaypoint.Position, INTERSECTION_SEARCH_RADIUS);

        var connections = nearestIntersection.GetConnections()?.ToList() ?? new List<IRoadElement>();
        if (connections.Count == 0)
        {
            Debug.LogWarning($"[JunctionNav] Intersection at {nearestIntersection.Position} has no connections!");
            return new List<IWaypoint>();
        }

        var exitWaypoints = new List<IWaypoint>();
        foreach (IRoadElement connection in connections)
        {
            if (connection == null || (currentRoad != null && connection == currentRoad))
            {
                continue;
            }

            if (connection is INavigable navigable)
            {
                exitWaypoints.AddRange(FindNearbyWaypoints(navigable, nearestIntersection));
            }
        }

        if (exitWaypoints.Count == 0)
        {
            Debug.LogWarning(
                $"[JunctionNav] Junction at {junctionWaypoint.Position}: " +
                $"Found {connections.Count} connections but no waypoints within {WAYPOINT_NEAR_INTERSECTION_THRESHOLD}m of intersection at {nearestIntersection.Position}");
        }

        return exitWaypoints;
    }

    private Intersection FindNearestIntersection(Vector3 position)
    {
        Intersection nearest = null;
        float minDistance = float.MaxValue;

        foreach (var intersection in networkManager.GetAllIntersections())
        {
            if (intersection == null)
            {
                continue;
            }

            float distance = Vector3.Distance(position, intersection.Position);
            if (distance <= INTERSECTION_SEARCH_RADIUS && distance < minDistance)
            {
                minDistance = distance;
                nearest = intersection;
            }
        }

        return nearest;
    }

    private List<IWaypoint> FindNearbyWaypoints(INavigable road, Intersection intersection)
    {
        var nearbyWaypoints = new List<IWaypoint>();
        if (road == null || intersection == null)
        {
            return nearbyWaypoints;
        }

        foreach (Lane lane in road.GetLanes())
        {
            if (lane == null)
            {
                continue;
            }

            IWaypoint closest = null;
            float closestDist = float.MaxValue;

            foreach (IWaypoint waypoint in lane.GetWaypoints())
            {
                if (waypoint == null || waypoint.Type == WaypointType.Junction)
                {
                    continue;
                }

                float dist = Vector3.Distance(waypoint.Position, intersection.Position);
                if (dist <= WAYPOINT_NEAR_INTERSECTION_THRESHOLD && dist < closestDist)
                {
                    closest = waypoint;
                    closestDist = dist;
                }
            }

            if (closest != null)
            {
                nearbyWaypoints.Add(closest);
            }
        }

        return nearbyWaypoints;
    }
}