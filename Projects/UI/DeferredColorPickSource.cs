namespace Sachssoft.Engine.Controls;
/// <summary>
/// Provides a stable pipette source while an application prepares an on-demand scene readback.
/// </summary>
/// <remarks>Assign Source after rendering the requested scene. Pending picks retry during Update.</remarks>
public sealed class DeferredColorPickSource : IColorPickSource
{
    /// <summary>
    /// Gets or sets the ready, application-owned sampling source; null defers sampling.
    /// </summary>
    public IColorPickSource? Source { get; set; }
    /// <inheritdoc/>
    public bool TryPick(Vector2 position, out Color color)
    {
        color = default;
        return Source is { } source && source.TryPick(position, out color);
    }
}
