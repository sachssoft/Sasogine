namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a back curve that accelerates from the start with an initial overshoot in the opposite direction.
/// </summary>
public sealed class BackInCurve : CurveBase
{
    /// <summary>
    /// Calculates the curve value for the specified normalized input.
    /// </summary>
    /// <param name="value">
    /// The normalized input, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the curve.</returns>
    public override float GetValue(float value)
    {
        const float s = 1.70158f;
        return value * value * ((s + 1) * value - s);
    }
}