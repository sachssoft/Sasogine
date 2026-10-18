using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Combines normalized 2D noise across weighted octaves.</summary>
/// <remarks>Settings are immutable. Thread safety depends on the wrapped source.</remarks>
public sealed class FractalNoise2 : INoise2
{
    private readonly FractalSettings _settings;
    /// <summary>Creates 1..32 octaves with positive finite frequencies and persistence in [0, 1].</summary>
    public FractalNoise2(INoise2 source, int octaves = 4, float frequency = 1f, float lacunarity = 2f, float persistence = 0.5f)
    {
        ArgumentNullException.ThrowIfNull(source);
        _settings = new FractalSettings(octaves, frequency, lacunarity, persistence);
        Source = source;
    }
    /// <summary>Gets the wrapped source.</summary>
    public INoise2 Source { get; }
    /// <summary>Gets the configured octave count.</summary>
    public int Octaves => _settings.Octaves;
    /// <summary>Gets the initial frequency.</summary>
    public float Frequency => _settings.Frequency;
    /// <summary>Gets the frequency multiplier.</summary>
    public float Lacunarity => _settings.Lacunarity;
    /// <summary>Gets the amplitude multiplier.</summary>
    public float Persistence => _settings.Persistence;
    /// <inheritdoc />
    public float GetValue(float x, float y)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        NoiseMath.ValidateCoordinate(y, nameof(y));
        for (int index = 0; index < _settings.Frequencies.Length; index++)
        {
            NoiseMath.ScaleCoordinate(x, _settings.Frequencies[index], nameof(x));
            NoiseMath.ScaleCoordinate(y, _settings.Frequencies[index], nameof(y));
        }
        double total = 0d;
        for (int index = 0; index < _settings.Frequencies.Length; index++)
        {
            float sample = Source.GetValue(NoiseMath.ScaleCoordinate(x, _settings.Frequencies[index], nameof(x)), NoiseMath.ScaleCoordinate(y, _settings.Frequencies[index], nameof(y)));
            FractalSettings.ValidateSample(sample);
            total += sample * _settings.Amplitudes[index];
        }
        return NoiseMath.Clamp01((float)(total / _settings.TotalAmplitude));
    }
}
