using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Vehicle : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float angularSpeed = 240f;

    [Header("Detection Settings")]
    [SerializeField] private float lookAheadDistance = 3f;
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private LayerMask vehicleLayer;
    [SerializeField] private LayerMask intersectionLayer;

    [Header("Waypoint Navigation")]
    [SerializeField] private float waypointReachDistance = 5f;

    private NavMeshAgent navMeshAgent;
    private VehicleAI vehicleAI;
    private VehicleNavigator navigator;
    private VehicleConfig vehicleConfig;

    public Transform Transform => transform;
    public Vector3 Position => transform.position;
    public Vector3 Forward => transform.forward;
    public IWaypoint CurrentWaypoint => navigator?.CurrentWaypoint;
    public VehicleAI AI => vehicleAI;

    public float MaxSpeed
    {
        get => maxSpeed;
        set => SetMaxSpeed(value);
    }

    public float Acceleration
    {
        get => acceleration;
        set => SetAcceleration(value);
    }

    public float Deceleration
    {
        get => deceleration;
        set => SetDeceleration(value);
    }

    public float AngularSpeed
    {
        get => angularSpeed;
        set => SetAngularSpeed(value);
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        InitializeComponents();
    }

    private void Update()
    {
        navigator.Update();
        vehicleAI.Update();
    }

    private void OnDestroy()
    {
        TrafficManager.Instance?.OnVehicleDestroyed(this);
    }

    private void InitializeComponents()
    {
        ConfigureNavMeshAgent();

        navigator = new VehicleNavigator(navMeshAgent, transform);
        navigator.Configure(waypointReachDistance);
        navigator.OnWaypointReached += HandleWaypointReached;

        vehicleAI = new VehicleAI(this, navMeshAgent);
        ConfigureAI();
        vehicleAI.ChangeState(new MovingState());
    }

    private void ConfigureNavMeshAgent()
    {
        navMeshAgent.speed = maxSpeed;
        navMeshAgent.acceleration = acceleration;
        navMeshAgent.angularSpeed = angularSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.updateRotation = true;
        navMeshAgent.autoBraking = false;
    }

    private void ConfigureAI()
    {
        vehicleAI.MaxSpeed = maxSpeed;
        vehicleAI.Acceleration = acceleration;
        vehicleAI.Deceleration = deceleration;
        vehicleAI.LookAheadDistance = lookAheadDistance;
        vehicleAI.StoppingDistance = stoppingDistance;
        vehicleAI.VehicleLayer = vehicleLayer;
        vehicleAI.IntersectionLayer = intersectionLayer;
    }

    private void HandleWaypointReached(WaypointType waypointType)
    {
        if (waypointType == WaypointType.Exit)
        {
            TrafficManager.Instance?.OnVehicleDespawned(this);
        }
    }

    public void SetDestinationToExit(IWaypoint exitWaypoint)
    {
        if (exitWaypoint == null)
        {
            Debug.LogError("[Vehicle] Exit waypoint is null");
            return;
        }

        IWaypoint startWaypoint = RoadNetworkManager.Instance.GetNearestWaypoint(transform.position);
        if (startWaypoint == null)
        {
            Debug.LogError("[Vehicle] No start waypoint found near vehicle position");
            return;
        }

        List<IWaypoint> path = RoadNetworkManager.Instance.FindPath(startWaypoint.Position, exitWaypoint.Position);
        if (path != null && path.Count > 0)
        {
            navigator.SetPath(path);
            vehicleAI.ChangeState(new MovingState());
        }
        else
        {
            Debug.LogWarning($"[Vehicle] No path found to exit waypoint at {exitWaypoint.Position}");
        }
    }

    public void SetDestinationUsingRoadNetwork(Vector3 target)
    {
        var path = RoadNetworkManager.Instance.FindPath(transform.position, target);
        if (path != null && path.Count > 0)
        {
            navigator.SetPath(path);
            vehicleAI.ChangeState(new MovingState());
        }
        else
        {
            SetDestination(target);
        }
    }

    public void SetWaypoint(IWaypoint startWaypoint)
    {
        if (startWaypoint == null)
            return;

        navigator.SetWaypoint(startWaypoint);
        vehicleAI.ChangeState(new MovingState());
    }

    public void SetDestination(Vector3 target)
    {
        navigator.SetDestination(target);
        vehicleAI.ChangeState(new MovingState());
    }

    public void ResetForReuse()
    {
        navigator.Reset();
        vehicleAI.Reset();
    }

    public void SetVehicleConfig(VehicleConfig config)
    {
        vehicleConfig = config;
    }

    public VehicleConfig GetVehicleConfig()
    {
        return vehicleConfig;
    }

    private void SetMaxSpeed(float speed)
    {
        maxSpeed = speed;
        if (navMeshAgent != null)
            navMeshAgent.speed = speed;
        if (vehicleAI != null)
            vehicleAI.MaxSpeed = speed;
    }

    private void SetAcceleration(float accel)
    {
        acceleration = accel;
        if (navMeshAgent != null)
            navMeshAgent.acceleration = accel;
        if (vehicleAI != null)
            vehicleAI.Acceleration = accel;
    }

    private void SetDeceleration(float decel)
    {
        deceleration = decel;
        if (vehicleAI != null)
            vehicleAI.Deceleration = decel;
    }

    private void SetAngularSpeed(float value)
    {
        angularSpeed = value;
        if (navMeshAgent != null)
            navMeshAgent.angularSpeed = value;
    }
}