using System;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Restores the previous clip without allocating. Keep scopes nested and do not copy an active scope.
/// </summary>
public struct ControlClipScope : IDisposable
{
    private ControlDrawContext? _owner;
    private readonly Bounds2 _previous;
    private readonly int _previousPlanes;
    internal ControlClipScope(ControlDrawContext owner, Bounds2 previous, int previousPlanes)
    {
        _owner = owner;
        _previous = previous;
        _previousPlanes = previousPlanes;
    }

    /// <summary>
    /// Restores the owning context's previous clip once for this scope value.
    /// </summary>
    public void Dispose()
    {
        if (_owner is not ControlDrawContext owner)
            return;
        _owner = null;
        owner.RestoreClip(_previous, _previousPlanes);
    }
}
