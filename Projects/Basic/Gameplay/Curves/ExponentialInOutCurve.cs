namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies an exponential curve that accelerates at the beginning and decelerates toward the end.
/// </summary>
public sealed class ExponentialInOutCurve : CurveBase
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
        if (value == 0) return 0;
        if (value == 1) return 1;
        if (value < 0.5f)
            return 0.5f * float.Pow(2, 20 * value - 10);
        return 1 - 0.5f * float.Pow(2, -20 * value + 10);
    }
}