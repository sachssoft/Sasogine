using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Internal;

/// <summary>Provides coordinate validation and interpolation for lattice noise.</summary>
internal static class NoiseMath
{
    internal static void ValidateCoordinate(float coordinate, string parameterName)
    {
        if (!float.IsFinite(coordinate) || (double)coordinate < int.MinValue || (double)coordinate >= int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Coordinate must be finite and within [int.MinValue, int.MaxValue).");
        }
    }

    internal static float ScaleCoordinate(float coordinate, double frequency, string parameterName)
    {
        double scaled = coordinate * frequency;

        if (!double.IsFinite(scaled) || scaled < int.MinValue || scaled >= int.MaxValue)
            throw new ArgumentOutOfRangeException(parameterName, "An octave coordinate lies outside the supported range.");

        float value = (float)scaled;
        ValidateCoordinate(value, parameterName);
        return value;
    }

    internal static float Fade(float value)
    {
        return value * value * value * (value * (value * 6f - 15f) + 10f);
    }

    internal static float Lerp(float first, float second, float amount)
    {
        return first + (second - first) * amount;
    }

    internal static float Clamp01(float value)
    {
        return Math.Clamp(value, 0f, 1f);
    }
}
