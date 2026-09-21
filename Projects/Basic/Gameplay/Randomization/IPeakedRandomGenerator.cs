namespace Sachssoft.Sasogine.Gameplay.Randomization;

/// <summary>
/// Defines a random value generator whose distribution is influenced
/// by a configurable peak value.
/// </summary>
public interface IPeakedRandomGenerator : IRandomGenerator
{
    /// <summary>
    /// Gets the value around which generated random values are
    /// preferentially concentrated.
    /// </summary>
    public int Peak { get; init; }
}