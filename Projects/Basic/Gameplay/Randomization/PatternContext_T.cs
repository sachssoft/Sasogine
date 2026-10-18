using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Provides read-only access to the bounded history of a selection model.</summary>
/// <typeparam name="T">The selected value type.</typeparam>
public sealed class PatternContext<T> where T : notnull
{
    private readonly List<T> _history = new();
    private readonly IEqualityComparer<T> _comparer;
    private T _previous = default!;

    internal PatternContext(int historyCapacity, IEqualityComparer<T> comparer)
    {
        if (historyCapacity < 1) throw new ArgumentOutOfRangeException(nameof(historyCapacity));
        HistoryCapacity = historyCapacity;
        _comparer = comparer;
        History = _history.AsReadOnly();
    }
    /// <summary>Gets the retained history limit.</summary>
    public int HistoryCapacity { get; }
    /// <summary>Gets retained values in oldest-to-newest order.</summary>
    public IReadOnlyList<T> History { get; }
    /// <summary>Gets whether a previous selection exists.</summary>
    public bool HasPrevious { get; private set; }
    /// <summary>Gets the previous selection; throws when no value has been selected.</summary>
    public T Previous => HasPrevious ? _previous : throw new InvalidOperationException("No value has been selected.");
    /// <summary>Gets consecutive occurrences of the previous value, saturating at int.MaxValue.</summary>
    public int ConsecutiveCount { get; private set; }
    /// <summary>Gets successful selections since reset, saturating at ulong.MaxValue.</summary>
    public ulong DrawCount { get; private set; }

    /// <summary>Counts occurrences within the retained history.</summary>
    public int GetRecentCount(T value)
    {
        int count = 0;
        foreach (T item in _history) if (_comparer.Equals(item, value)) count++;
        return count;
    }

    internal void Record(T value)
    {
        bool repeated = HasPrevious && _comparer.Equals(_previous, value);
        ConsecutiveCount = repeated ? (ConsecutiveCount < int.MaxValue ? ConsecutiveCount + 1 : int.MaxValue) : 1;
        _previous = value;
        HasPrevious = true;
        if (DrawCount < ulong.MaxValue) DrawCount++;
        if (_history.Count == HistoryCapacity) _history.RemoveAt(0);
        _history.Add(value);
    }

    internal void Clear()
    {
        _history.Clear();
        _previous = default!;
        HasPrevious = false;
        ConsecutiveCount = 0;
        DrawCount = 0;
    }
}
