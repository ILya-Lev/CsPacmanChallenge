namespace CsPacman.Bot;

public record Cell
{
    public Point Location { get; init; }
    public int Value { get; set; }
    
    private readonly List<Cell> _neighbors = new();
    public IReadOnlyList<Cell> Neighbors => _neighbors;
    public void RegisterNeighbor(Cell neighbor) => _neighbors.Add(neighbor);
}