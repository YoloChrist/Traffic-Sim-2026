using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class IntersectionTrigger : MonoBehaviour
{
    private Intersection intersection;

    private void Awake()
    {
        intersection = GetComponent<Intersection>();
        
        BoxCollider triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Vehicle vehicle = other.GetComponent<Vehicle>();
        if (vehicle != null)
        {
            intersection.OnVehicleEnter(vehicle);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Vehicle vehicle = other.GetComponent<Vehicle>();
        if (vehicle != null)
        {
            intersection.OnVehicleExit(vehicle);
        }
    }
}