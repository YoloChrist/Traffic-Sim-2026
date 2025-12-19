using System.Collections.Generic;
using UnityEngine;

public class StopSignState : IIntersectionState
{
    private const float STOP_DURATION = 0.4f;
    private const float MAX_INTERSECTION_TIME = 10f;

    private readonly Queue<GameObject> vehicleQueue = new Queue<GameObject>();
    private readonly Dictionary<GameObject, float> stopTimers = new Dictionary<GameObject, float>();
    private readonly Dictionary<GameObject, float> intersectionTimers = new Dictionary<GameObject, float>();
    private readonly HashSet<GameObject> vehiclesInIntersection = new HashSet<GameObject>();
    private readonly HashSet<GameObject> vehiclesClearedToEnter = new HashSet<GameObject>();
    private readonly HashSet<GameObject> vehiclesPhysicallyInTrigger = new HashSet<GameObject>();

    public void OnEnter(Intersection intersection)
    {
        ClearAllVehicles();
        intersection.SetVisualColor(Color.yellow);
    }

    public void OnExit(Intersection intersection)
    {
        ClearAllVehicles();
    }

    public void Update(Intersection intersection)
    {
        if (stopTimers.Count == 0 && intersectionTimers.Count == 0)
            return;

        UpdateStopTimers();
        UpdateIntersectionTimers();
        CleanupStuckVehicles(intersection);
    }

    public bool CanPassThrough(Intersection intersection, Vector3 approachDirection)
    {
        return false;
    }

    public void OnVehicleStopped(Intersection intersection, GameObject vehicle)
    {
        if (vehicle == null || stopTimers.ContainsKey(vehicle))
            return;

        vehicleQueue.Enqueue(vehicle);
        stopTimers[vehicle] = STOP_DURATION;
    }

    public void OnVehicleEntering(GameObject vehicle)
    {
        if (vehicle == null)
            return;

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

        // Always clean up queue and timers when vehicle leaves
        if (stopTimers.ContainsKey(vehicle))
        {
            RemoveFromQueue(vehicle);
            stopTimers.Remove(vehicle);
        }
    }

    public bool CanVehicleLeave(Intersection intersection, GameObject vehicle)
    {
        if (vehicle == null || !stopTimers.ContainsKey(vehicle))
            return false;

        if (!IsFirstInQueue(vehicle))
            return false;

        if (!HasWaitedLongEnough(vehicle))
            return false;

        if (IsIntersectionBlocked(vehicle))
            return false;

        MarkVehicleAsCleared(vehicle);
        return true;
    }

    public string GetStateName() => "Stop Sign";

    private bool ShouldTrackVehicle(GameObject vehicle)
    {
        return vehiclesClearedToEnter.Contains(vehicle) || stopTimers.ContainsKey(vehicle);
    }

    private bool IsFirstInQueue(GameObject vehicle)
    {
        return vehicleQueue.Count > 0 && vehicleQueue.Peek() == vehicle;
    }

    private bool HasWaitedLongEnough(GameObject vehicle)
    {
        return stopTimers.TryGetValue(vehicle, out float timeRemaining) && timeRemaining <= 0.0001f;
    }

    private bool IsIntersectionBlocked(GameObject vehicle)
    {
        foreach (GameObject v in vehiclesInIntersection)
        {
            if (v != null && v != vehicle)
                return true;
        }
        return false;
    }

    private void MarkVehicleAsCleared(GameObject vehicle)
    {
        if (vehiclesClearedToEnter.Contains(vehicle))
            return;

        vehiclesClearedToEnter.Add(vehicle);

        if (vehiclesPhysicallyInTrigger.Contains(vehicle) && !vehiclesInIntersection.Contains(vehicle))
        {
            vehiclesInIntersection.Add(vehicle);
            intersectionTimers[vehicle] = 0f;
        }
    }

    private void RemoveFromQueue(GameObject vehicle)
    {
        if (vehicleQueue.Count == 0)
            return;

        if (vehicleQueue.Peek() == vehicle)
        {
            vehicleQueue.Dequeue();
            return;
        }

        // Vehicle is not at front - rebuild queue without it
        Queue<GameObject> newQueue = new Queue<GameObject>();
        foreach (GameObject v in vehicleQueue)
        {
            if (v != vehicle && v != null)
            {
                newQueue.Enqueue(v);
            }
        }

        vehicleQueue.Clear();
        foreach (GameObject v in newQueue)
        {
            vehicleQueue.Enqueue(v);
        }
    }

    private void UpdateStopTimers()
    {
        List<GameObject> nullVehicles = new List<GameObject>();
        List<GameObject> vehicles = new List<GameObject>(stopTimers.Keys);

        foreach (GameObject vehicle in vehicles)
        {
            if (vehicle == null)
            {
                nullVehicles.Add(vehicle);
                continue;
            }

            stopTimers[vehicle] -= Time.deltaTime;
        }

        if (nullVehicles.Count > 0)
        {
            CleanupNullVehicles(nullVehicles);
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

    private void CleanupNullVehicles(List<GameObject> nullVehicles)
    {
        foreach (GameObject vehicle in nullVehicles)
        {
            vehiclesPhysicallyInTrigger.Remove(vehicle);
            vehiclesInIntersection.Remove(vehicle);
            vehiclesClearedToEnter.Remove(vehicle);
            intersectionTimers.Remove(vehicle);
            stopTimers.Remove(vehicle);
            RemoveFromQueue(vehicle);
        }
    }

    private void ClearAllVehicles()
    {
        vehicleQueue.Clear();
        stopTimers.Clear();
        intersectionTimers.Clear();
        vehiclesInIntersection.Clear();
        vehiclesClearedToEnter.Clear();
        vehiclesPhysicallyInTrigger.Clear();
    }
}
