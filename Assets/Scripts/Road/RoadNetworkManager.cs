using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Manages whole road network - facade pattern, like an API for other scripts

public class RoadNetworkManager : MonoBehaviour
{
    private static RoadNetworkManager instance;
    public static RoadNetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<RoadNetworkManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("RoadNetworkManager");
                    instance = go.AddComponent<RoadNetworkManager>();
                }
            }
            return instance;
        }
    }

    [Header("Network Elements")]
    [SerializeField] private List<RoadSegment> roadSegments = new List<RoadSegment>();
    [SerializeField] private List<Intersection> intersections = new List<Intersection>();

    // Services
    private WaypointRegistry waypointRegistry;
    private RoadRegistry roadRegistry;
    private PathfindingService pathfindingService;
    private IJunctionNavigator junctionNavigator;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        Initialize();
    }

    private void Initialize()
    {
        if (roadSegments.Count == 0)
        {
            roadSegments = FindObjectsByType<RoadSegment>(FindObjectsSortMode.None).ToList();
        }

        if (intersections.Count == 0)
        {
            intersections = FindObjectsByType<Intersection>(FindObjectsSortMode.None).ToList();
        }

        roadRegistry = new RoadRegistry(roadSegments, intersections);
        waypointRegistry = new WaypointRegistry(roadSegments);
        junctionNavigator = new IntersectionJunctionNavigator(this);
        pathfindingService = new PathfindingService(new AStarPathfinder(this));

        roadRegistry.BuildConnections();
        
        // Inject junction navigator into all waypoints
        InjectJunctionNavigatorIntoWaypoints();

        Debug.Log($"[RoadNetwork] Initialized with {roadSegments.Count} roads, {intersections.Count} intersections");
    }

    private void InjectJunctionNavigatorIntoWaypoints()
    {
        var allWaypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);
        
        foreach (var waypoint in allWaypoints)
        {
            waypoint.SetJunctionNavigator(junctionNavigator);
        }
    }

    // Public API

    public IWaypoint GetRandomExitWaypoint()
    {
        return waypointRegistry.GetRandomWaypointOfType(WaypointType.Exit);
    }

    public IWaypoint GetRandomEntryWaypoint()
    {
        return waypointRegistry.GetRandomWaypointOfType(WaypointType.Entry);
    }

    public List<IWaypoint> GetWaypointsByType(WaypointType type)
    {
        return waypointRegistry.GetWaypointsByType(type);
    }

    public List<IWaypoint> GetAllExitWaypoints()
    {
        return waypointRegistry.GetWaypointsByType(WaypointType.Exit);
    }

    public List<IWaypoint> GetAllEntryWaypoints()
    {
        return waypointRegistry.GetWaypointsByType(WaypointType.Entry);
    }

    public IWaypoint GetNearestWaypoint(Vector3 position)
    {
        return waypointRegistry.GetNearestWaypoint(position);
    }

    public void RefreshWaypointCache()
    {
        waypointRegistry.RefreshCache();
    }

    public List<IWaypoint> FindPath(Vector3 start, Vector3 end)
    {
        return pathfindingService.FindPath(start, end);
    }

    public IRoadElement GetRoadElementAtPosition(Vector3 position, float searchRadius = 10f)
    {
        return roadRegistry.GetRoadElementAtPosition(position, searchRadius);
    }

    public List<RoadSegment> GetAllRoads()
    {
        return roadRegistry.GetAllRoads();
    }

    public List<Intersection> GetAllIntersections()
    {
        return roadRegistry.GetAllIntersections();
    }
}