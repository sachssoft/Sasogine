using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Produces normalized seeded gradient noise in one dimension.</summary>
/// <remarks>Integer lattice coordinates return 0.5. Use fractional coordinates to sample detail.</remarks>
public sealed class PerlinNoise1 : INoise1
{
    /// <summary>Creates immutable gradient noise from a 64-bit seed.</summary>
    public PerlinNoise1(ulong seed) { Seed = seed; }
    /// <summary>Gets the gradient seed.</summary>
    public ulong Seed { get; }
    /// <inheritdoc />
    public float GetValue(float x)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        int cell = (int)MathF.Floor(x);
        float offset = x - cell;
        float first = (RandomHash.Lattice(Seed, cell) & 1UL) == 0 ? offset : -offset;
        float second = (RandomHash.Lattice(Seed, cell + 1) & 1UL) == 0 ? offset - 1f : 1f - offset;
        return NoiseMath.Clamp01(0.5f + NoiseMath.Lerp(first, second, NoiseMath.Fade(offset)));
    }
}
