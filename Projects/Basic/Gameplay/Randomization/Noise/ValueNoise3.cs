using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Interpolates seeded lattice values to produce smooth immutable 3D noise.</summary>
public sealed class ValueNoise3 : INoise3
{
    /// <summary>Creates value noise from a 64-bit seed.</summary>
    public ValueNoise3(ulong seed) { Seed = seed; }
    /// <summary>Gets the lattice seed.</summary>
    public ulong Seed { get; }
    /// <inheritdoc />
    public float GetValue(float x, float y, float z)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        NoiseMath.ValidateCoordinate(y, nameof(y));
        NoiseMath.ValidateCoordinate(z, nameof(z));
        int left = (int)MathF.Floor(x), top = (int)MathF.Floor(y), back = (int)MathF.Floor(z);
        float horizontal = NoiseMath.Fade(x - left), vertical = NoiseMath.Fade(y - top), depth = NoiseMath.Fade(z - back);
        float frontTop = NoiseMath.Lerp(Lattice(left, top, back), Lattice(left + 1, top, back), horizontal);
        float frontBottom = NoiseMath.Lerp(Lattice(left, top + 1, back), Lattice(left + 1, top + 1, back), horizontal);
        float backTop = NoiseMath.Lerp(Lattice(left, top, back + 1), Lattice(left + 1, top, back + 1), horizontal);
        float backBottom = NoiseMath.Lerp(Lattice(left, top + 1, back + 1), Lattice(left + 1, top + 1, back + 1), horizontal);
        return NoiseMath.Clamp01(NoiseMath.Lerp(NoiseMath.Lerp(frontTop, frontBottom, vertical), NoiseMath.Lerp(backTop, backBottom, vertical), depth));
    }
    private float Lattice(int x, int y, int z) => (RandomHash.Lattice(Seed, x, y, z) >> 40) * (1f / 16_777_216f);
}
