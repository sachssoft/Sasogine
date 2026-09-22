using System;
using System.Collections.Generic;

namespace Sachssoft.Engine;

/// <summary>
/// Represents a game registry key that combines a string identifier
/// with a strongly typed enumeration index.
/// </summary>
/// <typeparam name="TEnum">
/// The enumeration type used to identify the indexed registry entry.
/// </typeparam>
public readonly struct IndexedGameRegistryKey<TEnum> :
    IEquatable<IndexedGameRegistryKey<TEnum>>,
    IGameRegistryKey
    where TEnum : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="IndexedGameRegistryKey{TEnum}"/> structure.
    /// </summary>
    /// <param name="name">
    /// The name identifying the registry key.
    /// </param>
    /// <param name="index">
    /// The enumeration value identifying the indexed entry.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is <see langword="null"/>,
    /// empty, or consists only of white-space characters.
    /// </exception>
    public IndexedGameRegistryKey(
        string name,
        TEnum index)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Index = index;
    }

    /// <summary>
    /// Gets the name identifying the registry key.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the enumeration value identifying the indexed registry entry.
    /// </summary>
    public TEnum Index { get; }

    /// <summary>
    /// Determines whether this registry key is equal to another registry key
    /// by comparing both the name and enumeration index.
    /// </summary>
    /// <param name="other">
    /// The registry key to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if both keys have the same ordinal name
    /// and enumeration index; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(
        IndexedGameRegistryKey<TEnum> other)
    {
        return StringComparer.Ordinal.Equals(
                   Name,
                   other.Name) &&
               EqualityComparer<TEnum>.Default.Equals(
                   Index,
                   other.Index);
    }

    /// <summary>
    /// Determines whether this registry key is equal to the specified object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with this instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is an
    /// <see cref="IndexedGameRegistryKey{TEnum}"/> with the same name
    /// and index; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is IndexedGameRegistryKey<TEnum> other &&
               Equals(other);
    }

    /// <summary>
    /// Returns a hash code based on the registry key name and index.
    /// </summary>
    /// <returns>
    /// A hash code for this registry key.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.Ordinal.GetHashCode(Name),
            EqualityComparer<TEnum>.Default.GetHashCode(Index));
    }

    /// <summary>
    /// Returns a string representation containing the registry key
    /// name and index separated by a colon.
    /// </summary>
    /// <returns>
    /// A string in the form <c>Name:Index</c>.
    /// </returns>
    public override string ToString()
    {
        return $"{Name}:{Index}";
    }

    /// <summary>
    /// Determines whether two registry keys are equal.
    /// </summary>
    /// <param name="left">
    /// The first registry key to compare.
    /// </param>
    /// <param name="right">
    /// The second registry key to compare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if both registry keys are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(
        IndexedGameRegistryKey<TEnum> left,
        IndexedGameRegistryKey<TEnum> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two registry keys are not equal.
    /// </summary>
    /// <param name="left">
    /// The first registry key to compare.
    /// </param>
    /// <param name="right">
    /// The second registry key to compare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the registry keys are not equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(
        IndexedGameRegistryKey<TEnum> left,
        IndexedGameRegistryKey<TEnum> right)
    {
        return !left.Equals(right);
    }
}