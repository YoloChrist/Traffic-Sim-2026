using System.Collections.Generic;

// Interface for road elements that can connect to others

public interface IConnectable
{
    IEnumerable<IRoadElement> GetConnections();
    void AddConnection(IRoadElement element);
    void RemoveConnection(IRoadElement element);
    bool IsConnectedTo(IRoadElement element);
}