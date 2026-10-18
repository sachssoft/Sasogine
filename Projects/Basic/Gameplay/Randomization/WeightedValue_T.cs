using System;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Associates a value with a finite, nonnegative relative weight.</summary>
/// <typeparam name="T">The value type.</typeparam>
public readonly struct WeightedValue<T>
{
    /// <summary>Creates a weighted value. Zero excludes the value from selection.</summary>
    public WeightedValue(T value, double weight)
    {
        if (!double.IsFinite(weight) || weight < 0d)
            throw new ArgumentOutOfRangeException(nameof(weight));
        Value = value;
        Weight = weight;
    }
    /// <summary>Gets the value.</summary>
    public T Value { get; }
    /// <summary>Gets the relative selection weight.</summary>
    public double Weight { get; }
}
