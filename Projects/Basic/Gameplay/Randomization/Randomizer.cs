using Sachssoft.Engine.Gameplay.Curves;
using System;
using System.Security.Cryptography;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Provides convenient random draws using a synchronized shared stream.</summary>
/// <remarks>
/// Generate uses inclusive integer bounds for compatibility with the old helper.
/// Next and NextSingle exclude the upper bound. Seeded Generate calls start a
/// separate stream; other calls advance the shared stream. Parallel call order
/// is unspecified. Use dedicated generators for independent reproducible streams.
/// Callbacks and supplied curves execute under the shared lock.
/// </remarks>
public static class Randomizer
{
    private static readonly object Sync = new();
    private static RandomGenerator _random = new(CreateSeed());

    /// <summary>Creates a seed using the operating system's random source.</summary>
    public static ulong CreateSeed()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        return System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    /// <summary>Restarts the shared stream. This affects every caller of this helper.</summary>
    public static void SetSeed(ulong seed)
    {
        lock (Sync) _random = new RandomGenerator(seed);
    }

    /// <summary>Draws an integer including both bounds.</summary>
    public static int Generate(int minimum, int maximum)
    {
        lock (Sync) return Inclusive(_random, minimum, maximum);
    }

    /// <summary>Draws once from a separate seeded stream, including both bounds.</summary>
    public static int Generate(int minimum, int maximum, ulong seed)
        => Inclusive(new RandomGenerator(seed), minimum, maximum);

    /// <summary>Draws once using a signed legacy seed and inclusive bounds.</summary>
    public static int Generate(int minimum, int maximum, int seed)
        => Generate(minimum, maximum, unchecked((ulong)(long)seed));

    /// <summary>Draws an integer in [minimum, maximum).</summary>
    public static int Next(int minimum, int maximum)
    {
        lock (Sync) return _random.Next(minimum, maximum);
    }

    /// <summary>Draws a raw 64-bit value.</summary>
    public static ulong NextUInt64()
    {
        lock (Sync) return _random.NextUInt64();
    }

    /// <summary>Draws a float in [0, 1).</summary>
    public static float NextSingle()
    {
        lock (Sync) return _random.NextSingle();
    }

    /// <summary>Draws a float in [minimum, maximum).</summary>
    public static float NextSingle(float minimum, float maximum)
    {
        lock (Sync) return _random.NextSingle(minimum, maximum);
    }

    /// <summary>Draws a curve-shaped float in [0, 1).</summary>
    public static float NextSingle(CurveBase curve, bool clampOutput = true)
    {
        lock (Sync) return new CurveRandomGenerator(_random, curve, clampOutput).NextSingle();
    }

    /// <summary>Draws a curve-shaped float in [minimum, maximum).</summary>
    public static float NextSingle(float minimum, float maximum, CurveBase curve, bool clampOutput = true)
    {
        lock (Sync) return new CurveRandomGenerator(_random, curve, clampOutput).NextSingle(minimum, maximum);
    }

    /// <summary>Draws a curve-shaped integer in [minimum, maximum).</summary>
    public static int Next(int minimum, int maximum, CurveBase curve, bool clampOutput = true)
    {
        lock (Sync) return new CurveRandomGenerator(_random, curve, clampOutput).Next(minimum, maximum);
    }

    /// <summary>Returns true with the supplied probability in [0, 1].</summary>
    public static bool Chance(double probability = 0.5d)
    {
        if (!double.IsFinite(probability) || probability < 0d || probability > 1d)
            throw new ArgumentOutOfRangeException(nameof(probability));
        if (probability == 0d) return false;
        if (probability == 1d) return true;
        lock (Sync) return (_random.NextUInt64() >> 11) * (1d / 9_007_199_254_740_992d) < probability;
    }

    /// <summary>Selects one item uniformly from a nonempty array.</summary>
    public static T Choose<T>(params T[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0) throw new ArgumentException("Choices must not be empty.", nameof(values));
        lock (Sync) return values[_random.Next(0, values.Length)];
    }

    /// <summary>Selects one item using relative weights. Inputs are copied for this draw.</summary>
    public static T ChooseWeighted<T>(params WeightedValue<T>[] choices)
        => Evaluate(new WeightedChoiceExpression<T>(choices));

    /// <summary>Evaluates an expression using the shared stream.</summary>
    public static T Evaluate<T>(IRandomExpression<T> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        lock (Sync) return expression.GetValue(_random);
    }

    /// <summary>Advances an existing pattern, preserving its own stream and history.</summary>
    /// <remarks>Use this helper consistently for synchronization; direct calls are not protected.</remarks>
    public static T Next<T>(IRandomPattern<T> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        lock (Sync) return pattern.Next();
    }

    private static int Inclusive(IRandomGenerator random, int minimum, int maximum)
    {
        if (minimum > maximum) throw new ArgumentOutOfRangeException(nameof(maximum));
        if (minimum == maximum) return minimum;
        if (maximum < int.MaxValue) return random.Next(minimum, maximum + 1);
        if (minimum > int.MinValue) return random.Next(minimum - 1, maximum) + 1;
        return unchecked((int)random.NextUInt64());
    }
}
