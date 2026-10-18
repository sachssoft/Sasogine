using Sachssoft.Engine.Gameplay.Randomization.Generators;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Expressions;

/// <summary>Returns a constant without consuming random values.</summary>
/// <typeparam name="T">The constant's type.</typeparam>
public sealed class ConstantExpression<T> : IRandomExpression<T>
{
    /// <summary>Creates a constant expression.</summary>
    public ConstantExpression(T value) { Value = value; }

    /// <summary>Gets the constant value.</summary>
    public T Value { get; }

    /// <inheritdoc />
    public T GetValue(IRandomGenerator random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return Value;
    }
}
