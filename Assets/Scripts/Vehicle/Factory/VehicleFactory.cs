using System.Collections.Generic;
using UnityEngine;

// Concrete implementation of vehicle factory
public class VehicleFactory : IVehicleFactory
{
    private readonly Dictionary<VehicleType, VehicleConfig> vehicleConfigs;
    private readonly Transform vehicleParent;

    public VehicleFactory(VehicleConfig[] configs, Transform parent = null)
    {
        vehicleConfigs = new Dictionary<VehicleType, VehicleConfig>();
        vehicleParent = parent;

        if (configs == null)
        {
            Debug.LogError("VehicleFactory received null configs array.");
            return;
        }

        foreach (VehicleConfig config in configs)
        {
            RegisterVehicleConfig(config);
        }
    }

    public void RegisterVehicleConfig(VehicleConfig config)
    {
        if (config == null)
        {
            Debug.LogError("Cannot register null config.");
            return;
        }

        if (config.vehiclePrefab == null)
        {
            Debug.LogError($"Config for '{config.vehicleType}' has no prefab assigned.");
            return;
        }

        if (vehicleConfigs.ContainsKey(config.vehicleType))
        {
            Debug.LogWarning($"Vehicle config for type {config.vehicleType} is already registered. Overwriting.");
        }

        vehicleConfigs[config.vehicleType] = config;
    }

    public bool TryGetConfig(VehicleType type, out VehicleConfig config)
    {
        return vehicleConfigs.TryGetValue(type, out config);
    }

    public Vehicle CreateVehicle(VehicleType type, Vector3 position, Quaternion rotation)
    {
        if (!vehicleConfigs.TryGetValue(type, out VehicleConfig config))
        {
            Debug.LogError($"No vehicle config found for type {type}.");
            return null;
        }

        return CreateVehicle(config, position, rotation);
    }

    public Vehicle CreateVehicle(VehicleConfig config, Vector3 position, Quaternion rotation)
    {
        if (config == null || config.vehiclePrefab == null)
        {
            Debug.LogError("Invalid vehicle config or prefab.");
            return null;
        }

        GameObject vehicleObj = Object.Instantiate(config.vehiclePrefab, position, rotation, vehicleParent);
        vehicleObj.name = $"{config.vehicleType}_Vehicle";

        Vehicle vehicle = vehicleObj.GetComponent<Vehicle>();
        if (vehicle == null)
        {
            Debug.LogError("Vehicle prefab does not contain a Vehicle component.");
            Object.Destroy(vehicleObj);
            return null;
        }

        ApplyConfiguration(vehicle, config);
        return vehicle;
    }

    private void ApplyConfiguration(Vehicle vehicle, VehicleConfig config)
    {
        vehicle.MaxSpeed = config.maxSpeed;
        vehicle.Acceleration = config.acceleration;
        vehicle.Deceleration = config.deceleration;
        vehicle.AngularSpeed = config.angularSpeed;

        vehicle.SetVehicleConfig(config);
    }
}
