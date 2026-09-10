namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-write access to a two-dimensional transform position.
/// </summary>
public interface ITransformPosition2 : IReadOnlyTransformPosition2
{
    /// <summary>
    /// Gets or sets the two-dimensional transform position.
    /// </summary>
    new Point2 Position { get; set; }
}
