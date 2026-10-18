using Sachssoft.Engine.Gameplay.Randomization.Generators;

namespace Sachssoft.Engine.Gameplay.Randomization.Expressions;

/// <summary>Evaluates a value using an explicitly supplied random stream.</summary>
/// <typeparam name="T">The expression result type.</typeparam>
public interface IRandomExpression<out T>
{
    /// <summary>Evaluates this expression with the supplied stream.</summary>
    T GetValue(IRandomGenerator random);
}
