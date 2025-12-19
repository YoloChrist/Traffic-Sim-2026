using System.Collections.Generic;

// Interface for different lane behaviours

public interface ILaneStrategy
{
    IEnumerable<IWaypoint> GetWaypoints();
    bool CanVehicleEnter(Vehicle vehicle);
}