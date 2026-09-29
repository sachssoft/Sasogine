namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a bounce curve that produces bouncing behavior at both the beginning and end.
/// </summary>
public sealed class BounceInOutCurve : CurveBase
{
    private readonly BounceInCurve _in = new();
    private readonly BounceOutCurve _out = new();

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
            return 0.5f * _in.GetValue(value * 2);
        return 0.5f * _out.GetValue(value * 2 - 1) + 0.5f;
    }
}