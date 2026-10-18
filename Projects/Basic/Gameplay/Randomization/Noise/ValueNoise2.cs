using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Interpolates seeded lattice values to produce smooth 2D value noise.</summary>
/// <remarks>
/// Instances are immutable and may be shared between threads. Sampling does not
/// advance a random stream and does not depend on the order of previous samples.
/// </remarks>
public sealed class ValueNoise2 : INoise2
{
    /// <summary>Initializes the noise with the specified 64-bit seed.</summary>
    /// <param name="seed">The seed used to hash lattice coordinates.</param>
    public ValueNoise2(ulong seed)
    {
        Seed = seed;
    }

    /// <summary>Gets the seed used to hash lattice coordinates.</summary>
    public ulong Seed { get; }

    /// <inheritdoc />
    public float GetValue(float x, float y)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        NoiseMath.ValidateCoordinate(y, nameof(y));

        int left = (int)MathF.Floor(x);
        int top = (int)MathF.Floor(y);
        float horizontal = NoiseMath.Fade(x - left);
        float vertical = NoiseMath.Fade(y - top);

        float first = NoiseMath.Lerp(Lattice(left, top), Lattice(left + 1, top), horizontal);
        float second = NoiseMath.Lerp(Lattice(left, top + 1), Lattice(left + 1, top + 1), horizontal);

        return NoiseMath.Clamp01(NoiseMath.Lerp(first, second, vertical));
    }

    private float Lattice(int x, int y)
    {
        return (RandomHash.Lattice(Seed, x, y) >> 40) * (1f / 16_777_216f);
    }
}
