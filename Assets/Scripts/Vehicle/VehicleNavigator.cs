using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Handles waypoint and destination navigation for vehicles
public class VehicleNavigator
{
    private readonly NavMeshAgent navMeshAgent;
    private readonly Transform transform;

    private List<IWaypoint> currentPath;
    private int currentPathIndex;
    private IWaypoint currentWaypoint;

    private bool isFollowingPath;
    private bool hasDestination;
    private float waypointReachDistance;

    public event Action OnDestinationReached;
    public event Action<WaypointType> OnWaypointReached;

    public IWaypoint CurrentWaypoint => currentWaypoint;
    public bool IsNavigating => isFollowingPath || hasDestination;

    public VehicleNavigator(NavMeshAgent agent, Transform vehicleTransform)
    {
        navMeshAgent = agent;
        transform = vehicleTransform;
    }

    public void Configure(float waypointReachDistance)
    {
        this.waypointReachDistance = waypointReachDistance;
    }

    public void Update()
    {
        if (isFollowingPath)
        {
            UpdatePathNavigation();
        }
        else if (hasDestination)
        {
            UpdateDestinationNavigation();
        }
    }

    public void SetPath(List<IWaypoint> path)
    {
        if (path == null || path.Count == 0)
        {
            return;
        }

        currentPath = path;
        currentPathIndex = 0;
        isFollowingPath = true;
        hasDestination = false;

        SetAgentDestinationToCurrentPathWaypoint();
    }

    public void SetDestination(Vector3 target)
    {
        navMeshAgent.SetDestination(target);
        hasDestination = true;
        isFollowingPath = false;
    }

    public void SetWaypoint(IWaypoint waypoint)
    {
        if (waypoint == null)
        {
            return;
        }

        currentWaypoint = waypoint;
        isFollowingPath = true;
        hasDestination = false;

        navMeshAgent.SetDestination(currentWaypoint.Position);
    }

    public void Reset()
    {
        currentPath = null;
        currentPathIndex = 0;
        currentWaypoint = null;
        isFollowingPath = false;
        hasDestination = false;

        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.isStopped = false;
        }
    }

    private void UpdatePathNavigation()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            isFollowingPath = false;
            return;
        }

        IWaypoint targetWaypoint = currentPath[currentPathIndex];

        if (HasReachedWaypoint(targetWaypoint))
        {
            HandleWaypointReached(targetWaypoint);
        }
    }

    private void UpdateDestinationNavigation()
    {
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
        {
            ReachedDestination();
        }
    }

    private bool HasReachedWaypoint(IWaypoint waypoint)
    {
        float distanceToWaypoint = Vector3.Distance(transform.position, waypoint.Position);
        return distanceToWaypoint <= waypointReachDistance;
    }

    private void HandleWaypointReached(IWaypoint waypoint)
    {
        OnWaypointReached?.Invoke(waypoint.Type);

        if (waypoint.Type == WaypointType.Exit)
        {
            return;
        }

        currentPathIndex++;

        if (currentPathIndex >= currentPath.Count)
        {
            ReachedDestination();
            return;
        }

        SetAgentDestinationToCurrentPathWaypoint();
    }

    private void SetAgentDestinationToCurrentPathWaypoint()
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            return;
        }

        IWaypoint targetWaypoint = currentPath[currentPathIndex];
        navMeshAgent.SetDestination(targetWaypoint.Position);
    }

    private void ReachedDestination()
    {
        hasDestination = false;
        isFollowingPath = false;
        currentPath = null;
        currentPathIndex = 0;

        OnDestinationReached?.Invoke();
    }
}