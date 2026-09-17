using System;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Represents a named game registry key.
/// </summary>
public readonly struct NamedGameRegistryKey :
    IGameRegistryKey,
    IEquatable<NamedGameRegistryKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NamedGameRegistryKey"/> structure.
    /// </summary>
    /// <param name="name">The string identifier of the registry key.</param>
    public NamedGameRegistryKey(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool Equals(NamedGameRegistryKey other)
        => StringComparer.Ordinal.Equals(Name, other.Name);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is NamedGameRegistryKey other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Name);

    /// <inheritdoc/>
    public override string ToString() => Name;

    public static bool operator ==(NamedGameRegistryKey left, NamedGameRegistryKey right)
        => left.Equals(right);

    public static bool operator !=(NamedGameRegistryKey left, NamedGameRegistryKey right)
        => !left.Equals(right);
}