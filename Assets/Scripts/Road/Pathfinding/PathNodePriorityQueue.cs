using System.Collections.Generic;

// Priority queue for A* pathfinding (min-heap by F-score, then H-score for stability)
public class PathNodePriorityQueue
{
    private readonly List<PathNode> nodes = new List<PathNode>();

    public int Count => nodes.Count;

    public void Enqueue(PathNode node)
    {
        if (node == null)
        {
            return;
        }

        nodes.Add(node);
        int currentIndex = nodes.Count - 1;

        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;
            if (IsHigherOrEqualPriority(parentIndex, currentIndex))
            {
                break;
            }

            Swap(parentIndex, currentIndex);
            currentIndex = parentIndex;
        }
    }

    public PathNode Dequeue()
    {
        if (nodes.Count == 0)
        {
            return null;
        }

        PathNode result = nodes[0];
        int lastIndex = nodes.Count - 1;
        nodes[0] = nodes[lastIndex];
        nodes.RemoveAt(lastIndex);

        HeapifyDown(0);
        return result;
    }

    public void Clear()
    {
        nodes.Clear();
    }

    private void HeapifyDown(int index)
    {
        int currentIndex = index;

        while (true)
        {
            int left = 2 * currentIndex + 1;
            int right = 2 * currentIndex + 2;
            int smallest = currentIndex;

            if (left < nodes.Count && IsHigherPriority(left, smallest))
            {
                smallest = left;
            }

            if (right < nodes.Count && IsHigherPriority(right, smallest))
            {
                smallest = right;
            }

            if (smallest == currentIndex)
            {
                break;
            }

            Swap(currentIndex, smallest);
            currentIndex = smallest;
        }
    }

    private bool IsHigherPriority(int a, int b)
    {
        return nodes[a].FScore < nodes[b].FScore ||
               (nodes[a].FScore == nodes[b].FScore && nodes[a].HScore < nodes[b].HScore);
    }

    private bool IsHigherOrEqualPriority(int a, int b)
    {
        return !IsHigherPriority(b, a);
    }

    private void Swap(int i, int j)
    {
        PathNode temp = nodes[i];
        nodes[i] = nodes[j];
        nodes[j] = temp;
    }
}

// PathNode used by A* algorithm
public class PathNode
{
    public IWaypoint Waypoint { get; }
    public PathNode Parent { get; set; }
    public float GScore { get; set; } // cost from start to this node
    public float HScore { get; }      // heuristic cost to goal
    public float FScore => GScore + HScore; // total cost

    public PathNode(IWaypoint waypoint, PathNode parent, float gScore, float hScore)
    {
        Waypoint = waypoint;
        Parent = parent;
        GScore = gScore;
        HScore = hScore;
    }
}