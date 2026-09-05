using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Defines a rectangular pixel region inside a texture resource.
/// </summary>
public interface ISourceRegion
{
    /// <summary>
    /// Gets the pixel bounds that identify this region within the source texture.
    /// </summary>
    PixelBounds2 SourceBounds { get; }
}