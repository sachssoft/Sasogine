using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental;

/// <summary>
/// Represents a registry key consisting of a string identifier and an enumeration value.
/// </summary>
/// <typeparam name="TEnum">The enumeration type used as the index value.</typeparam>
public readonly struct IndexedGameRegistryKey<TEnum> :
    IEquatable<IndexedGameRegistryKey<TEnum>>,
    IGameRegistryKey
    where TEnum : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexedGameRegistryKey{TEnum}"/> structure.
    /// </summary>
    /// <param name="name">The string identifier of the registry key.</param>
    /// <param name="index">The enumeration value used as the index.</param>
    public IndexedGameRegistryKey(string name, TEnum index)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Index = index;
    }

    /// <summary>
    /// Gets the string identifier of the registry key.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the enumeration value used as the index.
    /// </summary>
    public TEnum Index { get; }

    public bool Equals(IndexedGameRegistryKey<TEnum> other)
    {
        return StringComparer.Ordinal.Equals(Name, other.Name) &&
               EqualityComparer<TEnum>.Default.Equals(Index, other.Index);
    }

    public override bool Equals(object? obj)
        => obj is IndexedGameRegistryKey<TEnum> other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(
            StringComparer.Ordinal.GetHashCode(Name),
            EqualityComparer<TEnum>.Default.GetHashCode(Index));

    public override string ToString() => $"{Name}:{Index}";

    public static bool operator ==(
        IndexedGameRegistryKey<TEnum> left,
        IndexedGameRegistryKey<TEnum> right) => left.Equals(right);

    public static bool operator !=(
        IndexedGameRegistryKey<TEnum> left,
        IndexedGameRegistryKey<TEnum> right) => !left.Equals(right);
}