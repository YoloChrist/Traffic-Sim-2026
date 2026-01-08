using System.Collections.Generic;
using UnityEngine;

public class TrafficLightState : IIntersectionState
{
    private enum LightPhase { Red, Amber, Green }
    private enum Direction { North, South, East, West }

    private const float GREEN_DURATION = 8f;
    private const float AMBER_DURATION = 2f;
    private const float RED_DURATION = 1f;
    private const float MAX_INTERSECTION_TIME = 15f;

    private Direction activeDirection;
    private LightPhase currentPhase;
    private float phaseTimer;

    private Dictionary<GameObject, Direction> vehicleDirections = new Dictionary<GameObject, Direction>();
    private Dictionary<GameObject, float> intersectionTimers = new Dictionary<GameObject, float>();
    private HashSet<GameObject> vehiclesInIntersection = new HashSet<GameObject>();
    private HashSet<GameObject> vehiclesClearedToEnter = new HashSet<GameObject>();
    private HashSet<GameObject> vehiclesPhysicallyInTrigger = new HashSet<GameObject>();
    private HashSet<Direction> directionsWithVehicles = new HashSet<Direction>();

    public void OnEnter(Intersection intersection)
    {
        // Preserve existing tracking to avoid blocking vehicles already in the box
        activeDirection = Direction.North;
        currentPhase = LightPhase.Green;
        phaseTimer = GREEN_DURATION;
    }

    public void OnExit(Intersection intersection)
    {
        ClearAllVehicles();
    }

    public void Update(Intersection intersection)
    {
        phaseTimer -= Time.deltaTime;

        if (phaseTimer <= 0f)
        {
            AdvancePhase();
        }

        UpdateIntersectionTimers();
        CleanupStuckVehicles(intersection);
    }

    public bool CanPassThrough(Intersection intersection, Vector3 approachDirection)
    {
        return false;
    }

    public void OnVehicleStopped(Intersection intersection, GameObject vehicle)
    {
        if (vehicle == null || vehicleDirections.ContainsKey(vehicle))
        {
            return;
        }

        Direction dir = GetVehicleDirection(intersection, vehicle);
        vehicleDirections[vehicle] = dir;
        directionsWithVehicles.Add(dir);
    }

    public void OnVehicleEntering(GameObject vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        vehiclesPhysicallyInTrigger.Add(vehicle);

        if (ShouldTrackVehicle(vehicle) && !vehiclesInIntersection.Contains(vehicle))
        {
            vehiclesInIntersection.Add(vehicle);
            intersectionTimers[vehicle] = 0f;
        }
    }

    public void OnVehicleLeaving(Intersection intersection, GameObject vehicle)
    {
        vehiclesPhysicallyInTrigger.Remove(vehicle);
        vehiclesInIntersection.Remove(vehicle);
        intersectionTimers.Remove(vehicle);
        vehiclesClearedToEnter.Remove(vehicle);

        if (vehicleDirections.ContainsKey(vehicle))
        {
            Direction dir = vehicleDirections[vehicle];
            vehicleDirections.Remove(vehicle);
            UpdateDirectionTracking(dir);
        }
    }

    public bool CanVehicleLeave(Intersection intersection, GameObject vehicle)
    {
        if (vehicle == null)
        {
            return false;
        }

        if (!vehicleDirections.TryGetValue(vehicle, out Direction vehicleDir))
        {
            vehicleDir = GetVehicleDirection(intersection, vehicle);
            vehicleDirections[vehicle] = vehicleDir;
            directionsWithVehicles.Add(vehicleDir);
        }

        if (IsVehicleAlreadyInside(vehicle))
        {
            MarkVehicleAsCleared(vehicle);
            return true;
        }

        if (activeDirection != vehicleDir)
        {
            return false;
        }

        if (currentPhase != LightPhase.Green)
        {
            return false;
        }

        if (IsIntersectionBlockedByOtherDirection(vehicle, vehicleDir))
        {
            return false;
        }

        MarkVehicleAsCleared(vehicle);
        return true;
    }

    public string GetStateName()
    {
        return $"Traffic Light ({activeDirection} - {currentPhase})";
    }

