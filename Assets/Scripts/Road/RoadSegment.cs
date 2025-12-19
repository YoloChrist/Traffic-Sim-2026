using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Composite road element

public class RoadSegment : RoadElementBase, INavigable
{
    [Header("Lanes")]
    [SerializeField] private List<Lane> lanes = new List<Lane>(); // All lanes in this road segment

    [Header("Connections")]
    [SerializeField] private List<IRoadElement> connectedElements = new List<IRoadElement>(); // Connected road elements

    protected override void OnInitialize()
    {

    }

    public override string GetElementType()
    {
        return "RoadSegment";
    }

    // INavigable implementation
    public IEnumerable<IWaypoint> GetAllWaypoints()
    {
        if (!isInitialized) 
            Initialize();
        return lanes.SelectMany(lane => lane.GetWaypoints());
    }

    public IEnumerable<Lane> GetLanes()
    {
        return lanes;
    }

    public bool CanEnter(Vehicle vehicle)
    {

        return lanes.Any(lane => lane.CanVehicleEnter(vehicle));
    }

    // IConnectable implementation
    public override IEnumerable<IRoadElement> GetConnections()
    {
        return connectedElements;
    }

    public override void AddConnection(IRoadElement element)
    {
        if (!connectedElements.Contains(element))
        {
            connectedElements.Add(element);
        }
    }

    public override void RemoveConnection(IRoadElement element)
    {
        connectedElements.Remove(element);
    }

    public override bool IsConnectedTo(IRoadElement element)
    {
        return connectedElements.Contains(element);
    }

    public Waypoint GetEntryWaypoint(int laneIndex = 0)
    {

        if (laneIndex >= 0 && laneIndex < lanes.Count)
            return lanes[laneIndex].GetEntryWaypoint();

        return lanes.FirstOrDefault()?.GetEntryWaypoint();
    }
}

[Serializable]
public class RoadConnection
{
    public IRoadElement ConnectedElement;
}