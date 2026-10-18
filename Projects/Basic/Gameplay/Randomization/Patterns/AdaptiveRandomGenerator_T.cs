using Sachssoft.Engine.Gameplay.Randomization.Generators;
using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Gameplay.Randomization.Patterns;

/// <summary>Increases missed values' relative weights and resets selected values to their base weights.</summary>
/// <typeparam name="T">The non-null, unique value type.</typeparam>
/// <remarks>This relative-weight pity model increases chances; it does not guarantee a drop by a fixed draw.</remarks>
public sealed class AdaptiveRandomGenerator<T> : IRandomPattern<T> where T : notnull
{
    private readonly AdaptiveChoice<T>[] _choices;
    private readonly double[] _weights;
    private readonly IEqualityComparer<T> _comparer;

    /// <summary>Creates an adaptive model with an independent seeded stream.</summary>
    public AdaptiveRandomGenerator(ulong seed, IEnumerable<AdaptiveChoice<T>> choices, IEqualityComparer<T>? comparer = null)
        : this(new RandomGenerator(seed), choices, comparer) { }
    /// <summary>Creates an adaptive model from copied configuration.</summary>
    public AdaptiveRandomGenerator(IRandomGenerator random, IEnumerable<AdaptiveChoice<T>> choices, IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(choices);
        _comparer = comparer ?? EqualityComparer<T>.Default;
        var seen = new HashSet<T>(_comparer);
        var items = new List<AdaptiveChoice<T>>();
        foreach (AdaptiveChoice<T> choice in choices)
        {
            if (choice.Value is null || !seen.Add(choice.Value))
                throw new ArgumentException("Values must be non-null and unique.", nameof(choices));
            // The default struct is valid only if the complete table remains selectable.
            if (!double.IsFinite(choice.Weight) || !double.IsFinite(choice.FailureIncrease) || !double.IsFinite(choice.MaximumWeight) || choice.Weight < 0d || choice.FailureIncrease < 0d || choice.MaximumWeight < choice.Weight)
                throw new ArgumentException("Invalid adaptive choice.", nameof(choices));
            items.Add(choice);
        }
        _choices = items.ToArray();
        _weights = new double[_choices.Length];
        for (int index = 0; index < _weights.Length; index++) _weights[index] = _choices[index].Weight;
        WeightedSelection.Validate(_weights);
        Random = random;
    }
    /// <inheritdoc />
    public IRandomGenerator Random { get; }
    /// <inheritdoc />
    public T Next()
    {
        int selected = WeightedSelection.Choose(Random, _weights);
        for (int index = 0; index < _weights.Length; index++)
        {
            AdaptiveChoice<T> choice = _choices[index];
            _weights[index] = index == selected ? choice.Weight : Math.Min(choice.MaximumWeight, _weights[index] + choice.FailureIncrease);
        }
        return _choices[selected].Value;
    }
    /// <summary>Gets a value's current relative weight.</summary>
    public double GetWeight(T value)
    {
        for (int index = 0; index < _choices.Length; index++)
            if (_comparer.Equals(value, _choices[index].Value)) return _weights[index];
        throw new KeyNotFoundException("The value is not registered.");
    }
    /// <summary>Restores base weights without restarting the random stream.</summary>
    public void Reset()
    {
        for (int index = 0; index < _weights.Length; index++) _weights[index] = _choices[index].Weight;
    }
}
