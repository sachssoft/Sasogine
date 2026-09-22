using System;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Specifies how a two-dimensional texture is flipped during rendering.
/// </summary>
[Flags]
public enum Texture2DFlipMode
{
    /// <summary>
    /// Specifies that the texture is not flipped.
    /// </summary>
    None = 0,

    /// <summary>
    /// Flips the texture horizontally.
    /// </summary>
    Horizontal = 1,

    /// <summary>
    /// Flips the texture vertically.
    /// </summary>
    Vertical = 2
}