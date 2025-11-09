using UnityEngine;

public interface IIntersectionState
{
    void OnEnter(Intersection intersection);
    void OnExit(Intersection intersection);
    void Update(Intersection intersection);

    bool CanPassThrough(Intersection intersection, Vector3 approachDirection);
    string GetStateName();
}