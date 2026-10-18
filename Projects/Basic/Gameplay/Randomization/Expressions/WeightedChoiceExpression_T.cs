using Sachssoft.Engine.Gameplay.Randomization.Generators;
using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Gameplay.Randomization.Expressions;

/// <summary>Selects a value using copied relative weights.</summary>
/// <typeparam name="T">The selected value type.</typeparam>
public sealed class WeightedChoiceExpression<T> : IRandomExpression<T>
{
    private readonly WeightedChoices<T> _choices;

    /// <summary>Creates a selection with at least one positive weight.</summary>
    public WeightedChoiceExpression(IEnumerable<WeightedValue<T>> choices)
    {
        _choices = new WeightedChoices<T>(choices);
    }

    /// <inheritdoc />
    public T GetValue(IRandomGenerator random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return _choices.Values[WeightedSelection.Choose(random, _choices.Weights)];
    }
}
