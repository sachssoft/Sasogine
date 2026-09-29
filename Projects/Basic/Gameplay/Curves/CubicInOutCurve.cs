namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a cubic curve that accelerates at the beginning and decelerates toward the end.
/// </summary>
public sealed class CubicInOutCurve : CurveBase
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
        return value < 0.5f
            ? 4 * value * value * value
            : (value - 1) * (2 * value - 2) * (2 * value - 2) + 1;
    }
}