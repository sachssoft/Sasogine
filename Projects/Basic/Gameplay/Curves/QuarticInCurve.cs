namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a quartic curve that accelerates from the start.
/// </summary>
public sealed class QuarticInCurve : CurveBase
{
    /// <summary>
    /// Calculates the curve value for the specified normalized input.
    /// </summary>
    /// <param name="value">
    /// The normalized input, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the curve.</returns>
    public override float GetValue(float value) => value * value * value * value;
}