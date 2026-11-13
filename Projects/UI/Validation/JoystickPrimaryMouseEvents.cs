using Microsoft.Xna.Framework;
using Sachssoft.Engine.Services.Platform;

/// <summary>Replays native mouse edges for the joystick sampler regression.</summary>
internal sealed class JoystickPrimaryMouseEvents : IPrimaryMouseEvents
{
    private readonly Queue<(Point Position, bool Pressed)> _edges;

    /// <summary>Stores the edges in arrival order.</summary>
    /// <param name="edges">The coordinates and state for each button edge.</param>
    internal JoystickPrimaryMouseEvents(params (Point Position, bool Pressed)[] edges)
    {
        _edges = new Queue<(Point, bool)>(edges);
    }

    /// <inheritdoc/>
    public bool TryDequeuePrimaryMouseEvent(out Point position, out bool pressed)
    {
        bool found = _edges.TryDequeue(out var edge);
        (position, pressed) = edge;
        return found;
    }
}
