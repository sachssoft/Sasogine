namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>
/// Defines a generator that produces random integer values within
/// a configurable range using a specified randomization seed.
/// </summary>
public interface IRandomGenerator
{
    /// <summary>
    /// Gets the minimum value that can be produced by the generator.
    /// </summary>
    int Minimum { get; init; }

    /// <summary>
    /// Gets the maximum value that can be produced by the generator.
    /// </summary>
    int Maximum { get; init; }

    /// <summary>
    /// Gets the seed used to initialize the random number generation
    /// sequence and provide reproducible results.
    /// </summary>
    int Seed { get; init; }

    /// <summary>
    /// Generates the next random integer value according to the
    /// generator's configured range and distribution.
    /// </summary>
    /// <returns>
    /// The generated random integer value.
    /// </returns>
    int Generate();
}