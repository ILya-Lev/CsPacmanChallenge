using System.Drawing;
using PacmanBot;
using Xunit.Abstractions;

namespace CsPacman.Tests;

public class FindPathTests
{
    private readonly ITestOutputHelper _output;

    public FindPathTests(ITestOutputHelper output) => _output = output;

    private const int Height = 28;
    private const int Width = 32;

    [Fact]
    public void SquareGrid_NoObstacles_MainDiagonal()
    {
        var cells = new Dictionary<Point, Cell>();
        for (int row = 0; row < Height; row++)
        for (int col = 0; col < Width; col++)
        {
            var c = new Cell() { Location = new Point(row, col), };
            cells.Add(c.Location, c);
        }

        var moves = new (int X, int Y)[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
        foreach (var cell in cells.Values)
        foreach (var move in moves)
        {
            var position = new Point(cell.Location.X + move.X, cell.Location.Y + move.Y);
            if (cells.TryGetValue(position, out var neighbor))
                cell.RegisterNeighbor(neighbor);
        }

        var start = cells[new Point(0, 0)];
        var end = cells[new Point(Height - 1, Width - 1)];
        
        var path = PathAlgorithms.FindAPath(start, end, new HashSet<Point>());

        foreach (var cell in path)
        {
            _output.WriteLine(cell.Location.ToString());
        }
    }
}