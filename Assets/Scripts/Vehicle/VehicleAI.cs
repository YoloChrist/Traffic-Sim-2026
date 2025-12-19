using UnityEngine;
using UnityEngine.AI;

public class VehicleAI
{
    private readonly Vehicle vehicle;
    private readonly NavMeshAgent navMeshAgent;
    private IVehicleState currentState;

    // Detection settings
    public float MaxSpeed { get; set; }
    public float Acceleration { get; set; }
    public float Deceleration { get; set; }
    public float LookAheadDistance { get; set; }
    public float StoppingDistance { get; set; }
    public LayerMask VehicleLayer { get; set; }
    public LayerMask IntersectionLayer { get; set; }

    // Intersection state
    public Intersection CurrentIntersection { get; set; }
    public bool HasStoppedAtIntersection { get; set; }
    public float IntersectionStopTimer { get; set; }
    public bool HasClearedIntersection { get; set; }

    // Public accessors for states
    public Vehicle Vehicle => vehicle;
    public NavMeshAgent NavMeshAgent => navMeshAgent;

    public VehicleAI(Vehicle vehicle, NavMeshAgent navMeshAgent)
    {
        this.vehicle = vehicle;
        this.navMeshAgent = navMeshAgent;
    }

    public void ChangeState(IVehicleState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    public void Update()
    {
        currentState?.Execute(this);
    }

    public void Reset()
    {
        ChangeState(new MovingState());
        CurrentIntersection = null;
        HasStoppedAtIntersection = false;
        IntersectionStopTimer = 0f;
        HasClearedIntersection = false;
    }

    public IVehicleState GetCurrentState() => currentState;
}