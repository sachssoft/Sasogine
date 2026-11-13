using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Services.Platform;

/// <summary>Supplies ordered primary mouse button edges from the native game window.</summary>
public interface IPrimaryMouseEvents
{
    /// <summary>Consumes the oldest pending primary button edge on the game thread.</summary>
    /// <param name="position">The native client position at the time of the event.</param>
    /// <param name="pressed">Whether this edge pressed the primary button.</param>
    /// <returns>Whether an edge was available.</returns>
    bool TryDequeuePrimaryMouseEvent(out Point position, out bool pressed);
}
