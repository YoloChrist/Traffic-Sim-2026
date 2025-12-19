public interface IVehicleState
{
    void Enter(VehicleAI ai);
    void Execute(VehicleAI ai);
    void Exit(VehicleAI ai);
    string GetStateName(); // debugging
}