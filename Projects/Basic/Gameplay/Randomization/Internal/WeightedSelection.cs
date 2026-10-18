using Sachssoft.Engine.Gameplay.Randomization;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Gameplay.Randomization.Internal;

internal static class WeightedSelection
{
    internal static void Validate(ReadOnlySpan<double> weights)
    {
        bool positive = false;
        foreach (double weight in weights)
        {
            if (!double.IsFinite(weight) || weight < 0d)
                throw new ArgumentException("Weights must be finite and nonnegative.", nameof(weights));
            positive |= weight > 0d;
        }
        if (!positive)
            throw new ArgumentException("At least one weight must be positive.", nameof(weights));
    }

    internal static int Choose(IRandomGenerator random, ReadOnlySpan<double> weights)
    {
        double maximum = 0d;
        foreach (double weight in weights)
        {
            if (!double.IsFinite(weight) || weight < 0d)
                throw new InvalidOperationException("Computed weights must be finite and nonnegative.");
            maximum = Math.Max(maximum, weight);
        }
        if (maximum == 0d)
            throw new InvalidOperationException("The current rules leave no selectable value.");

        double total = 0d;
        foreach (double weight in weights) total += weight / maximum;
        double unit = (random.NextUInt64() >> 11) * (1d / 9_007_199_254_740_992d);
        double target = Math.Min(unit * total, double.BitDecrement(total));
        int lastPositive = -1;
        for (int index = 0; index < weights.Length; index++)
        {
            double weight = weights[index] / maximum;
            if (weight <= 0d) continue;
            lastPositive = index;
            if (target < weight) return index;
            target -= weight;
        }
        return lastPositive;
    }
}

internal sealed class WeightedChoices<T>
{
    internal readonly T[] Values;
    internal readonly double[] Weights;

    internal WeightedChoices(IEnumerable<WeightedValue<T>> choices, bool unique = false, IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(choices);
        var values = new List<T>();
        var weights = new List<double>();
        var seen = unique ? new HashSet<T>(comparer) : null;
        foreach (WeightedValue<T> choice in choices)
        {
            if (seen is not null && (choice.Value is null || !seen.Add(choice.Value)))
                throw new ArgumentException("Pattern values must be non-null and unique.", nameof(choices));
            values.Add(choice.Value);
            weights.Add(choice.Weight);
        }
        Values = values.ToArray();
        Weights = weights.ToArray();
        WeightedSelection.Validate(Weights);
    }
}
