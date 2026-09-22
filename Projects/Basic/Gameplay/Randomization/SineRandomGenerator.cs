using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>
/// Generates random integer values using a cosine-based weighting distribution
/// centered around a configurable peak value.
/// </summary>
/// <remarks>
/// Values near <see cref="Peak"/> receive a higher probability, while values
/// farther from the peak receive progressively lower weights.
/// </remarks>
public class SineRandomGenerator : IPeakedRandomGenerator
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
    /// Gets the value around which generated values are preferentially concentrated.
    /// </summary>
    public int Peak { get; init; }

    /// <summary>
    /// Generates a random integer using a cosine-based weighting distribution
    /// centered around <see cref="Peak"/>.
    /// </summary>
    /// <returns>The generated random integer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="Minimum"/> is greater than or equal to
    /// <see cref="Maximum"/>.
    /// </exception>
    public int Generate()
    {
        if (Minimum >= Maximum)
            throw new ArgumentException("Minimum must be less than maximum.");

        int range = Maximum - Minimum;
        int count = range + 1;

        var random = new Random(Seed);

        // Erzeuge Gewichtungen basierend auf einer Sinus-Kurve
        List<(int value, float weight)> weighted = new();

        for (int i = 0; i < count; i++)
        {
            int value = Minimum + i;

            // Verschiebung der Sinus-Kurve zum gewünschten Peak
            float shifted_x = (float)(value - Peak) / range * float.Pi;
            float weight = float.Cos(shifted_x); // Peak = max bei cos(0)

            if (weight < 0)
                weight = 0;

            weighted.Add((value, weight));
        }

        // Normalisiere die Gewichte
        float total_weight = weighted.Sum(w => w.weight);
        float rand = (float)random.NextDouble() * total_weight;

        // Wähle basierend auf Gewichtung
        foreach (var (value, weight) in weighted)
        {
            if (rand < weight)
                return value;

            rand -= weight;
        }

        return Maximum; // Fallback
    }
}