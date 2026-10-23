namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Selects the UI rendering implementation without changing control behavior.
/// </summary>
public enum ControlRenderer
{
    /// <summary>
    /// Uses Sasogine's clipped, buffered frame renderer.
    /// </summary>
    FrameBatch,
    /// <summary>
    /// Uses the original per-control SpriteBatch implementation for compatibility and comparison.
    /// </summary>
    SpriteBatch
}
