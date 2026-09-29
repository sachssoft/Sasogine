namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a bounce curve that produces bouncing behavior at the beginning.
/// </summary>
public sealed class BounceInCurve : CurveBase
{
    private readonly BounceOutCurve _out = new();

    /// <summary>
    /// Calculates the curve value for the specified normalized input.
    /// </summary>
    /// <param name="value">
    /// The normalized input, typically between 0.0 and 1.0.
    /// </param>
    /// <returns>The value produced by the curve.</returns>
    public override float GetValue(float value) =>
        1 - _out.GetValue(1 - value);
}