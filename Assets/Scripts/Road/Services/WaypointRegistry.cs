using System.Collections.Generic;
using UnityEngine;

// Waypoint caching and queries (separated out from RoadNetworkManager for single responsibility)

public class WaypointRegistry
{
    private List<IWaypoint> allWaypoints;
    private Dictionary<WaypointType, List<IWaypoint>> waypointsByType;
    private List<RoadSegment> roadSegments;

    public WaypointRegistry(List<RoadSegment> roads)
    {
        roadSegments = roads;
        allWaypoints = new List<IWaypoint>();
        waypointsByType = new Dictionary<WaypointType, List<IWaypoint>>();

        CacheAllWaypoints();
    }

    private void CacheAllWaypoints()
    {
        allWaypoints.Clear();
        waypointsByType.Clear();

        foreach (var road in roadSegments)
        {
            if (road is INavigable navigable)
            {
                var waypoints = navigable.GetAllWaypoints();
                foreach (var waypoint in waypoints)
                {
                        if (!allWaypoints.Contains(waypoint))
                        {
                            allWaypoints.Add(waypoint);

                            // by type
                            if (!waypointsByType.ContainsKey(waypoint.Type))
                            {
                                waypointsByType[waypoint.Type] = new List<IWaypoint>();
                            }
                            waypointsByType[waypoint.Type].Add(waypoint);
                        } 
                }
            }
        }
    }

    public void RefreshCache()
    {
        CacheAllWaypoints();
    }

    public List<IWaypoint> GetWaypointsByType(WaypointType type)
    {
        return waypointsByType.TryGetValue(type, out var waypoints) ? waypoints : new List<IWaypoint>();
    }

    public IWaypoint GetRandomWaypointOfType(WaypointType type)
    {
        var waypoints = GetWaypointsByType(type);

        return waypoints.Count > 0 ? waypoints[UnityEngine.Random.Range(0, waypoints.Count)] : null;
    }

    public IWaypoint GetNearestWaypoint(Vector3 position, float maxDistance = 2000f)
    {
        float closestDistance = float.MaxValue;
        IWaypoint closestWaypoint = null;

        foreach (var waypoint in allWaypoints)
        {
            float distance = Vector3.Distance(position, waypoint.Position);
            if (distance < closestDistance && distance <= maxDistance)
            {
                closestDistance = distance;
                closestWaypoint = waypoint;
            }
        }

        if (closestWaypoint == null)
            Debug.LogWarning($"No waypoint found near {position} within {maxDistance}m");

        return closestWaypoint;
    }

    public List<IWaypoint> GetAllWaypoints()
    {
        return allWaypoints;
    }
}