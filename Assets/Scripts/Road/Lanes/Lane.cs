using System.Collections.Generic;
using UnityEngine;

// Single traffic lane with waypoints

public class Lane : MonoBehaviour, ILaneStrategy
{
    [Header("Lane Configuration")]
    [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();

    private void OnValidate()
    {
        LinkWaypoints();
    }

    private void Awake()
    {
        LinkWaypoints();
    }

    public IEnumerable<IWaypoint> GetWaypoints()
    {
        return waypoints;
    }

    public bool CanVehicleEnter(Vehicle vehicle)
    {
        return true;
    }


    public Waypoint GetEntryWaypoint()
    {
        return waypoints.Count > 0 ? waypoints[0] : null;
    }

    public Waypoint GetExitWaypoint()
    {
        return waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : null;
    }

    private void LinkWaypoints()
    {
        // Link waypoints within this lane
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                waypoints[i].SetNext(waypoints[i + 1]);
        }

        // Connect exit waypoint to entry waypoint of next lane segment
        if (waypoints.Count > 0)
        {
            Waypoint exitWaypoint = GetExitWaypoint();
            if (exitWaypoint != null)
            {
                // Find another lane's entry waypoint at the same position
                Lane[] allLanes = FindObjectsByType<Lane>(FindObjectsSortMode.None);
                foreach (Lane otherLane in allLanes)
                {
                    if (otherLane == this) continue;

                    Waypoint otherEntry = otherLane.GetEntryWaypoint();
                    if (otherEntry != null &&
                        Vector3.Distance(exitWaypoint.transform.position, otherEntry.transform.position) < 0.5f)
                    {
                        exitWaypoint.SetNext(otherEntry);
                        break;
                    }
                }
            }
        }
    }
}