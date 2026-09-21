using System;

namespace Sachssoft.Sasogine.Gameplay.Randomization;

/// <summary>
/// Provides utility methods for generating random integer values using
/// configurable random generator implementations.
/// </summary>
public static class RandomUtil
{
    /// <summary>
    /// Generates a random integer within the specified range using
    /// <see cref="SystemRandomGenerator"/>.
    /// </summary>
    /// <param name="minimum">The minimum value of the generation range.</param>
    /// <param name="maximum">The maximum value of the generation range.</param>
    /// <returns>The generated random integer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="minimum"/> is greater than
    /// <paramref name="maximum"/>.
    /// </exception>
    public static int Generate(int minimum, int maximum)
    {
        return Generate<SystemRandomGenerator>(minimum, maximum);
    }

    /// <summary>
    /// Generates a random integer within the specified range using
    /// <see cref="SystemRandomGenerator"/> and the specified seed.
    /// </summary>
    /// <param name="minimum">The minimum value of the generation range.</param>
    /// <param name="maximum">The maximum value of the generation range.</param>
    /// <param name="seed">The seed used to initialize the random generator.</param>
    /// <returns>The generated random integer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="minimum"/> is greater than
    /// <paramref name="maximum"/>.
    /// </exception>
    public static int Generate(int minimum, int maximum, int seed)
    {
        return Generate<SystemRandomGenerator>(minimum, maximum, seed);
    }

    /// <summary>
    /// Generates a random integer within the specified range using
    /// the specified random generator type and an automatically generated seed.
    /// </summary>
    /// <typeparam name="TGenerator">
    /// The type of random generator to use.
    /// </typeparam>
    /// <param name="minimum">The minimum value of the generation range.</param>
    /// <param name="maximum">The maximum value of the generation range.</param>
    /// <returns>The generated random integer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="minimum"/> is greater than
    /// <paramref name="maximum"/>.
    /// </exception>
    public static int Generate<TGenerator>(int minimum, int maximum)
        where TGenerator : class, IRandomGenerator, new()
    {
        return Generate<TGenerator>(minimum, maximum, GenerateSeed());
    }

    /// <summary>
    /// Generates a random integer within the specified range using
    /// the specified random generator type and seed.
    /// </summary>
    /// <typeparam name="TGenerator">
    /// The type of random generator to use.
    /// </typeparam>
    /// <param name="minimum">The minimum value of the generation range.</param>
    /// <param name="maximum">The maximum value of the generation range.</param>
    /// <param name="seed">The seed used to initialize the random generator.</param>
    /// <returns>The generated random integer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="minimum"/> is greater than
    /// <paramref name="maximum"/>.
    /// </exception>
    public static int Generate<TGenerator>(int minimum, int maximum, int seed)
        where TGenerator : class, IRandomGenerator, new()
    {
        if (minimum > maximum)
            throw new ArgumentException("Minimum must not be greater than maximum.");

        var generator = new TGenerator
        {
            Minimum = minimum,
            Maximum = maximum,
            Seed = seed
        };

        return generator.Generate();
    }

    /// <summary>
    /// Generates a random integer within the specified range using a
    /// peaked random generator and an automatically generated seed.
    /// </summary>
    /// <typeparam name="TGenerator">
    /// The type of peaked random generator to use.
    /// </typeparam>
    /// <param name="minimum">The minimum value of the generation range.</param>
    /// <param name="maximum">The maximum value of the generation range.</param>
    /// <param name="peak">
    /// The peak value used by the random generator.
    /// </param>
    /// <returns>The generated random integer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="minimum"/> is greater than
    /// <paramref name="maximum"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="peak"/> is outside the range defined by
    /// <paramref name="minimum"/> and <paramref name="maximum"/>.
    /// </exception>
    public static int GeneratePeaked<TGenerator>(int minimum, int maximum, int peak)
        where TGenerator : class, IPeakedRandomGenerator, new()
    {
        return GeneratePeaked<TGenerator>(minimum, maximum, peak, GenerateSeed());
    }

    /// <summary>
    /// Generates a random integer within the specified range using a
    /// peaked random generator with the specified peak and seed.
    /// </summary>
    /// <typeparam name="TGenerator">
    /// The type of peaked random generator to use.
    /// </typeparam>
    /// <param name="minimum">The minimum value of the generation range.</param>
    /// <param name="maximum">The maximum value of the generation range.</param>
    /// <param name="peak">
    /// The peak value used by the random generator.
    /// </param>
    /// <param name="seed">The seed used to initialize the random generator.</param>
    /// <returns>The generated random integer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="minimum"/> is greater than
    /// <paramref name="maximum"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="peak"/> is outside the range defined by
    /// <paramref name="minimum"/> and <paramref name="maximum"/>.
    /// </exception>
    public static int GeneratePeaked<TGenerator>(
        int minimum,
        int maximum,
        int peak,
        int seed)
        where TGenerator : class, IPeakedRandomGenerator, new()
    {
        if (minimum > maximum)
            throw new ArgumentException("Minimum must not be greater than maximum.");

        if (peak < minimum || peak > maximum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(peak),
                "Peak must be within the range of minimum and maximum.");
        }

        var generator = new TGenerator
        {
            Minimum = minimum,
            Maximum = maximum,
            Seed = seed,
            Peak = peak
        };

        return generator.Generate();
    }

    private static int GenerateSeed()
    {
        return Guid.NewGuid().GetHashCode();
    }
}