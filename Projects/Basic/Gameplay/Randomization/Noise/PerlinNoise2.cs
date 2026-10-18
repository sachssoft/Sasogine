using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Produces normalized 2D gradient noise using eight gradient directions.</summary>
/// <remarks>
/// This implementation uses seeded coordinate hashing instead of a repeating
/// permutation table. Integer lattice points return 0.5; use fractional coordinates
/// to sample detail. Instances are immutable and may be shared between threads.
/// </remarks>
public sealed class PerlinNoise2 : INoise2
{
    private const float InverseSqrtTwo = 0.7071067811865475f;

    /// <summary>Initializes the noise with the specified 64-bit seed.</summary>
    /// <param name="seed">The seed used to choose lattice gradients.</param>
    public PerlinNoise2(ulong seed)
    {
        Seed = seed;
    }

    /// <summary>Gets the seed used to choose lattice gradients.</summary>
    public ulong Seed { get; }

    /// <inheritdoc />
    public float GetValue(float x, float y)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        NoiseMath.ValidateCoordinate(y, nameof(y));

        int left = (int)MathF.Floor(x);
        int top = (int)MathF.Floor(y);
        float offsetX = x - left;
        float offsetY = y - top;
        float horizontal = NoiseMath.Fade(offsetX);
        float vertical = NoiseMath.Fade(offsetY);

        float first = NoiseMath.Lerp(
            Gradient(left, top, offsetX, offsetY),
            Gradient(left + 1, top, offsetX - 1f, offsetY),
            horizontal);

        float second = NoiseMath.Lerp(
            Gradient(left, top + 1, offsetX, offsetY - 1f),
            Gradient(left + 1, top + 1, offsetX - 1f, offsetY - 1f),
            horizontal);

        float value = NoiseMath.Lerp(first, second, vertical);
        return NoiseMath.Clamp01(0.5f + value * InverseSqrtTwo);
    }

    private float Gradient(int x, int y, float offsetX, float offsetY)
    {
        return (RandomHash.Lattice(Seed, x, y) & 7UL) switch
        {
            0 => offsetX,
            1 => -offsetX,
            2 => offsetY,
            3 => -offsetY,
            4 => (offsetX + offsetY) * InverseSqrtTwo,
            5 => (offsetX - offsetY) * InverseSqrtTwo,
            6 => (-offsetX + offsetY) * InverseSqrtTwo,
            _ => (-offsetX - offsetY) * InverseSqrtTwo
        };
    }
}
