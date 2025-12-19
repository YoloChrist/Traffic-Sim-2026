using UnityEngine;

public abstract class VehicleStateBase : IVehicleState
{
    protected VehicleDetector detector;

    protected VehicleStateBase()
    {
        detector = new VehicleDetector();
    }

    public virtual void Enter(VehicleAI ai) { }
    public abstract void Execute(VehicleAI ai);
    public virtual void Exit(VehicleAI ai) { }
    public abstract string GetStateName();

    protected bool CheckAhead(VehicleAI ai, out DetectionResult result)
    {
        result = new DetectionResult();
        Vector3 origin = ai.Vehicle.Position;
        Vector3 forward = ai.Vehicle.Forward;

        result.VehicleAhead = detector.CheckForVehiclesAhead(ai, origin, forward, out result.VehicleRef, out result.VehicleDistance);
        result.IntersectionAhead = detector.CheckForIntersectionAhead(ai, origin, forward, out result.IntersectionRef, out result.IntersectionDistance);

        if (result.VehicleAhead && result.IntersectionAhead)
        {
            if (result.VehicleDistance < result.IntersectionDistance)
            {
                result.IntersectionAhead = false;
                result.IntersectionRef = null;
                result.IntersectionDistance = 0f;
            }
            else
            {
                result.VehicleAhead = false;
                result.VehicleRef = null;
                result.VehicleDistance = 0f;
            }
        }

        return result.VehicleAhead || result.IntersectionAhead;
    }

    protected void UpdateIntersectionTracking(VehicleAI ai, Intersection intersection)
    {
        if (ai.CurrentIntersection != intersection)
        {
            ai.CurrentIntersection = intersection;
            ai.HasStoppedAtIntersection = false;
            ai.IntersectionStopTimer = 0f;
            ai.HasClearedIntersection = false;
        }
    }

    protected bool CanPassIntersection(VehicleAI ai, Intersection intersection)
    {
        if (intersection?.CurrentState == null)
            return true;

        if (intersection.CurrentState is TrafficLightState trafficLight)
        {
            if (ai.HasStoppedAtIntersection)
            {
                return trafficLight.CanVehicleLeave(intersection, ai.Vehicle.gameObject);
            }

            trafficLight.OnVehicleStopped(intersection, ai.Vehicle.gameObject);
            bool canPass = trafficLight.CanVehicleLeave(intersection, ai.Vehicle.gameObject);
            return canPass;
        }

        if (intersection.CurrentState is StopSignState)
        {
            if (!ai.HasStoppedAtIntersection)
                return false;

            return intersection.CurrentState.CanVehicleLeave(intersection, ai.Vehicle.gameObject);
        }

        Vector3 approachDirection = (intersection.transform.position - ai.Vehicle.Position).normalized;
        return intersection.CurrentState.CanPassThrough(intersection, approachDirection);
    }

    protected void SetSpeed(VehicleAI ai, float desiredSpeed)
    {
        float finalSpeed = Mathf.Clamp(desiredSpeed, 0f, ai.MaxSpeed);
        ai.NavMeshAgent.speed = finalSpeed;
    }

    protected float CalculateStoppingDistance(float currentSpeed, float deceleration)
    {
        if (deceleration <= 0f)
            return 0f;

        return (currentSpeed * currentSpeed) / (2f * deceleration);
    }

    protected class DetectionResult
    {
        public bool VehicleAhead;
        public Vehicle VehicleRef;
        public float VehicleDistance;
        public bool IntersectionAhead;
        public Intersection IntersectionRef;
        public float IntersectionDistance;
    }
}