    private Direction GetVehicleDirection(Intersection intersection, GameObject vehicle)
    {
        Vector3 toVehicle = vehicle.transform.position - intersection.transform.position;
        toVehicle.y = 0f;

        float angle = Vector3.SignedAngle(Vector3.forward, toVehicle, Vector3.up);

        if (angle >= -45f && angle < 45f)
        {
            return Direction.North;
        }
        if (angle >= 45f && angle < 135f)
        {
            return Direction.East;
        }
        if (angle >= -135f && angle < -45f)
        {
            return Direction.West;
        }

        return Direction.South;
    }

    private void AdvancePhase()
    {
        switch (currentPhase)
        {
            case LightPhase.Green:
                currentPhase = LightPhase.Amber;
                phaseTimer = AMBER_DURATION;
                break;
            case LightPhase.Amber:
                currentPhase = LightPhase.Red;
                phaseTimer = RED_DURATION;
                break;
            case LightPhase.Red:
                activeDirection = GetNextDirection();
                currentPhase = LightPhase.Green;
                phaseTimer = GREEN_DURATION;
                break;
        }
    }

    private Direction GetNextDirection()
    {
        Direction[] directions = { Direction.North, Direction.East, Direction.South, Direction.West };
        int currentIndex = System.Array.IndexOf(directions, activeDirection);

        for (int i = 1; i <= 4; i++)
        {
            Direction nextDir = directions[(currentIndex + i) % 4];
            if (directionsWithVehicles.Contains(nextDir))
            {
                return nextDir;
            }
        }

        return directions[(currentIndex + 1) % 4];
    }

    private bool IsIntersectionBlockedByOtherDirection(GameObject vehicle, Direction vehicleDir)
    {
        foreach (GameObject v in vehiclesInIntersection)
        {
            if (v != null && v != vehicle && vehicleDirections.TryGetValue(v, out Direction otherDir) && otherDir != vehicleDir)
            {
                return true;
            }
        }
        return false;
    }

    private bool ShouldTrackVehicle(GameObject vehicle)
    {
        return vehiclesClearedToEnter.Contains(vehicle) || vehicleDirections.ContainsKey(vehicle);
    }

    private void MarkVehicleAsCleared(GameObject vehicle)
    {
        if (vehiclesClearedToEnter.Contains(vehicle))
        {
            return;
        }

        vehiclesClearedToEnter.Add(vehicle);

        if (vehiclesPhysicallyInTrigger.Contains(vehicle) && !vehiclesInIntersection.Contains(vehicle))
        {
            vehiclesInIntersection.Add(vehicle);
            intersectionTimers[vehicle] = 0f;
        }
    }

    private void UpdateDirectionTracking(Direction dir)
    {
        bool hasVehiclesInDirection = false;
        foreach (var kvp in vehicleDirections)
        {
            if (kvp.Value == dir && kvp.Key != null)
            {
                hasVehiclesInDirection = true;
                break;
            }
        }

        if (!hasVehiclesInDirection)
        {
            directionsWithVehicles.Remove(dir);
        }
    }

    private void UpdateIntersectionTimers()
    {
        List<GameObject> vehicles = new List<GameObject>(intersectionTimers.Keys);

        foreach (GameObject vehicle in vehicles)
        {
            if (vehicle != null)
            {
                intersectionTimers[vehicle] += Time.deltaTime;
            }
        }
    }

    private void CleanupStuckVehicles(Intersection intersection)
    {
        List<GameObject> stuckVehicles = new List<GameObject>();

        foreach (var kvp in intersectionTimers)
        {
            if (kvp.Value > MAX_INTERSECTION_TIME)
            {
                stuckVehicles.Add(kvp.Key);
            }
        }

        foreach (GameObject vehicle in stuckVehicles)
        {
            OnVehicleLeaving(intersection, vehicle);
        }
    }

    private void ClearAllVehicles()
    {
        vehicleDirections.Clear();
        intersectionTimers.Clear();
        vehiclesInIntersection.Clear();
        vehiclesClearedToEnter.Clear();
        vehiclesPhysicallyInTrigger.Clear();
        directionsWithVehicles.Clear();
    }

    private bool IsVehicleAlreadyInside(GameObject vehicle)
    {
        return vehiclesPhysicallyInTrigger.Contains(vehicle) || vehiclesInIntersection.Contains(vehicle);
    }
}