using System.Collections.Generic;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    private static TrafficManager instance;
    public static TrafficManager Instance => instance;

    [Header("Vehicle Factory")]
    [SerializeField] private VehicleConfig[] vehicleConfigs;
    [SerializeField] private Transform vehicleContainer;

    [Header("Pooling Settings")]
    [SerializeField] private bool usePooling = true;
    [SerializeField] private int initialPoolSize = 10;

    [Header("Stats")]
    [SerializeField] private int totalVehiclesSpawned;
    [SerializeField] private int totalVehiclesDespawned;
    [SerializeField] private int totalVehiclesPooled;
    [SerializeField] private int activeVehicles;

    private IVehicleFactory vehicleFactory;
    private VehiclePool vehiclePool;
    private readonly List<Vehicle> activeVehicleList = new List<Vehicle>();
    private VehicleType[] availableTypes;

    // Event to notify when a vehicle is despawned
    public event System.Action<Vehicle> OnVehicleDespawnedEvent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Initialise();
    }

    private void Initialise()
    {
        if (vehicleContainer == null)
        {
            GameObject container = new GameObject("Vehicles");
            vehicleContainer = container.transform;
        }

        vehicleFactory = new VehicleFactory(vehicleConfigs, vehicleContainer);
        availableTypes = BuildAvailableTypes(vehicleConfigs);

        if (usePooling)
        {
            vehiclePool = new VehiclePool(vehicleFactory, vehicleContainer);
            PrewarmPool();
        }
    }

    // create vehicles for the pool
    private void PrewarmPool()
    {
        if (vehiclePool == null || availableTypes == null)
        {
            return;
        }

        foreach (VehicleType type in availableTypes)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                Vehicle vehicle = vehicleFactory.CreateVehicle(type, Vector3.zero, Quaternion.identity);
                if (vehicle != null)
                {
                    vehiclePool.Return(vehicle);
                }
            }
        }
    }

    // get vehicle from pool
    public Vehicle GetVehicle(VehicleType type, Vector3 position, Quaternion rotation)
    {
        if (usePooling && vehiclePool != null)
        {
            return vehiclePool.Get(type, position, rotation);
        }

        return vehicleFactory.CreateVehicle(type, position, rotation);
    }

    // return vehicle to pool
    public void ReturnVehicle(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        if (usePooling && vehiclePool != null)
        {
            vehiclePool.Return(vehicle);
            totalVehiclesPooled++;
        }
        else
        {
            Destroy(vehicle.gameObject);
        }
    }

    public IVehicleFactory GetVehicleFactory() => vehicleFactory;

    public VehicleType[] GetAvailableTypes() => availableTypes;

    public void OnVehicleSpawned(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        activeVehicleList.Add(vehicle);
        totalVehiclesSpawned++;
        activeVehicles = activeVehicleList.Count;
    }

    public void OnVehicleDespawned(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        activeVehicleList.Remove(vehicle);
        totalVehiclesDespawned++;
        activeVehicles = activeVehicleList.Count;

        // Invoke event before returning to pool
        OnVehicleDespawnedEvent?.Invoke(vehicle);
        ReturnVehicle(vehicle);
    }

    public void OnVehicleDestroyed(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        activeVehicleList.Remove(vehicle);
        activeVehicles = activeVehicleList.Count;

        if (usePooling && vehiclePool != null)
        {
            vehiclePool.Return(vehicle);
        }
    }

    public int GetActiveVehicleCount() => activeVehicles;

    private void Update()
    {
        activeVehicleList.RemoveAll(v => v == null);
        activeVehicles = activeVehicleList.Count;
    }

    private static VehicleType[] BuildAvailableTypes(VehicleConfig[] configs)
    {
        if (configs == null || configs.Length == 0)
        {
            return System.Array.Empty<VehicleType>();
        }

        List<VehicleType> types = new List<VehicleType>();
        foreach (VehicleConfig config in configs)
        {
            if (config != null)
            {
                types.Add(config.vehicleType);
            }
        }

        return types.ToArray();
    }
}
