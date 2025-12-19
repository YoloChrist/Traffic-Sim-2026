public class MovingState : VehicleStateBase
{
    public override void Enter(VehicleAI ai)
    {
        ai.NavMeshAgent.isStopped = false;
        SetSpeed(ai, ai.MaxSpeed);
    }

    public override void Execute(VehicleAI ai)
    {
        if (!CheckAhead(ai, out DetectionResult result))
            return;

        if (result.VehicleAhead)
        {
            HandleVehicleAhead(ai, result.VehicleDistance);
        }
        else if (result.IntersectionAhead)
        {
            HandleIntersectionAhead(ai, result.IntersectionRef, result.IntersectionDistance);
        }
    }

    public override string GetStateName() => "Moving";

    private void HandleVehicleAhead(VehicleAI ai, float distance)
    {
        if (distance < ai.StoppingDistance)
        {
            ai.ChangeState(new StoppingState());
        }
        else if (distance < ai.LookAheadDistance)
        {
            ai.ChangeState(new SlowingDownState());
        }
    }

    private void HandleIntersectionAhead(VehicleAI ai, Intersection intersection, float distance)
    {
        // Already cleared to pass through
        if (ai.HasClearedIntersection && ai.CurrentIntersection == intersection)
            return;

        UpdateIntersectionTracking(ai, intersection);

        if (CanPassIntersection(ai, intersection))
        {
            ai.HasClearedIntersection = true;
            ai.ChangeState(new InIntersectionState());
            return;
        }

        // Need to slow down or stop
        if (distance < ai.StoppingDistance * 2f)
        {
            ai.ChangeState(new StoppingState());
        }
        else if (distance < ai.LookAheadDistance)
        {
            ai.ChangeState(new SlowingDownState());
        }
    }
}
