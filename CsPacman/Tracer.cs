namespace CsPacman;

public interface ITracer
{
    void Register(Point currentPosition, Point targetMove);
}

internal class Tracer : ITracer
{
    private readonly List<string> _history = new ();

    public void Register(Point currentPosition, Point targetMove)
    {
        _history.Add($"current ({currentPosition.X}, {currentPosition.Y}), step ({targetMove.X}, {targetMove.Y})");
    }
}