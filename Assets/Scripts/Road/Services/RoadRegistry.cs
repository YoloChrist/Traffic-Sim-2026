using System.Collections.Generic;
using UnityEngine;

// road and intersection registration (separated out from RoadNetworkManager for single responsibility)
public class RoadRegistry
{
    private List<RoadSegment> roadSegments;
    private List<Intersection> intersections;

    private const float CONNECTION_THRESHOLD = 10f; // distance to auto connect connections
    private const float LANE_END_CHECK_DISTANCE = 60f;

    public RoadRegistry(List<RoadSegment> roads, List<Intersection> intersections)
    {
        this.roadSegments = roads ?? new List<RoadSegment>();
        this.intersections = intersections ?? new List<Intersection>();

        ValidateNetwork();
    }

    private void ValidateNetwork()
    {
        if (roadSegments.Count == 0)
        {
            Debug.LogError("RoadRegistry initialized with no road segments.");
        }
        if (intersections.Count == 0)
        {
            Debug.LogWarning("RoadRegistry initialized with no intersections.");
        }
    }

    public void BuildConnections()
    {
        AutoDetectConnections();
        ValidateNetwork();
    }

    public void AutoDetectConnections()
    {
        Debug.Log("[RoadRegistry] Auto-detecting connections between road segments and intersections...");
        int connectionsBuilt = 0;

        foreach (var road in roadSegments)
        {
            var lanes = road.GetLanes();
            
            if (lanes == null)
            {
                Debug.LogWarning($"[RoadRegistry] Road at {road.Position} has no lanes.");
                continue;
            }

            foreach (var intersection in intersections)
            {
                bool shouldConnect = false;

                // Check center to center distance
                float centerDistance = Vector3.Distance(intersection.Position, road.Position);
                
                if (centerDistance < CONNECTION_THRESHOLD)
                {
                    shouldConnect = true;
                }

                if (!shouldConnect)
                {
                    // Check lane endpoints
                    foreach (var lane in lanes)
                    {
                        var entryWaypoint = lane.GetEntryWaypoint();
                        var exitWaypoint = lane.GetExitWaypoint();

                        if (entryWaypoint != null)
                        {
                            float entryDistance = Vector3.Distance(intersection.Position, entryWaypoint.Position);
                            
                            if (entryDistance < LANE_END_CHECK_DISTANCE)
                            {
                                shouldConnect = true;
                                break;
                            }
                        }

                        if (exitWaypoint != null)
                        {
                            float exitDistance = Vector3.Distance(intersection.Position, exitWaypoint.Position);
                            
                            if (exitDistance < LANE_END_CHECK_DISTANCE)
                            {
                                shouldConnect = true;
                                break;
                            }
                        }
                    }
                }
                
                if (shouldConnect)
                {
                    if (!intersection.IsConnectedTo(road))
                    {
                        intersection.AddConnection(road);
                        connectionsBuilt++;
                    }
                    if (!road.IsConnectedTo(intersection))
                    {
                        road.AddConnection(intersection);
                        connectionsBuilt++;
                    }
                }
            }
        }
        
        Debug.Log($"[RoadRegistry] Auto-detection complete. Total connections built: {connectionsBuilt}");
    }

    public List<RoadSegment> GetAllRoads() => roadSegments;
    public List<Intersection> GetAllIntersections() => intersections;

    // find road element at position
    public IRoadElement GetRoadElementAtPosition(Vector3 position, float searchRadius = 10f)
    {
        // check roads
        foreach (var road in roadSegments)
        {
            if (Vector3.Distance(position, road.Position) < searchRadius)
                return road;
        }

        // check intersections
        foreach (var intersection in intersections)
        {
            if (Vector3.Distance(position, intersection.Position) < searchRadius)
                return intersection;
        }

        return null;
    }
}
