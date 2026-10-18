using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Selects the next state using the current state's weighted transitions.</summary>
/// <typeparam name="T">The non-null state type.</typeparam>
public sealed class MarkovGenerator<T> : IRandomPattern<T> where T : notnull
{
    private readonly Dictionary<T, WeightedChoices<T>> _transitions;
    /// <summary>Creates a Markov model with an independent seeded stream.</summary>
    public MarkovGenerator(ulong seed, T initialState, IEqualityComparer<T>? comparer = null)
        : this(new RandomGenerator(seed), initialState, comparer) { }
    /// <summary>Creates a model; transitions are configured separately.</summary>
    public MarkovGenerator(IRandomGenerator random, T initialState, IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(initialState);
        Random = random;
        Current = initialState;
        _transitions = new Dictionary<T, WeightedChoices<T>>(comparer);
    }
    /// <inheritdoc />
    public IRandomGenerator Random { get; }
    /// <summary>Gets the current state.</summary>
    public T Current { get; private set; }
    /// <summary>Copies and replaces the transitions for one state.</summary>
    public void SetTransitions(T state, IEnumerable<WeightedValue<T>> transitions)
    {
        ArgumentNullException.ThrowIfNull(state);
        var table = new WeightedChoices<T>(transitions);
        foreach (T value in table.Values)
            if (value is null) throw new ArgumentException("Transition states must not be null.", nameof(transitions));
        _transitions[state] = table;
    }
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">The current state has no configured transitions.</exception>
    public T Next()
    {
        if (!_transitions.TryGetValue(Current, out WeightedChoices<T>? table))
            throw new InvalidOperationException("No transitions are configured for the current state.");
        Current = table.Values[WeightedSelection.Choose(Random, table.Weights)];
        return Current;
    }
    /// <summary>Sets the current state without restarting the random stream.</summary>
    public void Reset(T state)
    {
        ArgumentNullException.ThrowIfNull(state);
        Current = state;
    }
}
