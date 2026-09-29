namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Provides the base implementation for curves that map normalized
/// input to a modified value.
/// </summary>
public abstract class CurveBase
{
    /// <summary>
    /// Calculates the curve value for the specified normalized input.
    /// </summary>
    /// <param name="value">
    /// The normalized input, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the curve.</returns>
    public abstract float GetValue(float value);
}
