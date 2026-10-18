namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>
/// Provides a reproducible stream of pseudo-random values.
/// </summary>
/// <remarks>
/// Integer and floating-point ranges include the minimum and exclude the
/// maximum. Equal bounds return that bound without advancing the stream.
/// </remarks>
public interface IRandomGenerator
{
    /// <summary>Gets the seed used to initialize the stream.</summary>
    ulong Seed { get; }

    /// <summary>Returns the next unsigned 64-bit value.</summary>
    ulong NextUInt64();

    /// <summary>Returns an integer in [minimum, maximum).</summary>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// The minimum is greater than the maximum.
    /// </exception>
    int Next(int minimum, int maximum);

    /// <summary>Returns a floating-point value in [0, 1).</summary>
    float NextSingle();

    /// <summary>Returns a floating-point value in [minimum, maximum).</summary>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// A bound is not finite, or the minimum is greater than the maximum.
    /// </exception>
    float NextSingle(float minimum, float maximum);
}
