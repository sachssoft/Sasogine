using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Generators;

/// <summary>
/// Generates deterministic pseudo-random values using SplitMix64.
/// </summary>
/// <remarks>
/// Reuse an instance to advance its stream. Creating another instance with
/// the same seed restarts the same sequence. Instances are not thread-safe.
/// This generator is intended for gameplay, not cryptographic use.
/// </remarks>
public sealed class RandomGenerator : IRandomGenerator
{
    private ulong _state;

    /// <summary>Initializes a stream with the specified seed.</summary>
    /// <param name="seed">The seed; all 64-bit values, including zero, are valid.</param>
    public RandomGenerator(ulong seed)
    {
        Seed = seed;
        _state = seed;
    }

    /// <inheritdoc />
    public ulong Seed { get; }

    /// <inheritdoc />
    public ulong NextUInt64()
    {
        _state = unchecked(_state + RandomHash.Increment);
        return RandomHash.Mix(_state);
    }

    /// <inheritdoc />
    public int Next(int minimum, int maximum)
    {
        if (minimum > maximum)
            throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum must not be less than minimum.");

        if (minimum == maximum)
            return minimum;

        ulong range = (ulong)((long)maximum - minimum);
        ulong threshold = unchecked(0UL - range) % range;
        ulong value;

        // Reject the incomplete part of the 64-bit range to avoid modulo bias.
        do
        {
            value = NextUInt64();
        }
        while (value < threshold);

        return (int)(minimum + (long)(value % range));
    }

    /// <inheritdoc />
    public float NextSingle()
    {
        return (NextUInt64() >> 40) * (1f / 16_777_216f);
    }

    /// <inheritdoc />
    public float NextSingle(float minimum, float maximum)
    {
        if (!float.IsFinite(minimum))
            throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum must be finite.");

        if (!float.IsFinite(maximum) || minimum > maximum)
            throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum must be finite and not less than minimum.");

        if (minimum == maximum)
            return minimum;

        // Double intermediates avoid overflow for ranges spanning both float extremes.
        double result = minimum + ((double)maximum - minimum) * NextSingle();
        float value = (float)result;

        // Rounding to float must not accidentally include the exclusive upper bound.
        return value < maximum ? MathF.Max(minimum, value) : float.BitDecrement(maximum);
    }
}
