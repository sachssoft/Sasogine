using System;

namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies an elastic curve that oscillates while accelerating from the start.
/// </summary>
public sealed class ElasticInCurve : CurveBase
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
        if (value == 0 || value == 1) return value;
        return -float.Pow(2, 10 * (value - 1)) * float.Sin((value - 1.075f) * (2 * MathF.PI) / 0.3f);
    }
}