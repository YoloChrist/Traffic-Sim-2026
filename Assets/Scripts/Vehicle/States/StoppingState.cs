public class StoppingState : VehicleStateBase
{
    private const float STOPPED_THRESHOLD = 1f;

    public override void Enter(VehicleAI ai)
    {
        ai.NavMeshAgent.isStopped = false;
        SetSpeed(ai, 0f);
    }

    public override void Execute(VehicleAI ai)
    {
        if (ai.NavMeshAgent.velocity.magnitude <= STOPPED_THRESHOLD)
        {
            ai.ChangeState(new StoppedState());
            return;
        }

        if (!CheckAhead(ai, out DetectionResult result))
        {
            ai.ChangeState(new MovingState());
            return;
        }

        if (result.IntersectionAhead)
        {
            UpdateIntersectionTracking(ai, result.IntersectionRef);

            if (CanPassIntersection(ai, result.IntersectionRef))
            {
                ai.HasClearedIntersection = true;
                ai.ChangeState(new InIntersectionState());
                return;
            }
        }
        else if (result.VehicleAhead)
        {
            if (result.VehicleDistance > ai.LookAheadDistance * 0.75f)
            {
                ai.ChangeState(new SlowingDownState());
            }
        }
        else
        {
            ai.ChangeState(new MovingState());
        }
    }

    public override string GetStateName() => "Stopping";
}
