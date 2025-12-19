using System.Collections.Generic;

// Interface for elements that support vehicle navigation

public interface INavigable
{
    IEnumerable<IWaypoint> GetAllWaypoints();
    IEnumerable<Lane> GetLanes();
    bool CanEnter(Vehicle vehicle);
}