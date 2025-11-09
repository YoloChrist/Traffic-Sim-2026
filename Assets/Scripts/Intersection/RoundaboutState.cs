using System.Collections.Generic;
using UnityEngine;

public class RoundaboutState : IIntersectionState
{
    // Track vehicles currently in the roundabout
    private HashSet<Transform> vehiclesInRoundabout = new HashSet<Transform>();

    private const float ROUNDABOUT_RADIUS = 5f;


    public void OnEnter(Intersection intersection)
    {
        vehiclesInRoundabout.Clear();
        intersection.SetVisualColor(Color.blue);
    }

    public void OnExit(Intersection intersection)
    {
        vehiclesInRoundabout.Clear();
    }

    public void Update(Intersection intersection)
    {
        // Check for vehicles in roundabout area
        // Simplified version - use triggers or something
        Collider[] colliders = Physics.OverlapSphere(
            intersection.transform.position,
            ROUNDABOUT_RADIUS
        );

        // Update tracking set
        vehiclesInRoundabout.Clear();
        foreach (var col in colliders)
            if (col.CompareTag("Vehicle"))
                vehiclesInRoundabout.Add(col.transform);
    }

    public bool CanPassThrough(Intersection intersection, Vector3 approachDirection)
    {
        // Give way to vehicles already in the roundabout
        // Simplified version - check if vehicles on left
        return vehiclesInRoundabout.Count == 0;
    }

    public string GetStateName()
    {
        return "Roundabout";
    }
}
