using Sachssoft.Engine.Gameplay.Randomization.Generators;
using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Gameplay.Randomization.Patterns;

/// <summary>Computes relative weights from base weights, retained history and caller rules.</summary>
/// <typeparam name="T">The non-null, unique value type.</typeparam>
public sealed class WeightedPatternGenerator<T> : IRandomPattern<T> where T : notnull
{
    private readonly WeightedChoices<T> _choices;
    private readonly double[] _weights;
    private readonly Func<T, double, PatternContext<T>, double> _weightRule;

    /// <summary>Creates a rule-based model with an independent seeded stream.</summary>
    public WeightedPatternGenerator(ulong seed, IEnumerable<WeightedValue<T>> choices,
        Func<T, double, PatternContext<T>, double> weightRule, int historyCapacity = 16,
        IEqualityComparer<T>? comparer = null)
        : this(new RandomGenerator(seed), choices, weightRule, historyCapacity, comparer) { }

    /// <summary>Creates a model whose rule returns a final finite nonnegative weight per value.</summary>
    /// <remarks>Rules run in collection order before any random draw and should not modify the generator.</remarks>
    public WeightedPatternGenerator(IRandomGenerator random, IEnumerable<WeightedValue<T>> choices,
        Func<T, double, PatternContext<T>, double> weightRule, int historyCapacity = 16,
        IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(weightRule);
        comparer ??= EqualityComparer<T>.Default;
        Context = new PatternContext<T>(historyCapacity, comparer);
        _choices = new WeightedChoices<T>(choices, true, comparer);
        _weights = new double[_choices.Values.Length];
        Random = random;
        _weightRule = weightRule;
    }
    /// <inheritdoc />
    public IRandomGenerator Random { get; }
    /// <summary>Gets a read-only view of the selection history.</summary>
    public PatternContext<T> Context { get; }
    /// <inheritdoc />
    public T Next()
    {
        for (int index = 0; index < _weights.Length; index++)
            _weights[index] = _weightRule(_choices.Values[index], _choices.Weights[index], Context);
        T selected = _choices.Values[WeightedSelection.Choose(Random, _weights)];
        Context.Record(selected);
        return selected;
    }
    /// <summary>Clears history without restarting the random stream.</summary>
    public void Reset() => Context.Clear();
}
