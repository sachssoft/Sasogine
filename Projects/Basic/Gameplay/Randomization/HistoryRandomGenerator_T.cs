using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Penalizes recently selected values and enforces a consecutive-repeat limit.</summary>
/// <typeparam name="T">The non-null, unique value type.</typeparam>
/// <remarks>Contradictory rules throw instead of silently exceeding the repeat limit.</remarks>
public sealed class HistoryRandomGenerator<T> : IRandomPattern<T> where T : notnull
{
    private readonly WeightedChoices<T> _choices;
    private readonly double[] _weights;
    private readonly IEqualityComparer<T> _comparer;

    /// <summary>Creates a history model with an independent seeded stream.</summary>
    public HistoryRandomGenerator(ulong seed, IEnumerable<WeightedValue<T>> choices,
        int historyCapacity = 8, int maximumConsecutive = 2, double recentPenalty = 0.5d,
        IEqualityComparer<T>? comparer = null)
        : this(new RandomGenerator(seed), choices, historyCapacity, maximumConsecutive, recentPenalty, comparer) { }

    /// <summary>Creates a model; each recent occurrence multiplies the base weight by recentPenalty.</summary>
    public HistoryRandomGenerator(IRandomGenerator random, IEnumerable<WeightedValue<T>> choices,
        int historyCapacity = 8, int maximumConsecutive = 2, double recentPenalty = 0.5d,
        IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (maximumConsecutive < 1) throw new ArgumentOutOfRangeException(nameof(maximumConsecutive));
        if (!double.IsFinite(recentPenalty) || recentPenalty < 0d || recentPenalty > 1d)
            throw new ArgumentOutOfRangeException(nameof(recentPenalty));
        _comparer = comparer ?? EqualityComparer<T>.Default;
        Context = new PatternContext<T>(historyCapacity, _comparer);
        _choices = new WeightedChoices<T>(choices, true, _comparer);
        _weights = new double[_choices.Values.Length];
        Random = random;
        MaximumConsecutive = maximumConsecutive;
        RecentPenalty = recentPenalty;
    }
    /// <inheritdoc />
    public IRandomGenerator Random { get; }
    /// <summary>Gets the maximum allowed consecutive occurrences.</summary>
    public int MaximumConsecutive { get; }
    /// <summary>Gets the per-occurrence recent-history weight multiplier.</summary>
    public double RecentPenalty { get; }
    /// <summary>Gets a read-only view of the selection history.</summary>
    public PatternContext<T> Context { get; }
    /// <inheritdoc />
    public T Next()
    {
        for (int index = 0; index < _weights.Length; index++)
        {
            T value = _choices.Values[index];
            bool blocked = Context.HasPrevious && Context.ConsecutiveCount >= MaximumConsecutive && _comparer.Equals(Context.Previous, value);
            _weights[index] = blocked ? 0d : _choices.Weights[index] * RandomMath.Power(RecentPenalty, Context.GetRecentCount(value));
        }
        T selected = _choices.Values[WeightedSelection.Choose(Random, _weights)];
        Context.Record(selected);
        return selected;
    }
    /// <summary>Clears history without restarting the underlying random stream.</summary>
    public void Reset() => Context.Clear();
}
