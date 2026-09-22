using System;

namespace Sachssoft.Engine;

/// <summary>
/// Represents a game registry key identified by a string name.
/// </summary>
public readonly struct NamedGameRegistryKey :
    IGameRegistryKey,
    IEquatable<NamedGameRegistryKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NamedGameRegistryKey"/> structure.
    /// </summary>
    /// <param name="name">The string identifier of the registry key.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is empty or consists only of white-space characters.
    /// </exception>
    public NamedGameRegistryKey(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Determines whether this registry key is equal to another
    /// <see cref="NamedGameRegistryKey"/>.
    /// </summary>
    /// <param name="other">The registry key to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both keys have the same name using
    /// ordinal comparison; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(NamedGameRegistryKey other)
        => StringComparer.Ordinal.Equals(Name, other.Name);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is NamedGameRegistryKey other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Name);

    /// <inheritdoc/>
    public override string ToString()
        => Name;

    /// <summary>
    /// Determines whether two registry keys are equal.
    /// </summary>
    /// <param name="left">The first registry key to compare.</param>
    /// <param name="right">The second registry key to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both registry keys are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(
        NamedGameRegistryKey left,
        NamedGameRegistryKey right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two registry keys are not equal.
    /// </summary>
    /// <param name="left">The first registry key to compare.</param>
    /// <param name="right">The second registry key to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the registry keys are not equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(
        NamedGameRegistryKey left,
        NamedGameRegistryKey right)
        => !left.Equals(right);
}