namespace Sachssoft.Sasogine.Graphics;

/// <summary>
/// Specifies how texture coordinates outside the standard texture range
/// are mapped when sampling a two-dimensional texture.
/// </summary>
public enum Texture2DAddressMode
{
    /// <summary>
    /// Clamps texture coordinates to the texture boundaries, causing
    /// coordinates outside the valid range to use the nearest edge value.
    /// </summary>
    Clamp,

    /// <summary>
    /// Wraps texture coordinates around the texture boundaries,
    /// causing the texture to repeat.
    /// </summary>
    Wrap,

    /// <summary>
    /// Mirrors the texture at each boundary, causing repeated copies
    /// to alternate their orientation.
    /// </summary>
    Mirror
}