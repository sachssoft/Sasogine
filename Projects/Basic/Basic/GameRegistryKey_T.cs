using System;
using System.Collections.Generic;

namespace Sachssoft.Engine;

/// <summary>
/// Represents a composite registry key consisting of a string identifier
/// and a strongly typed value.
/// </summary>
/// <typeparam name="TKey">
/// The type of the secondary key value.
/// </typeparam>
public readonly struct GameRegistryKey<TKey> :
    IEquatable<GameRegistryKey<TKey>>,
    IGameRegistryKey
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new registry key.
    /// </summary>
    /// <param name="name">
    /// The string identifier of the key.
    /// </param>
    /// <param name="value">
    /// The strongly typed value of the key.
    /// </param>
    public GameRegistryKey(
        string name,
        TKey value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;
        Value = value;
    }

    /// <summary>
    /// Gets the string identifier.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the strongly typed key value.
    /// </summary>
    public TKey Value { get; }

    /// <inheritdoc/>
    public bool Equals(GameRegistryKey<TKey> other)
    {
        return StringComparer.Ordinal.Equals(
                   Name,
                   other.Name) &&
               EqualityComparer<TKey>.Default.Equals(
                   Value,
                   other.Value);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is GameRegistryKey<TKey> other &&
               Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.Ordinal.GetHashCode(Name),
            EqualityComparer<TKey>.Default.GetHashCode(Value));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Name}:{Value}";
    }

    /// <summary>
    /// Determines whether two registry keys are equal.
    /// </summary>
    public static bool operator ==(GameRegistryKey<TKey> left, GameRegistryKey<TKey> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two registry keys are not equal.
    /// </summary>
    public static bool operator !=(GameRegistryKey<TKey> left, GameRegistryKey<TKey> right)
    {
        return !left.Equals(right);
    }
}