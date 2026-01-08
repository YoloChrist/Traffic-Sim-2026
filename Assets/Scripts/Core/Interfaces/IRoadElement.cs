using UnityEngine;

// Base interface for all road network elements (Composite Pattern)

public interface IRoadElement
{
    Vector3 Position { get; }
    string GetElementType();
    void Initialize();
}