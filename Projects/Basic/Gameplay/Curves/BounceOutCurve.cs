namespace Sachssoft.Engine.Gameplay.Curves;

/// <summary>
/// Applies a bounce curve that produces bouncing behavior at the end.
/// </summary>
public sealed class BounceOutCurve : CurveBase
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
        if (value < 1 / 2.75f)
        {
            return 7.5625f * value * value;
        }
        else if (value < 2 / 2.75f)
        {
            value -= 1.5f / 2.75f;
            return 7.5625f * value * value + 0.75f;
        }
        else if (value < 2.5 / 2.75)
        {
            value -= 2.25f / 2.75f;
            return 7.5625f * value * value + 0.9375f;
        }
        else
        {
            value -= 2.625f / 2.75f;
            return 7.5625f * value * value + 0.984375f;
        }
    }
}