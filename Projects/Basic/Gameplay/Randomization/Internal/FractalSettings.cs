using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Internal;

internal sealed class FractalSettings
{
    internal readonly double[] Frequencies;
    internal readonly double[] Amplitudes;
    internal readonly double TotalAmplitude;
    internal readonly int Octaves;
    internal readonly float Frequency, Lacunarity, Persistence;

    internal FractalSettings(int octaves, float frequency, float lacunarity, float persistence)
    {
        if (octaves < 1 || octaves > 32) throw new ArgumentOutOfRangeException(nameof(octaves));
        if (!float.IsFinite(frequency) || frequency <= 0f) throw new ArgumentOutOfRangeException(nameof(frequency));
        if (!float.IsFinite(lacunarity) || lacunarity <= 0f) throw new ArgumentOutOfRangeException(nameof(lacunarity));
        if (!float.IsFinite(persistence) || persistence < 0f || persistence > 1f) throw new ArgumentOutOfRangeException(nameof(persistence));
        Octaves = octaves; Frequency = frequency; Lacunarity = lacunarity; Persistence = persistence;
        int count = persistence == 0f ? 1 : octaves;
        Frequencies = new double[count]; Amplitudes = new double[count];
        double currentFrequency = frequency, amplitude = 1d;
        for (int index = 0; index < count; index++)
        {
            if (!double.IsFinite(currentFrequency) || currentFrequency < float.Epsilon || currentFrequency > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(lacunarity), "Octave frequencies must remain in the positive finite float range.");
            Frequencies[index] = currentFrequency; Amplitudes[index] = amplitude; TotalAmplitude += amplitude;
            currentFrequency *= lacunarity; amplitude *= persistence;
        }
    }
    internal static void ValidateSample(float value)
    {
        if (!float.IsFinite(value) || value < 0f || value > 1f)
            throw new InvalidOperationException("The noise source must return a finite value within [0, 1].");
    }
}
