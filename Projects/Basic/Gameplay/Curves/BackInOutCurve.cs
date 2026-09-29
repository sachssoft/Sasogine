namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a back curve with overshoot behavior at both the beginning and end.
/// </summary>
public sealed class BackInOutCurve : CurveBase
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
        const float s = 1.70158f * 1.525f;
        value *= 2;
        if (value < 1)
            return 0.5f * (value * value * ((s + 1) * value - s));
        value -= 2;
        return 0.5f * (value * value * ((s + 1) * value + s) + 2);
    }
}