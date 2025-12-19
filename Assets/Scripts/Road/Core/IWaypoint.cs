using System.Collections.Generic;
using UnityEngine;

// Interface for navigation waypoints

public interface IWaypoint
{
    Vector3 Position { get; }
    IWaypoint NextWaypoint { get; set; }
    WaypointType Type { get; }
    IEnumerable<IWaypoint> GetNeighbours();
}

public enum WaypointType
{
    Normal,         // Regular waypoint
    Entry,          // Added for spawning and despawning
    Exit,           // Added for spawning and despawning
    Junction       // Junction / intersection waypoint
}