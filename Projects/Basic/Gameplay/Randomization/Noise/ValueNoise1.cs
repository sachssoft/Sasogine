using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Produces smooth seeded value noise in one dimension.</summary>
public sealed class ValueNoise1 : INoise1
{
    /// <summary>Creates immutable value noise from a 64-bit seed.</summary>
    public ValueNoise1(ulong seed) { Seed = seed; }
    /// <summary>Gets the lattice seed.</summary>
    public ulong Seed { get; }
    /// <inheritdoc />
    public float GetValue(float x)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        int cell = (int)MathF.Floor(x);
        float first = (RandomHash.Lattice(Seed, cell) >> 40) * (1f / 16_777_216f);
        float second = (RandomHash.Lattice(Seed, cell + 1) >> 40) * (1f / 16_777_216f);
        return NoiseMath.Clamp01(NoiseMath.Lerp(first, second, NoiseMath.Fade(x - cell)));
    }
}
