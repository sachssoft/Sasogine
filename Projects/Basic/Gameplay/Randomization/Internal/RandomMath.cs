using Sachssoft.Engine.Gameplay.Curves;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Internal;

internal static class RandomMath
{
    internal static void ValidateRange(float minimum, float maximum)
    {
        if (!float.IsFinite(minimum))
            throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum must be finite.");
        if (!float.IsFinite(maximum) || minimum > maximum)
            throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum must be finite and not less than minimum.");
    }

    internal static float Map(float value, float minimum, float maximum)
    {
        float mapped = (float)(minimum + ((double)maximum - minimum) * value);
        return mapped < maximum ? MathF.Max(mapped, minimum) : float.BitDecrement(maximum);
    }

    internal static float Shape(float value, CurveBase curve, bool clampOutput)
    {
        if (!float.IsFinite(value) || value < 0f || value > 1f)
            throw new InvalidOperationException("Curve input must be finite and within [0, 1].");
        float shaped = curve.GetValue(value);
        if (!float.IsFinite(shaped))
            throw new InvalidOperationException("The curve returned a non-finite value.");
        if (!clampOutput && (shaped < 0f || shaped > 1f))
            throw new InvalidOperationException("The curve returned a value outside [0, 1].");
        return Math.Clamp(shaped, 0f, 1f);
    }

    internal static double Power(double value, int exponent)
    {
        double result = 1d;
        while (exponent > 0)
        {
            if ((exponent & 1) != 0) result *= value;
            value *= value;
            exponent >>= 1;
        }
        return result;
    }
}
