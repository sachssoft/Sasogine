namespace sachssoft.Sasogine.Gameplay;

/// <summary>
/// Represents the life status of a game actor.
/// </summary>
public readonly struct LifeInfo
{
    /// <summary>
    /// The current health points.
    /// </summary>
    public int Current { get; }

    /// <summary>
    /// The maximum health points.
    /// </summary>
    public int Max { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LifeInfo"/> struct with specified values.
    /// </summary>
    /// <param name="current">The current health points.</param>
    /// <param name="max">The maximum health points.</param>
    public LifeInfo(int current, int max)
    {
        Current = current;
        Max = max;
    }

    /// <summary>
    /// Gets a value indicating whether the actor is still alive (health points > 0).
    /// </summary>
    public bool IsAlive => Current > 0;

    /// <summary>
    /// Gets a value indicating whether the actor has full health.
    /// </summary>
    public bool IsFullHealth => Current >= Max;

    /// <summary>
    /// Returns a new instance with increased health points (up to the maximum).
    /// </summary>
    /// <param name="amount">The amount to heal.</param>
    /// <returns>A new <see cref="LifeInfo"/> instance with updated health points.</returns>
    public LifeInfo Heal(int amount)
    {
        var new_current = Current + amount;
        if (new_current > Max)
            new_current = Max;
        return new LifeInfo(new_current, Max);
    }

    /// <summary>
    /// Returns a new instance with decreased health points (minimum zero).
    /// </summary>
    /// <param name="amount">The damage amount.</param>
    /// <returns>A new <see cref="LifeInfo"/> instance with updated health points.</returns>
    public LifeInfo TakeDamage(int amount)
    {
        var new_current = Current - amount;
        if (new_current < 0)
            new_current = 0;
        return new LifeInfo(new_current, Max);
    }

    /// <summary>
    /// Returns a string representing the current health status, e.g. "3/5".
    /// </summary>
    /// <returns>A string in the format "<c>{Current}/{Max}</c>" describing the health.</returns>
    public override string ToString()
    {
        return $"{Current}/{Max}";
    }
}
