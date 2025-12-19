using UnityEngine;

public class VehicleDetector
{
    // raycast to check for vehicles ahead
    public bool CheckForVehiclesAhead(VehicleAI ai, Vector3 origin, Vector3 forward, out float distance)
    {
        return CheckForComponentAhead<Vehicle>(
            origin,
            forward.normalized,
            ai.LookAheadDistance,
            ai.VehicleLayer,
            out _,
            out distance);
    }

    // raycast to check for vehicles ahead and get the vehicle reference
    public bool CheckForVehiclesAhead(VehicleAI ai, Vector3 origin, Vector3 forward, out Vehicle vehicle, out float distance)
    {
        return CheckForComponentAhead(
            origin,
            forward.normalized,
            ai.LookAheadDistance,
            ai.VehicleLayer,
            out vehicle,
            out distance);
    }

    // double raycast: fires rays from left/right offsets; either hit counts as blocked
    public bool CheckForVehiclesAheadWide(VehicleAI ai, Transform transform, float lateralOffset, out Vehicle vehicle, out float distance)
    {
        vehicle = null;
        distance = 0f;

        Vector3 forward = transform.forward.normalized;
        float maxDistance = ai.LookAheadDistance;
        LayerMask mask = ai.VehicleLayer;

        Vector3 leftOrigin = transform.position - transform.right * lateralOffset * 0.5f;
        Vector3 rightOrigin = transform.position + transform.right * lateralOffset * 0.5f;

        Vehicle leftVehicle;
        float leftDistance;
        bool leftHit = CheckForComponentAhead(leftOrigin, forward, maxDistance, mask, out leftVehicle, out leftDistance);

        Vehicle rightVehicle;
        float rightDistance;
        bool rightHit = CheckForComponentAhead(rightOrigin, forward, maxDistance, mask, out rightVehicle, out rightDistance);

        if (!leftHit && !rightHit)
        {
            return false;
        }

        // pick the closer hit
        if (leftHit && (!rightHit || leftDistance <= rightDistance))
        {
            vehicle = leftVehicle;
            distance = leftDistance;
        }
        else
        {
            vehicle = rightVehicle;
            distance = rightDistance;
        }

        return true;
    }

    // raycast to check for intersections - now uses RaycastAll to see through vehicles
    public bool CheckForIntersectionAhead(VehicleAI ai, Vector3 origin, Vector3 forward, out Intersection intersection, out float distance)
    {
        intersection = null;
        distance = 0f;

        RaycastHit[] hits = Physics.RaycastAll(origin, forward, ai.LookAheadDistance, ai.IntersectionLayer);

        if (hits.Length > 0)
        {
            float closestDistance = float.MaxValue;
            Intersection closestIntersection = null;

            foreach (RaycastHit hit in hits)
            {
                Intersection foundIntersection = hit.collider.GetComponent<Intersection>();
                if (foundIntersection != null && hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestIntersection = foundIntersection;
                }
            }

            if (closestIntersection != null)
            {
                intersection = closestIntersection;
                distance = closestDistance;
                Debug.DrawRay(origin, forward * distance, Color.red);
                return true;
            }
        }

        Debug.DrawRay(origin, forward * ai.LookAheadDistance, Color.green);
        return false;
    }

    // generic raycast method to detect any component type
    private bool CheckForComponentAhead<T>(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask, out T component, out float distance) where T : Component
    {
        component = null;
        distance = 0f;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, maxDistance, layerMask))
        {
            Debug.DrawRay(origin, direction * hit.distance, Color.red);
            component = hit.collider.GetComponent<T>();
            if (component != null)
            {
                distance = hit.distance;
                return true;
            }
        }
        else
        {
            Debug.DrawRay(origin, direction * maxDistance, Color.green);
        }

        return false;
    }

    // overload to avoid generic inference noise inside this class
    private bool CheckForComponentAhead<T>(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask, out T component, out float distance, bool unused = false) where T : Component
    {
        return CheckForComponentAhead(origin, direction, maxDistance, layerMask, out component, out distance);
    }
}