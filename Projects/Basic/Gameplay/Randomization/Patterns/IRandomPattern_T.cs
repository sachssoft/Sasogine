using Sachssoft.Engine.Gameplay.Randomization.Generators;

namespace Sachssoft.Engine.Gameplay.Randomization.Patterns;

/// <summary>Produces values using a stateful selection model.</summary>
/// <typeparam name="T">The selected value type.</typeparam>
public interface IRandomPattern<out T>
{
    /// <summary>Gets the random stream used by the model.</summary>
    IRandomGenerator Random { get; }
    /// <summary>Selects the next value and updates the model's state.</summary>
    T Next();
}
