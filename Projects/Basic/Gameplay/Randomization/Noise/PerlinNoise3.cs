using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Produces immutable 3D gradient noise using twelve unit gradient directions.</summary>
/// <remarks>Integer lattice coordinates return 0.5. Normalization uses a conservative dimension bound.</remarks>
public sealed class PerlinNoise3 : INoise3
{
    private const float InverseSqrtTwo = 0.7071067811865475f;
    private const float HalfInverseSqrtThree = 0.2886751345948129f;
    /// <summary>Creates gradient noise from a 64-bit seed.</summary>
    public PerlinNoise3(ulong seed) { Seed = seed; }
    /// <summary>Gets the gradient seed.</summary>
    public ulong Seed { get; }
    /// <inheritdoc />
    public float GetValue(float x, float y, float z)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        NoiseMath.ValidateCoordinate(y, nameof(y));
        NoiseMath.ValidateCoordinate(z, nameof(z));
        int left = (int)MathF.Floor(x), top = (int)MathF.Floor(y), back = (int)MathF.Floor(z);
        float dx = x - left, dy = y - top, dz = z - back;
        float u = NoiseMath.Fade(dx), v = NoiseMath.Fade(dy), w = NoiseMath.Fade(dz);
        float first = NoiseMath.Lerp(Gradient(left, top, back, dx, dy, dz), Gradient(left + 1, top, back, dx - 1f, dy, dz), u);
        float second = NoiseMath.Lerp(Gradient(left, top + 1, back, dx, dy - 1f, dz), Gradient(left + 1, top + 1, back, dx - 1f, dy - 1f, dz), u);
        float third = NoiseMath.Lerp(Gradient(left, top, back + 1, dx, dy, dz - 1f), Gradient(left + 1, top, back + 1, dx - 1f, dy, dz - 1f), u);
        float fourth = NoiseMath.Lerp(Gradient(left, top + 1, back + 1, dx, dy - 1f, dz - 1f), Gradient(left + 1, top + 1, back + 1, dx - 1f, dy - 1f, dz - 1f), u);
        float value = NoiseMath.Lerp(NoiseMath.Lerp(first, second, v), NoiseMath.Lerp(third, fourth, v), w);
        return NoiseMath.Clamp01(0.5f + value * HalfInverseSqrtThree);
    }
    private float Gradient(int x, int y, int z, float dx, float dy, float dz)
    {
        float value = (RandomHash.Lattice(Seed, x, y, z) % 12UL) switch
        {
            0 => dx + dy, 1 => -dx + dy, 2 => dx - dy, 3 => -dx - dy,
            4 => dx + dz, 5 => -dx + dz, 6 => dx - dz, 7 => -dx - dz,
            8 => dy + dz, 9 => -dy + dz, 10 => dy - dz, _ => -dy - dz
        };
        return value * InverseSqrtTwo;
    }
}
