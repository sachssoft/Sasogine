namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a two-dimensional transform position.
/// </summary>
public interface IReadOnlyTransformPosition2 : ITransform2
{
    /// <summary>
    /// Gets the two-dimensional transform position.
    /// </summary>
    Point2 Position { get; }
}
