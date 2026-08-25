using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Defines a rectangular region inside a texture resource.
/// </summary>
public interface ISourceRegion
{
    /// <summary>
    /// Gets the pixel rectangle that identifies this region within the source texture.
    /// </summary>
    Rectangle SourceRectangle { get; }
}