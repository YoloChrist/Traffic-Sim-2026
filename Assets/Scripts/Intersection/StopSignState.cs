using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StopSignState : IIntersectionState
{
    private Queue<GameObject> vehicleQueue = new Queue<GameObject>();
    private Dictionary<GameObject, float> stopTimers = new Dictionary<GameObject, float>();

    private const float STOP_DURATION = 2f;
    private const float CHECK_RADIUS = 3f;

    public void OnEnter(Intersection intersection)
    {
        vehicleQueue.Clear();
        stopTimers.Clear();
        intersection.SetVisualColor(Color.yellow);
    }

    public void OnExit(Intersection intersection)
    {
        vehicleQueue.Clear();
        stopTimers.Clear();
    }

    public void Update(Intersection intersection)
    {
        // Update timers for stopped vehicles
        var vehiclesToRemove = new List<GameObject>();

        foreach (var kvp in stopTimers.ToList())
        {
            if (kvp.Key == null || Vector3.Distance(kvp.Key.transform.position, intersection.transform.position) > CHECK_RADIUS)
            {
                vehiclesToRemove.Add(kvp.Key);
            }
            else
            {
                stopTimers[kvp.Key] = kvp.Value - Time.deltaTime;
            }
        }

        foreach (var vehicle in vehiclesToRemove)
        {
            stopTimers.Remove(vehicle);
        }
    }

    public bool CanPassThrough(Intersection intersection, Vector3 approachDirection)
    {
        // Simplified. Should call a RegisterStop() method
        // Allow passage if not too many vehicles are waiting
        return vehicleQueue.Count < 2;
    }

    public void RegisterStop(GameObject vehicle)
    {
        if (!stopTimers.ContainsKey(vehicle))
        {
            vehicleQueue.Enqueue(vehicle);
            stopTimers[vehicle] = STOP_DURATION;
        }
    }

    public bool HasVehicleWaitedEnough(GameObject vehicle)
    {
        if (stopTimers.TryGetValue(vehicle, out float timeStopped))
        {
            return timeStopped <= 0;
        }
        return false;
    }

    public string GetStateName()
    {
        return "Stop Sign";
    }
}
