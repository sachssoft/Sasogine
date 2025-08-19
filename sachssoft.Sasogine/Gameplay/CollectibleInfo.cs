namespace Sachssoft.Sasogine.Gameplay;

/// <summary>
/// Represents the progress of a collectible goal, such as collecting gems or keys.
/// </summary>
public readonly struct CollectibleInfo
{
    /// <summary>
    /// The current collected amount.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// The required amount to complete the goal.
    /// </summary>
    public int Required { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectibleInfo"/> struct with the specified values.
    /// </summary>
    /// <param name="count">The current collected amount.</param>
    /// <param name="required">The amount required to complete the objective.</param>
    public CollectibleInfo(int count, int required)
    {
        Count = count;
        Required = required;
    }

    /// <summary>
    /// Returns a new instance with the <see cref="Count"/> increased by one.
    /// </summary>
    /// <returns>A new <see cref="CollectibleInfo"/> instance with the updated count.</returns>
    public CollectibleInfo IncrementCount()
    {
        return new CollectibleInfo(Count + 1, Required);
    }

    /// <summary>
    /// Returns a new instance with the specified required amount.
    /// </summary>
    /// <param name="required">The new required amount to complete the goal.</param>
    /// <returns>A new <see cref="CollectibleInfo"/> instance with the updated required value.</returns>
    public CollectibleInfo WithRequired(int required)
    {
        return new CollectibleInfo(Count, required);
    }

    /// <summary>
    /// Gets a value indicating whether the required amount has been reached or exceeded.
    /// </summary>
    public bool IsComplete => Count >= Required;

    /// <summary>
    /// Returns a string that represents the current progress, e.g. "3/5".
    /// </summary>
    /// <returns>
    /// A string in the format "<c>{Count}/{Required}</c>" representing the current collectible status.
    /// </returns>
    public override string ToString()
    {
        return $"{Count}/{Required}";
    }
}
