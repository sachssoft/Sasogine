namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a quartic curve that accelerates at the beginning and decelerates toward the end.
/// </summary>
public sealed class QuarticInOutCurve : CurveBase
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
        if (value < 0.5f)
            return 8 * value * value * value * value;
        value--;
        return 1 - 8 * value * value * value * value;
    }
}