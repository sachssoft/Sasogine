namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a three-dimensional transform position.
/// </summary>
public interface IReadOnlyTransformPosition3 : ITransform3
{
    /// <summary>
    /// Gets the three-dimensional transform position.
    /// </summary>
    Point3 Position { get; }
}
