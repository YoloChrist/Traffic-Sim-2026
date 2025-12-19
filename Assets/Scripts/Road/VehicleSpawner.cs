using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Spawn config")]
    [SerializeField] private Waypoint edgeWaypoint;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxVehicles = 10;
    [SerializeField] private LayerMask vehicleLayer;
    [SerializeField] private float minDistanceToExit = 40f;

    private float spawnTimer;
    private readonly List<Vehicle> spawnedVehicles = new List<Vehicle>();
    private VehicleType[] availableTypes;
    private TrafficManager trafficManager;

    private void Start()
    {
        if (!TryResolveTrafficManager())
        {
            Debug.LogError("[VehicleSpawner] TrafficManager instance not found. Disabling spawner.");
            enabled = false;
            return;
        }

        if (edgeWaypoint == null)
        {
            Debug.LogError("[VehicleSpawner] Edge waypoint is not assigned.");
            enabled = false;
            return;
        }

        if (edgeWaypoint.Type != WaypointType.Entry)
        {
            Debug.LogWarning($"[VehicleSpawner] Edge waypoint '{edgeWaypoint.name}' is not marked as Entry type!");
        }

        trafficManager.OnVehicleDespawnedEvent += HandleVehicleDespawned;
    }

    private void OnDestroy()
    {
        if (trafficManager != null)
        {
            trafficManager.OnVehicleDespawnedEvent -= HandleVehicleDespawned;
        }
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval && spawnedVehicles.Count < maxVehicles)
        {
            TrySpawnVehicle();
            spawnTimer = 0f;
        }

        spawnedVehicles.RemoveAll(v => v == null);
    }

    private bool TryResolveTrafficManager()
    {
        trafficManager = TrafficManager.Instance ?? FindAnyObjectByType<TrafficManager>();
        if (trafficManager == null)
        {
            return false;
        }

        availableTypes = trafficManager.GetAvailableTypes();
        return true;
    }

    private void HandleVehicleDespawned(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        spawnedVehicles.Remove(vehicle);
    }

    private void TrySpawnVehicle()
    {
        if (spawnedVehicles.Count >= maxVehicles)
        {
            return;
        }

        if (!IsSpawnPositionClear())
        {
            return;
        }

        VehicleType selectedType = SelectRandomVehicleType();
        Vehicle vehicle = SpawnVehicle(selectedType);

        if (vehicle != null)
        {
            spawnedVehicles.Add(vehicle);
        }
    }

    private Vehicle SpawnVehicle(VehicleType type)
    {
        Vector3 spawnPosition = edgeWaypoint.Position;
        Quaternion spawnRotation = GetSpawnRotation();

        Vehicle vehicle = trafficManager.GetVehicle(type, spawnPosition, spawnRotation);

        if (vehicle != null)
        {
            IWaypoint exitWaypoint = GetValidExitWaypoint();

            if (exitWaypoint != null)
            {
                vehicle.SetDestinationToExit(exitWaypoint);
            }
            else
            {
                Debug.LogError("[VehicleSpawner] No exit waypoints found! Vehicle will not have destination.");
                vehicle.SetWaypoint(edgeWaypoint);
            }

            trafficManager.OnVehicleSpawned(vehicle);
        }

        return vehicle;
    }

    private IWaypoint GetValidExitWaypoint()
    {
        List<IWaypoint> allExitWaypoints = RoadNetworkManager.Instance.GetAllExitWaypoints();

        if (allExitWaypoints == null || allExitWaypoints.Count == 0)
        {
            Debug.LogWarning("[VehicleSpawner] No exit waypoints available.");
            return null;
        }

        List<IWaypoint> validExits = new List<IWaypoint>();

        foreach (IWaypoint exit in allExitWaypoints)
        {
            float distance = Vector3.Distance(edgeWaypoint.Position, exit.Position);

            if (distance >= minDistanceToExit)
            {
                validExits.Add(exit);
            }
        }

        if (validExits.Count == 0)
        {
            Debug.LogWarning($"[VehicleSpawner] No exits beyond minimum distance ({minDistanceToExit}m). Using any available exit.");
            validExits = allExitWaypoints;
        }

        int randomIndex = Random.Range(0, validExits.Count);
        return validExits[randomIndex];
    }

    private bool IsSpawnPositionClear()
    {
        Vector3 spawnPos = edgeWaypoint.Position;
        Collider[] colliders = Physics.OverlapSphere(spawnPos, 2f, vehicleLayer);

        return colliders.Length == 0;
    }

    private VehicleType SelectRandomVehicleType()
    {
        if (availableTypes == null || availableTypes.Length == 0)
        {
            Debug.LogError("[VehicleSpawner] No vehicle types available.");
            return default;
        }

        int randomIndex = Random.Range(0, availableTypes.Length);
        return availableTypes[randomIndex];
    }

    private Quaternion GetSpawnRotation()
    {
        if (edgeWaypoint.NextWaypoint != null)
        {
            Vector3 direction = (edgeWaypoint.NextWaypoint.Position - edgeWaypoint.Position).normalized;
            return Quaternion.LookRotation(direction);
        }

        return edgeWaypoint.transform.rotation;
    }
}