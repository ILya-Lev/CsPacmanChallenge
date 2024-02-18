using System.Drawing;

namespace PacmanBot;

public static class PathAlgorithms
{
    public static int GetManhattanDistance(Cell start, Cell end) 
        => GetManhattanDistance(start.Location, end.Location);

    public static int GetManhattanDistance(Point start, Point end) 
        => Math.Abs(end.X - start.X)
           + Math.Abs(end.Y - start.Y);


    /// <summary> DFS depth-first search </summary>
    public static List<Cell> FindDfsPath(Cell start, Cell end, HashSet<Point> ghosts)
    {
        var stack = new Stack<Cell>();
        stack.Push(start);
        var visited = new HashSet<Cell>();

        while (stack.Any())
        {
            var current = stack.Peek();
            visited.Add(current);

            var validNeighbors = current.Neighbors
                .Where(n => !ghosts.Contains(n.Location) && !visited.Contains(n))
                .ToArray();

            if (!validNeighbors.Any())
                stack.Pop();

            stack.Push(validNeighbors.First());
            if (validNeighbors.First() == end)
                break;
        }

        var path = new List<Cell>(stack);
        return path;
    }

    /// <summary> A* </summary>
    public static List<Cell> FindAPath(Cell start, Cell end, HashSet<Point> ghosts)
    {
        var path = new List<Cell>();
        var token = GoDownA_Star(start, end, ghosts);
        while (token?.Parent is not null)
        {
            path.Add(token.Current);
            token = token.Parent;
        }
        path.Reverse();
        return path;
    }

    private static Node? GoDownA_Star(Cell start, Cell end, HashSet<Point> ghosts)
    {
        var frontier = new PriorityQueue<Node, int>();
        frontier.Enqueue(new Node(start), GetManhattanDistance(start, end));
        var seen = new HashSet<Cell> { start };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current.Current == end)
                return current;

            var validNeighbors = current.Current.Neighbors
                .Where(n => !ghosts.Contains(n.Location) && !seen.Contains(n))
                .ToArray();
            foreach (var neighbor in validNeighbors)
            {
                seen.Add(neighbor);
                frontier.Enqueue(new Node(neighbor) {Parent = current}
                    , GetManhattanDistance(neighbor, end));
            }
        }

        return null;
    }

    private class Node
    {
        public Node(Cell current) => Current = current;

        public Node? Parent { get; init; }
        public Cell Current { get; }
    }

}