namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a two-dimensional transform size.
/// </summary>
public interface IReadOnlyTransformSize2 : ITransform2
{
    /// <summary>
    /// Gets the two-dimensional transform size.
    /// </summary>
    Size2 Size { get; }
}