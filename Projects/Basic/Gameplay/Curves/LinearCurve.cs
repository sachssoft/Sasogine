namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies linear curve without modifying the normalized input.
/// </summary>
public sealed class LinearCurve : CurveBase
{
    /// <summary>
    /// Calculates the curve value for the specified normalized input.
    /// </summary>
    /// <param name="value">
    /// The normalized input, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the curve.</returns>
    public override float GetValue(float value) => value;
}
