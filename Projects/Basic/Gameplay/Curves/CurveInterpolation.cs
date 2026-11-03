namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Specifies how neighboring curve samples are interpolated.
/// </summary>
public enum CurveInterpolation
{
    /// <summary>
    /// Holds each sample value until the next sample.
    /// </summary>
    Step = 0,

    /// <summary>
    /// Connects neighboring samples with straight segments.
    /// </summary>
    Linear = 1,

    /// <summary>
    /// Uses shape-preserving monotone cubic Hermite interpolation.
    /// </summary>
    Monotone = 2
}
