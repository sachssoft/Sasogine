namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Specifies how a two-dimensional texture pattern behaves
/// when it extends beyond its boundaries.
/// </summary>
public enum Texture2DPatternMode
{
    /// <summary>
    /// Repeats the texture pattern beyond its boundaries.
    /// </summary>
    Repeat,

    /// <summary>
    /// Clamps the texture pattern to its boundary values.
    /// </summary>
    Clamp,

    /// <summary>
    /// Repeats the texture pattern while mirroring each adjacent repetition.
    /// </summary>
    Mirror
}