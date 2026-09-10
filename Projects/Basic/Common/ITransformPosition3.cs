namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-write access to a three-dimensional transform position.
/// </summary>
public interface ITransformPosition3 : IReadOnlyTransformPosition3
{
    /// <summary>
    /// Gets or sets the three-dimensional transform position.
    /// </summary>
    new Point3 Position { get; set; }
}
