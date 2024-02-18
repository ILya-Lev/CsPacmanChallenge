using CsPacman.Game;
using PacmanBot;

namespace CsPacman.Bot;

public class BotPlayer : IPlayer
{
    private const int GhostInertia = 2;
    private const int CloseDistance = 80;

    private readonly ITracer _tracer;
    private StateSnapshot? _previousState = null;
    private Dictionary<(int, int), Cell>? _allCells;

    public BotPlayer(ITracer tracer) => _tracer = tracer;

    public Point Step(StateSnapshot state)
    {
        try
        {
            if (_allCells is null)
                Initialize(state);

            AssignCellValues(state);

            var path = FindBestPath(state);
            var targetMove = FindTargetMove(state, path);

            UpdatePlayersState(state, targetMove);

            return targetMove;
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc);
            return default;
        }
    }

    private void Initialize(StateSnapshot state)
    {
        InitializeAllCells(state.level);
        InitializeNeighbors(state.level);
    }

    private void InitializeAllCells(Level level)
    {
        var cells = new Dictionary<(int, int), Cell>();
        for (int row = 0; row < Level.Height; row++)
            for (int col = 0; col < Level.Width; col++)
            {
                var location = new Point(row, col);
                if (level.IsWall(location))
                    continue;
                cells.Add((row, col), new Cell() { Location = location });
            }

        _allCells = cells;
    }

    private void InitializeNeighbors(Level level)
    {
        foreach (var cell in _allCells!.Values)
        {
            foreach (var move in Moves.All)
            {
                var shiftedLocation = (cell.Location.X + move.X, cell.Location.Y + move.Y);
                if (_allCells.TryGetValue(shiftedLocation, out var neighbor))
                    cell.RegisterNeighbor(neighbor);
            }

            for (int i = 0; i < level.teleport.Length; i++)
            {
                if (level.teleport[i] == cell.Location)
                {
                    var next = (i + 1) % level.teleport.Length;
                    var point = level.teleport[next];
                    if (_allCells.TryGetValue((point.X, point.Y), out var teleportNeighbor))
                        cell.RegisterNeighbor(teleportNeighbor);
                }
            }
        }
    }

    private void AssignCellValues(StateSnapshot state)
    {
        foreach (var cell in _allCells!.Values)
        {
            var closeGhosts = state.ghosts
                .Select((g, i) => (g, i))
                .Where(item => PathAlgorithms
                    .GetManhattanDistance(cell.Location, item.g) < CloseDistance)
                .ToArray();
            
            if (!closeGhosts.Any())
            {
                cell.Value = int.MaxValue;
                continue;
            }

            cell.Value = closeGhosts.Sum(item =>
            {
                var ghostPosition = EstimateGhostPosition(item.g, item.i, state.level);
                return PathAlgorithms.GetManhattanDistance(cell.Location, ghostPosition);
            });
        }
    }

    private Point EstimateGhostPosition(Point current, int index, Level level)
    {
        if (_previousState is null) return current;

        var dx = current.X - _previousState.ghosts[index].X;
        var dy = current.Y - _previousState.ghosts[index].Y;

        for (int shift = GhostInertia; shift >= 0; shift--)
        {
            var candidate = new Point(current.X + shift * dx, current.Y + shift * dy);
            if (!level.IsWall(candidate))
                return candidate;
        }
        return current;
    }

    private List<Cell> FindBestPath(StateSnapshot state)
    {
        var maxValue = _allCells!.Values.Max(c => c.Value);
        var candidates = _allCells.Values.Where(c => c.Value == maxValue).ToArray();

        var ghosts = new HashSet<Point>(state.ghosts);

        var paths = candidates
            .Select(c =>
            {
                var current = _allCells[(state.player.X, state.player.Y)];
                return PathAlgorithms.FindAPath(current, c, ghosts);
            })
            .Where(p => p.Any())
            .OrderByDescending(p => GetPathPriority(p[0].Location, ghosts, state))
            .ThenBy(p => p.Count)
            .ToArray();
        
        return paths.FirstOrDefault() ?? new List<Cell>();
    }

    private int GetPathPriority(Point location, HashSet<Point> ghosts, StateSnapshot state)
    {
        return ghosts
            .Select((g,i) => EstimateGhostPosition(g, i, state.level))
            .Where(g => PathAlgorithms.GetManhattanDistance(location, g) < CloseDistance)
            .Sum(g => PathAlgorithms.GetManhattanDistance(location, g));
    }


    private Point FindTargetMove(StateSnapshot state, IReadOnlyList<Cell> path)
    {
        if (path.Count <= 1)
            return Moves.All.FirstOrDefault(m => _allCells
                !.ContainsKey((state.player.X + m.X, state.player.Y + m.Y)));

        var nextLocation = path[0].Location;
        foreach (var move in Moves.All)
        {
            var candidate = new Point(state.player.X + move.X, state.player.Y + move.Y);
            if (candidate.Equals(nextLocation) 
                && _allCells!.ContainsKey((candidate.X, candidate.Y)))
                return move;
        }

        return Moves.All.FirstOrDefault(m => _allCells
            !.ContainsKey((state.player.X + m.X, state.player.Y + m.Y)));
    }

    private void UpdatePlayersState(StateSnapshot state, Point targetMove)
    {
        _previousState = state;
        _tracer.Register(state.player, targetMove);
    }
}