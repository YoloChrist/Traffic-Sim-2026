using UnityEngine;

// Interface for vehicle factory

public interface IVehicleFactory
{
    Vehicle CreateVehicle(VehicleType type, Vector3 position, Quaternion rotation);
    Vehicle CreateVehicle(VehicleConfig config, Vector3 position, Quaternion rotation);
    void RegisterVehicleConfig(VehicleConfig config);
}