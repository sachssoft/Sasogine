namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Specifies the texture filtering method used when sampling
/// a two-dimensional texture.
/// </summary>
public enum Texture2DFilterMode
{
    /// <summary>
    /// Uses point sampling, selecting the nearest texture pixel
    /// without interpolation.
    /// </summary>
    Point = 0,

    /// <summary>
    /// Uses linear filtering to interpolate between neighboring
    /// texture pixels.
    /// </summary>
    Linear = 1,

    /// <summary>
    /// Uses anisotropic filtering to improve texture quality,
    /// particularly when textures are viewed at oblique angles.
    /// </summary>
    Anisotropic = 2
}