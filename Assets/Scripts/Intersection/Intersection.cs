using UnityEngine;

public class Intersection : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color trafficLightColor = Color.red;
    [SerializeField] private Color roundaboutColor = Color.blue;
    [SerializeField] private Color stopSignColor = Color.yellow;

    private IIntersectionState currentState;

    private TrafficLightState trafficLightState;
    private RoundaboutState roundaboutState;
    private StopSignState stopSignState;

    // Grid position for pathfinding
    public Vector2Int GridPosition { get; private set; }

    private void Awake()
    {
        trafficLightState = new TrafficLightState();
        roundaboutState = new RoundaboutState();
        stopSignState = new StopSignState();

        SetState(stopSignState);

        GridPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
    }

    private void Update()
    {
        currentState?.Update(this);
    }

    // mouse down input

    public void CycleToNextState()
    {
        if (currentState is TrafficLightState)
            SetState(roundaboutState);
        else if (currentState is RoundaboutState)
            SetState(stopSignState);
        else if (currentState is StopSignState)
            SetState(trafficLightState);
    }

    private void SetState(IIntersectionState newState)
    {
        currentState?.OnExit(this);
        currentState = newState;
        currentState.OnEnter(this);
        
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (meshRenderer == null)
            Debug.LogWarning("MeshRenderer not assigned on Intersection.");

        Material material = meshRenderer.material;

        if (currentState is TrafficLightState)
            material.color = trafficLightColor;
        else if (currentState is RoundaboutState)
            material.color = roundaboutColor;
        else if (currentState is StopSignState)
            material.color = stopSignColor;
    }

    // Helper method to update visual externally
    public void SetVisualColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }
}
