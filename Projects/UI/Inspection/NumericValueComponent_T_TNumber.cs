using System.Numerics;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Describes one exact numeric component and its optional shared row heading.</summary>
/// <typeparam name = "T">The composite struct.</typeparam>
/// <typeparam name = "TNumber">The actual component number type.</typeparam>
public readonly struct NumericValueComponent<T, TNumber>
    where T : struct where TNumber : struct, INumber<TNumber>
{
    /// <summary>Creates a typed component; equal group names share a horizontal row.</summary>
    public NumericValueComponent(string name, Func<T, TNumber> read, Func<T, TNumber, T> withValue, string group = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(withValue);
        ArgumentNullException.ThrowIfNull(group);
        Name = name;
        Read = read;
        WithValue = withValue;
        Group = group;
    }

    /// <summary>Gets the short component label.</summary>
    public string Name { get; }
    /// <summary>Gets the typed component reader.</summary>
    public Func<T, TNumber> Read { get; }
    /// <summary>Gets the typed component replacement function.</summary>
    public Func<T, TNumber, T> WithValue { get; }
    /// <summary>Gets the shared row heading, or an empty string for a single row.</summary>
    public string Group { get; }
}
