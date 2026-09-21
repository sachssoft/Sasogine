namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Specifies the interpolation degree used for rounding or smoothing geometry.
/// </summary>
public enum RoundingType
{
    /// <summary>
    /// Uses linear interpolation to create straight transitions.
    /// </summary>
    Linear,

    /// <summary>
    /// Uses quadratic interpolation to create curved transitions.
    /// </summary>
    Quadratic,

    /// <summary>
    /// Uses cubic interpolation to create smoother and more flexible curved transitions.
    /// </summary>
    Cubic
}