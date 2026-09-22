namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Specifies the blending mode used when rendering a two-dimensional texture.
/// </summary>
public enum Texture2DBlendMode
{
    /// <summary>
    /// Uses standard premultiplied alpha blending, commonly used
    /// for transparent textures.
    /// </summary>
    AlphaBlend,

    /// <summary>
    /// Uses additive blending, where source and destination colors
    /// are added together. Commonly used for light and glow effects.
    /// </summary>
    Additive,

    /// <summary>
    /// Uses alpha blending for textures whose color values
    /// are not premultiplied by their alpha values.
    /// </summary>
    NonPremultiplied,

    /// <summary>
    /// Disables blending so that rendered texture pixels directly
    /// replace the destination pixels.
    /// </summary>
    Opaque,

    /// <summary>
    /// Uses a custom blend state supplied separately by the renderer.
    /// </summary>
    Custom
}