namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a back curve that decelerates toward the end with an overshoot beyond the target value.
/// </summary>
public sealed class BackOutCurve : CurveBase
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
        value--;
        return value * value * ((s + 1) * value + s) + 1;
    }
}
