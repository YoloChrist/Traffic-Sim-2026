using System.Collections.Generic;
using UnityEngine;

public class VehiclePool
{
    private readonly Dictionary<VehicleType, Queue<Vehicle>> pools = new Dictionary<VehicleType, Queue<Vehicle>>();
    private readonly IVehicleFactory factory;
    private readonly Transform poolParent;

    public VehiclePool(IVehicleFactory factory, Transform parent)
    {
        this.factory = factory;
        poolParent = parent;
    }

    public void Prewarm(VehicleType type, int count)
    {
        if (count <= 0)
        {
            return;
        }

        EnsurePool(type);

        for (int i = 0; i < count; i++)
        {
            Vehicle vehicle = factory.CreateVehicle(type, Vector3.zero, Quaternion.identity);
            if (vehicle == null)
            {
                continue;
            }

            Return(vehicle);
        }
    }

    public Vehicle Get(VehicleType type, Vector3 position, Quaternion rotation)
    {
        EnsurePool(type);

        if (pools[type].Count > 0)
        {
            Vehicle vehicle = pools[type].Dequeue();
            Transform t = vehicle.transform;
            t.SetParent(null);
            t.SetPositionAndRotation(position, rotation);
            vehicle.gameObject.SetActive(true);
            vehicle.ResetForReuse();
            return vehicle;
        }

        return factory.CreateVehicle(type, position, rotation);
    }

    public void Return(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            return;
        }

        vehicle.gameObject.SetActive(false);
        vehicle.transform.SetParent(poolParent, false);

        VehicleType type = vehicle.GetVehicleConfig().vehicleType;
        EnsurePool(type);
        pools[type].Enqueue(vehicle);
    }

    public void ClearPools()
    {
        foreach (Queue<Vehicle> pool in pools.Values)
        {
            while (pool.Count > 0)
            {
                Vehicle vehicle = pool.Dequeue();
                if (vehicle != null)
                {
                    Object.Destroy(vehicle.gameObject);
                }
            }
        }

        pools.Clear();
        Debug.Log("[VehiclePool] All pools cleared");
    }

    private void EnsurePool(VehicleType type)
    {
        if (!pools.ContainsKey(type))
        {
            pools[type] = new Queue<Vehicle>();
        }
    }
}