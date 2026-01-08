using System.Collections.Generic;
using UnityEngine;

// Abstract class for all road elements

public abstract class RoadElementBase : MonoBehaviour, IRoadElement, IConnectable
{
    [SerializeField] protected string elementId;

    protected bool isInitialized;
    
    public Vector3 Position => transform.position;

    public virtual void Initialize()
    {
        if (isInitialized) return;

        GenerateElementId();
        OnInitialize();
        isInitialized = true;
    }

    protected virtual void OnInitialize() { }

    protected virtual void GenerateElementId()
    {
        if (string.IsNullOrEmpty(elementId))
        {
            elementId = $"{GetElementType()}_{GetInstanceID()}";
        }
    }

    public abstract string GetElementType();
    public abstract IEnumerable<IRoadElement> GetConnections();
    public abstract void AddConnection(IRoadElement element);
    public abstract void RemoveConnection(IRoadElement element);
    public abstract bool IsConnectedTo(IRoadElement element);

    protected virtual void Awake()
    {
        Initialize();
    }
}