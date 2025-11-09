using UnityEngine;

public class TrafficLightState : IIntersectionState
{
    private enum LightColor { Red, Amber, Green }

    private LightColor currentLight;
    private float timer;

    private const float RED_DURATION = 5f;
    private const float AMBER_DURATION = 2f;
    private const float GREEN_DURATION = 8f;

    public void OnEnter(Intersection intersection)
    {
        currentLight = LightColor.Red;
        timer = RED_DURATION;
        UpdateLightColor(intersection);
    }

    public void OnExit(Intersection intersection)
    {
        // No specific exit logic needed for traffic lights
    }

    public void Update(Intersection intersection)
    {
        // Countdown timer
        timer -= Time.deltaTime;

        // Next phase when timer runs out
        if (timer <= 0f)
        {
            switch (currentLight)
            {
                case LightColor.Red:
                    currentLight = LightColor.Green;
                    timer = GREEN_DURATION;
                    break;
                case LightColor.Green:
                    currentLight = LightColor.Amber;
                    timer = AMBER_DURATION;
                    break;
                case LightColor.Amber:
                    currentLight = LightColor.Red;
                    timer = RED_DURATION;
                    break;
            }

            UpdateLightColor(intersection);
        }
    }

    public bool CanPassThrough(Intersection intersection, Vector3 approachDirection)
    {
        // Only pass on green
        return currentLight == LightColor.Green;
    }

    public string GetStateName()
    {
        return $"Traffic Light ({currentLight})";
    }

    private void UpdateLightColor(Intersection intersection)
    {
        switch (currentLight)
        {
            case LightColor.Red:
                intersection.SetVisualColor(Color.red);
                break;
            case LightColor.Amber:
                intersection.SetVisualColor(Color.orange);
                break;
            case LightColor.Green:
                intersection.SetVisualColor(Color.green);
                break;
        }
    }
}