using System;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>
/// Generates random integer values using the .NET
/// <see cref="Random"/> implementation.
/// </summary>
public sealed class SystemRandomGenerator : IRandomGenerator
{
    /// <summary>
    /// Gets the minimum value that can be generated.
    /// </summary>
    public int Minimum { get; init; }

    /// <summary>
    /// Gets the maximum value that can be generated.
    /// </summary>
    public int Maximum { get; init; }

    /// <summary>
    /// Gets the seed used to initialize the random number generator.
    /// </summary>
    public int Seed { get; init; }

    /// <summary>
    /// Generates a random integer within the configured range.
    /// </summary>
    /// <returns>
    /// A random integer greater than or equal to <see cref="Minimum"/>
    /// and less than or equal to <see cref="Maximum"/>.
    /// </returns>
    public int Generate()
    {
        var random = new Random(Seed);
        return random.Next(Minimum, Maximum + 1);
    }
}