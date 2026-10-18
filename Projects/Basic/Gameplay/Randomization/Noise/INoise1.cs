namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Provides normalized coordinate-dependent noise in 1 dimension(s).</summary>
/// <remarks>Values are in [0, 1]. Coordinates must be finite and in [int.MinValue, int.MaxValue).</remarks>
public interface INoise1
{
    /// <summary>Samples the noise at the specified coordinates.</summary>
    /// <exception cref="System.ArgumentOutOfRangeException">A coordinate is outside the supported range.</exception>
    float GetValue(float x);
}
