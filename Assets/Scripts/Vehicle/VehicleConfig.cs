using UnityEngine;

// Config data for vehicle tpyes

[CreateAssetMenu(fileName = "VehicleConfig", menuName = "Traffic/Vehicle Config", order = 1)]
public class VehicleConfig : ScriptableObject
{
    public VehicleType vehicleType;
    public GameObject vehiclePrefab;
    public float maxSpeed = 20f;
    public float acceleration = 20f;
    public float deceleration = 50f;
    public float angularSpeed = 120f;
}