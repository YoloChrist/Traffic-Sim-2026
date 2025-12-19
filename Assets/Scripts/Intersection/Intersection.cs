using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour, IRoadElement, IConnectable
{
    [Header("Visual Settings")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color trafficLightColor = Color.red;
    [SerializeField] private Color roundaboutColor = Color.blue;
    [SerializeField] private Color stopSignColor = Color.yellow;

    [Header("Connections")]
    [SerializeField] private List<RoadElementBase> connectedRoads = new List<RoadElementBase>();

    private IIntersectionState currentState;

    private TrafficLightState trafficLightState;
    private RoundaboutState roundaboutState;
    private StopSignState stopSignState;

    public Vector2Int GridPosition { get; private set; }
    public IIntersectionState CurrentState { get => currentState; set => currentState = value; }
    
    public Vector3 Position => transform.position;

    private void Awake()
    {
        trafficLightState = new TrafficLightState();
        roundaboutState = new RoundaboutState();
        stopSignState = new StopSignState();

        SetState(stopSignState);

        GridPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
    }

    public void Initialize()
    {
        EstablishConnections();
    }

    public string GetElementType()
    {
        return "Intersection";
    }

    public IEnumerable<IRoadElement> GetConnections()
    {
        return connectedRoads;
    }

    public void AddConnection(IRoadElement element)
    {
        if (element is RoadElementBase road && !connectedRoads.Contains(road))
        {
            connectedRoads.Add(road);
        }
        else
        {
            Debug.LogWarning("[Intersection] Attempted to add a connection that is already present or invalid.");
        }
    }

    public void RemoveConnection(IRoadElement element)
    {
        if (element is RoadElementBase road)
            connectedRoads.Remove(road);
    }

    public bool IsConnectedTo(IRoadElement element)
    {
        return connectedRoads.Contains(element as RoadElementBase);
    }

    private void EstablishConnections()
    {
        foreach (var road in connectedRoads)
        {
            if (road != null && !road.IsConnectedTo(this))
                road.AddConnection(this);
        }
    }

    private void Update()
    {
        currentState?.Update(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        Vehicle vehicle = other.GetComponent<Vehicle>();
        if (vehicle != null)
        {
            OnVehicleEnter(vehicle);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Vehicle vehicle = other.GetComponent<Vehicle>();
        if (vehicle != null)
        {
            OnVehicleExit(vehicle);
        }
    }

    public void CycleToNextState()
    {
        if (currentState is TrafficLightState)
            SetState(roundaboutState);
        else if (currentState is RoundaboutState)
            SetState(stopSignState);
        else if (currentState is StopSignState)
            SetState(trafficLightState);
    }

    private void SetState(IIntersectionState newState)
    {
        currentState?.OnExit(this);
        currentState = newState;
        currentState.OnEnter(this);

        if (newState is RoundaboutState)
        {
            RoadNetworkManager.Instance.RefreshWaypointCache();
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (meshRenderer == null)
            Debug.LogWarning("MeshRenderer not assigned on Intersection.");

        Material material = meshRenderer.material;

        if (currentState is TrafficLightState)
            material.color = trafficLightColor;
        else if (currentState is RoundaboutState)
            material.color = roundaboutColor;
        else if (currentState is StopSignState)
            material.color = stopSignColor;
    }

    public void SetVisualColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }

    public void OnVehicleEnter(Vehicle vehicle)
    {
        if (currentState is StopSignState stopSign)
        {
            stopSign.OnVehicleEntering(vehicle.gameObject);
        }
        else if (currentState is TrafficLightState trafficLight)
        {
            trafficLight.OnVehicleEntering(vehicle.gameObject);
        }
    }

    public void OnVehicleExit(Vehicle vehicle)
    {
        currentState?.OnVehicleLeaving(this, vehicle.gameObject);

        VehicleAI ai = vehicle.AI;
        if (ai != null && ai.CurrentIntersection == this)
        {
            ai.CurrentIntersection = null;
            ai.HasStoppedAtIntersection = false;
            ai.IntersectionStopTimer = 0f;
            ai.HasClearedIntersection = false;
        }
    }
}
