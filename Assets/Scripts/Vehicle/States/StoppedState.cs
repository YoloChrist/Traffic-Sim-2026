using UnityEngine;

public class StoppedState : VehicleStateBase
{
    private bool registeredAtIntersection;

    public override void Enter(VehicleAI ai)
    {
        ai.NavMeshAgent.isStopped = true;
        ai.NavMeshAgent.velocity = Vector3.zero;
        registeredAtIntersection = false;
    }

    public override void Execute(VehicleAI ai)
    {
        if (!CheckAhead(ai, out DetectionResult result))
        {
            ai.ChangeState(new MovingState());
            return;
        }

        if (result.IntersectionAhead)
        {
            HandleIntersectionStop(ai, result.IntersectionRef);
        }
        else if (result.VehicleAhead)
        {
            HandleVehicleStop(ai, result.VehicleDistance);
        }
        else
        {
            ai.ChangeState(new MovingState());
        }
    }

    public override void Exit(VehicleAI ai)
    {
        ai.NavMeshAgent.isStopped = false;
    }

    public override string GetStateName() => "Stopped";

    private void HandleIntersectionStop(VehicleAI ai, Intersection intersection)
    {
        UpdateIntersectionTracking(ai, intersection);

        if (!registeredAtIntersection)
        {
            intersection.CurrentState?.OnVehicleStopped(intersection, ai.Vehicle.gameObject);
            registeredAtIntersection = true;
            ai.HasStoppedAtIntersection = true;
        }

        ai.IntersectionStopTimer += Time.deltaTime;

        if (intersection.CurrentState?.CanVehicleLeave(intersection, ai.Vehicle.gameObject) == true)
        {
            ai.HasClearedIntersection = true;
            ai.ChangeState(new InIntersectionState());
        }
    }

    private void HandleVehicleStop(VehicleAI ai, float distance)
    {
        if (distance > ai.StoppingDistance * 1.5f)
        {
            ai.ChangeState(new SlowingDownState());
        }
    }
}
