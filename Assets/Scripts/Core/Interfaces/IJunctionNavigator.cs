using System.Collections.Generic;

// for finding navigable waypoints from junction waypoints

public interface IJunctionNavigator
{
    IEnumerable<IWaypoint> GetExitWaypoints(IWaypoint junctionWaypoint);
}