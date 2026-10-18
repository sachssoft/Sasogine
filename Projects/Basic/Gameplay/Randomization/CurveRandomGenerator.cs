using Sachssoft.Engine.Gameplay.Curves;
using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Shapes scalar random values using an existing gameplay curve.</summary>
/// <remarks>
/// The curve transforms uniformly sampled values; it is not interpreted as a
/// probability density. NextUInt64 forwards the underlying raw stream unchanged.
/// Instances share their underlying generator's mutable stream and are not thread-safe.
/// </remarks>
public sealed class CurveRandomGenerator : IRandomGenerator
{
    /// <summary>Creates a curve-shaped stream from a 64-bit seed.</summary>
    public CurveRandomGenerator(ulong seed, CurveBase curve, bool clampOutput = true)
        : this(new RandomGenerator(seed), curve, clampOutput) { }

    /// <summary>Wraps a random stream; out-of-range curve results are clamped or rejected.</summary>
    /// <exception cref="ArgumentNullException">The stream or curve is null.</exception>
    public CurveRandomGenerator(IRandomGenerator random, CurveBase curve, bool clampOutput = true)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(curve);
        Random = random;
        Curve = curve;
        ClampOutput = clampOutput;
    }

    /// <summary>Gets the underlying random stream.</summary>
    public IRandomGenerator Random { get; }
    /// <summary>Gets the curve applied to scalar draws.</summary>
    public CurveBase Curve { get; }
    /// <summary>Gets whether finite curve overshoot is clamped; otherwise it is rejected.</summary>
    public bool ClampOutput { get; }
    /// <inheritdoc />
    public ulong Seed => Random.Seed;
    /// <summary>Forwards an unshaped raw value from the underlying stream.</summary>
    public ulong NextUInt64() => Random.NextUInt64();

    /// <inheritdoc />
    public int Next(int minimum, int maximum)
    {
        if (minimum > maximum)
            throw new ArgumentOutOfRangeException(nameof(maximum));
        if (minimum == maximum) return minimum;
        double value = RandomMath.Shape(Random.NextSingle(), Curve, ClampOutput);
        long mapped = minimum + (long)(value * ((long)maximum - minimum));
        return (int)Math.Min(mapped, (long)maximum - 1);
    }

    /// <inheritdoc />
    public float NextSingle()
    {
        float value = RandomMath.Shape(Random.NextSingle(), Curve, ClampOutput);
        return value < 1f ? value : float.BitDecrement(1f);
    }

    /// <inheritdoc />
    public float NextSingle(float minimum, float maximum)
    {
        RandomMath.ValidateRange(minimum, maximum);
        if (minimum == maximum) return minimum;
        return RandomMath.Map(RandomMath.Shape(Random.NextSingle(), Curve, ClampOutput), minimum, maximum);
    }
}
