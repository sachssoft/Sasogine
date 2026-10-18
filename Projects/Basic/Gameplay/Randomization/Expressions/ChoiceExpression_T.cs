using Sachssoft.Engine.Gameplay.Randomization.Generators;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Gameplay.Randomization.Expressions;

/// <summary>Selects an item uniformly from a copied, nonempty input collection.</summary>
/// <typeparam name="T">The selected value type.</typeparam>
public sealed class ChoiceExpression<T> : IRandomExpression<T>
{
    private readonly T[] _values;

    /// <summary>Copies the values used for selection.</summary>
    public ChoiceExpression(IEnumerable<T> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values = new List<T>(values).ToArray();
        if (_values.Length == 0) throw new ArgumentException("Choices must not be empty.", nameof(values));
    }

    /// <inheritdoc />
    public T GetValue(IRandomGenerator random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return _values[random.Next(0, _values.Length)];
    }
}
