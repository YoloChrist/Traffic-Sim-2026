using System.Collections.Generic;
using UnityEngine;

// Concrete waypoint implementation

public class Waypoint : MonoBehaviour, IWaypoint
{
    [Header("Waypoint Settings")]
    [SerializeField] private WaypointType waypointType = WaypointType.Normal;
    [SerializeField] private Waypoint next;

    // injected dependency for junction navigation
    private IJunctionNavigator junctionNavigator;

    public Vector3 Position => transform.position;
    public IWaypoint NextWaypoint { get => next; set => next = value as Waypoint; }
    public WaypointType Type => waypointType;

    public void SetJunctionNavigator(IJunctionNavigator navigator)
    {
        junctionNavigator = navigator;
    }

    public IEnumerable<IWaypoint> GetNeighbours()
    {
        var neighbors = new List<IWaypoint>();

        // normal waypoint: follow the next waypoint link
        if (next != null)
        {
            neighbors.Add(next);
        }

        // Junction waypoint: delegate to junction navigator
        if (waypointType == WaypointType.Junction && junctionNavigator != null)
        {
            var junctionNeighbors = junctionNavigator.GetExitWaypoints(this);
            neighbors.AddRange(junctionNeighbors);
        }

        return neighbors;
    }

    public void SetNext(Waypoint nextWaypoint)
    {
        next = nextWaypoint;
    }

    private void OnDrawGizmos()
    {
        if (next != null)
        {
            Gizmos.color = GetGizmosColor();
            Gizmos.DrawLine(Position, next.Position);
            Gizmos.DrawSphere(Position, 0.3f);
        }
    }

    private Color GetGizmosColor()
    {
        return waypointType switch
        {
            WaypointType.Entry => Color.green,
            WaypointType.Exit => Color.red,
            WaypointType.Junction => Color.yellow,
            _ => Color.cyan,
        };
    }
}