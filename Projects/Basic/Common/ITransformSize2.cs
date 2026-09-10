namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-write access to a two-dimensional transform size.
/// </summary>
public interface ITransformSize2 : IReadOnlyTransformSize2
{
    /// <summary>
    /// Gets or sets the two-dimensional transform size.
    /// </summary>
    new Size2 Size { get; set; }
}