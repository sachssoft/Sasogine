namespace Sachssoft.Engine.Controls;
/// <summary>
/// Samples a straight RGBA color at a workspace logical position on the UI thread.
/// </summary>
public interface IColorPickSource
{
    /// <summary>
    /// Attempts to sample a valid position; false leaves the current color unchanged.
    /// </summary>
    bool TryPick(Vector2 position, out Color color);
}
