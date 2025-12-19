using UnityEngine;

public class InIntersectionState : VehicleStateBase
{
    private const float INTERSECTION_SPEED = 40f;
    private const float INTERSECTION_ACCELERATION = 120f;
    private const float INTERSECTION_ANGULAR_SPEED = 900;
    private const float INTERSECTION_DECELERATION = 120f;
    private const float SAFETY_TIMEOUT = 8f;

    private float originalSpeed;
    private float originalAcceleration;
    private float originalAngularSpeed;
    private float originalDeceleration;
    private float timeInIntersection;
    private Intersection enteredIntersection;

    public override void Enter(VehicleAI ai)
    {
        if (ai.CurrentIntersection == null)
        {
            Debug.LogWarning("[InIntersectionState] Entered state without CurrentIntersection set");
            ai.ChangeState(new MovingState());
            return;
        }

        // Store original NavMeshAgent settings
        originalSpeed = ai.NavMeshAgent.speed;
        originalAcceleration = ai.NavMeshAgent.acceleration;
        originalAngularSpeed = ai.NavMeshAgent.angularSpeed;
        originalDeceleration = ai.Deceleration;

        // Store reference to intersection we entered
        enteredIntersection = ai.CurrentIntersection;

        // Apply intersection-specific NavMeshAgent settings
        ai.NavMeshAgent.speed = INTERSECTION_SPEED;
        ai.NavMeshAgent.acceleration = INTERSECTION_ACCELERATION;
        ai.NavMeshAgent.angularSpeed = INTERSECTION_ANGULAR_SPEED;
        ai.Deceleration = INTERSECTION_DECELERATION;

        // Mark that vehicle has cleared the intersection entry phase
        ai.HasClearedIntersection = true;
        timeInIntersection = 0f;

        ai.NavMeshAgent.autoBraking = false;
    }

    public override void Execute(VehicleAI ai)
    {
        timeInIntersection += Time.deltaTime;

        // Check if CurrentIntersection changed (means OnVehicleExit was called via trigger)
        if (ai.CurrentIntersection == null || ai.CurrentIntersection != enteredIntersection)
        {
            ai.ChangeState(new MovingState());
            return;
        }

        // Safety timeout in case trigger fails
        if (timeInIntersection > SAFETY_TIMEOUT)
        {
            Debug.LogWarning($"[InIntersectionState] Vehicle {ai.Vehicle.name} exceeded timeout, forcing exit");
            ai.CurrentIntersection.OnVehicleExit(ai.Vehicle);
            ai.ChangeState(new MovingState());
            return;
        }

        // Check for obstacles even while in intersection
        if (CheckAhead(ai, out DetectionResult result))
        {
            if (result.VehicleAhead)
            {
                HandleVehicleAhead(ai, result);
            }
        }
    }

    public override void Exit(VehicleAI ai)
    {
        // Restore original NavMeshAgent settings
        ai.NavMeshAgent.speed = originalSpeed;
        ai.NavMeshAgent.acceleration = originalAcceleration;
        ai.NavMeshAgent.angularSpeed = originalAngularSpeed;
        ai.Deceleration = originalDeceleration;

        // Clear intersection tracking after exiting
        ai.CurrentIntersection = null;
        ai.HasStoppedAtIntersection = false;
        ai.IntersectionStopTimer = 0f;
        ai.HasClearedIntersection = false;

        ai.NavMeshAgent.autoBraking = false;
    }

    public override string GetStateName()
    {
        return "In Intersection";
    }

    private void HandleVehicleAhead(VehicleAI ai, DetectionResult result)
    {
        float stoppingDist = CalculateStoppingDistance(ai.NavMeshAgent.speed, ai.Deceleration);
        float safeDistance = stoppingDist + ai.StoppingDistance;

        if (result.VehicleDistance < safeDistance)
        {
            float speedReduction = 1f - (safeDistance - result.VehicleDistance) / safeDistance;
            float targetSpeed = INTERSECTION_SPEED * Mathf.Clamp01(speedReduction);
            ai.NavMeshAgent.speed = Mathf.Max(targetSpeed, 0f);
        }
    }
}