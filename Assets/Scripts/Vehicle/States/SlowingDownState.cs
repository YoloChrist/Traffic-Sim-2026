using UnityEngine;
using UnityEngine.AI;

public class SlowingDownState : VehicleStateBase
{
    private const float STATIONARY_THRESHOLD = 1f;
    private const float SPEED_UP_THRESHOLD = 0.8f;

    public override void Enter(VehicleAI ai)
    {
        ai.NavMeshAgent.isStopped = false;
    }

    public override void Execute(VehicleAI ai)
    {
        if (!CheckAhead(ai, out DetectionResult result))
        {
            SpeedUpOrTransition(ai);
            return;
        }

        if (result.VehicleAhead)
        {
            HandleVehicleAhead(ai, result.VehicleRef, result.VehicleDistance);
        }
        else if (result.IntersectionAhead)
        {
            HandleIntersectionAhead(ai, result.IntersectionRef, result.IntersectionDistance);
        }
        else
        {
            SpeedUpOrTransition(ai);
        }
    }

    public override string GetStateName() => "SlowingDown";

    private void HandleVehicleAhead(VehicleAI ai, Vehicle vehicleAhead, float distance)
    {
        if (vehicleAhead == null)
            return;

        NavMeshAgent targetAgent = vehicleAhead.GetComponent<NavMeshAgent>();
        if (targetAgent == null)
            return;

        float targetSpeed = targetAgent.velocity.magnitude;
        float minSafeDistance = ai.StoppingDistance * 3f;

        if (distance <= ai.StoppingDistance)
        {
            ai.ChangeState(new StoppingState());
            return;
        }

        if (targetSpeed < STATIONARY_THRESHOLD && distance <= minSafeDistance)
        {
            ai.ChangeState(new StoppingState());
            return;
        }

        float desiredSpeed = CalculateFollowSpeed(ai, targetSpeed, distance);
        SetSpeed(ai, desiredSpeed);
    }

    private void HandleIntersectionAhead(VehicleAI ai, Intersection intersection, float distance)
    {
        UpdateIntersectionTracking(ai, intersection);

        if (ai.HasClearedIntersection)
        {
            ai.ChangeState(new InIntersectionState());
            return;
        }

        if (CanPassIntersection(ai, intersection))
        {
            ai.HasClearedIntersection = true;
            ai.ChangeState(new InIntersectionState());
            return;
        }

        float currentSpeed = ai.NavMeshAgent.speed;
        float requiredStoppingDistance = CalculateStoppingDistance(currentSpeed, ai.Deceleration) * 1.1f;
        const float STOP_BUFFER = 10f;

        if (distance - STOP_BUFFER <= requiredStoppingDistance)
        {
            ai.ChangeState(new StoppingState());
        }
        else
        {
            float slowingFactor = Mathf.Clamp01(distance / ai.LookAheadDistance);
            SetSpeed(ai, ai.MaxSpeed * slowingFactor);
        }
    }

    private float CalculateFollowSpeed(VehicleAI ai, float targetSpeed, float distance)
    {
        if (distance <= ai.StoppingDistance)
        {
            return targetSpeed;
        }

        float approachRange = ai.LookAheadDistance - ai.StoppingDistance;
        float distanceFromStop = distance - ai.StoppingDistance;
        float approachFactor = Mathf.Clamp01(distanceFromStop / approachRange);

        return Mathf.Lerp(targetSpeed, ai.MaxSpeed, approachFactor);
    }

    private void SpeedUpOrTransition(VehicleAI ai)
    {
        // If already cleared and still flagged in current intersection, go straight to intersection handling
        if (ai.HasClearedIntersection && ai.CurrentIntersection != null)
        {
            ai.ChangeState(new InIntersectionState());
            return;
        }

        float currentSpeed = ai.NavMeshAgent.speed;

        if (currentSpeed >= ai.MaxSpeed * SPEED_UP_THRESHOLD)
        {
            ai.ChangeState(new MovingState());
        }
        else
        {
            SetSpeed(ai, ai.MaxSpeed);
        }
    }
}
