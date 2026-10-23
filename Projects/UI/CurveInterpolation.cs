namespace Sachssoft.Engine.Controls;
/// <summary>
/// Interpolation used by an editable normalized-time curve.
/// </summary>
public enum CurveInterpolation
{
    /// <summary>
    /// Holds each point value until the next point.
    /// </summary>
    Step,
    /// <summary>
    /// Connects neighboring points with straight segments.
    /// </summary>
    Linear,
    /// <summary>
    /// Shape-preserving monotone cubic Hermite interpolation.
    /// </summary>
    Monotone
}
