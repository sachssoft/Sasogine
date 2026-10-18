using System;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Configures a base weight, miss increment and cap for an adaptive selection.</summary>
/// <typeparam name="T">The value type.</typeparam>
public readonly struct AdaptiveChoice<T> where T : notnull
{
    /// <summary>Creates a choice whose weight increases whenever another value is selected.</summary>
    public AdaptiveChoice(T value, double weight, double failureIncrease, double maximumWeight = double.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!double.IsFinite(weight) || weight < 0d) throw new ArgumentOutOfRangeException(nameof(weight));
        if (!double.IsFinite(failureIncrease) || failureIncrease < 0d) throw new ArgumentOutOfRangeException(nameof(failureIncrease));
        if (!double.IsFinite(maximumWeight) || maximumWeight < weight) throw new ArgumentOutOfRangeException(nameof(maximumWeight));
        Value = value;
        Weight = weight;
        FailureIncrease = failureIncrease;
        MaximumWeight = maximumWeight;
    }
    /// <summary>Gets the selected value.</summary>
    public T Value { get; }
    /// <summary>Gets the initial weight, restored when the value is selected.</summary>
    public double Weight { get; }
    /// <summary>Gets the weight increment when another value is selected.</summary>
    public double FailureIncrease { get; }
    /// <summary>Gets the weight cap.</summary>
    public double MaximumWeight { get; }
}
