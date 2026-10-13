using Sachssoft.Engine.Geometry;

namespace Sachssoft.Engine.Graphics.Rendering;

/// <summary>
/// Extends shader functionality with support for rendering a specific texture region.
/// </summary>
public interface IShaderTextureRegion : IShader
{
    /// <summary>
    /// Gets or sets the texture region used for rendering.
    /// If null, the entire texture is used.
    /// </summary>
    PixelBounds2? TextureRegion { get; set; }
}