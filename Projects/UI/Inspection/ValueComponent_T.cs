namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Describes one numeric component of a value through typed delegates.</summary>
/// <typeparam name = "T">The composite value type.</typeparam>
public readonly struct ValueComponent<T>
    where T : struct
{
    /// <summary>Creates a named component with an explicit range and increment.</summary>
    public ValueComponent(string name, Func<T, float> read, Func<T, float, T> withValue, float minimum, float maximum, float step = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(withValue);
        if (!float.IsFinite(minimum) || !float.IsFinite(maximum) || minimum > maximum)
            throw new ArgumentOutOfRangeException(nameof(minimum));
        if (!float.IsFinite(step) || step <= 0)
            throw new ArgumentOutOfRangeException(nameof(step));
        Name = name;
        Read = read;
        WithValue = withValue;
        Minimum = minimum;
        Maximum = maximum;
        Step = step;
    }

    /// <summary>Gets the component label, such as X, Y, Z or W.</summary>
    public string Name { get; }
    /// <summary>Gets the typed component reader.</summary>
    public Func<T, float> Read { get; }
    /// <summary>Gets the function returning a composite with this component replaced.</summary>
    public Func<T, float, T> WithValue { get; }
    /// <summary>Gets the minimum component value.</summary>
    public float Minimum { get; }
    /// <summary>Gets the maximum component value.</summary>
    public float Maximum { get; }
    /// <summary>Gets the component increment.</summary>
    public float Step { get; }
}
