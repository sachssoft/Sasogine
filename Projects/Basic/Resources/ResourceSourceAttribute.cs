using System;

namespace Sachssoft.Engine.Resources;

/// <summary>Declares a name used to identify a concrete resource source in authored data.</summary>
/// <remarks>The localization generator resolves names at compile time. A source used by the
/// generator must provide a public constructor with a single string parameter.</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ResourceSourceAttribute : Attribute
{
    /// <summary>Initializes a resource source name.</summary>
    /// <param name="name">The alias used to identify the resource source.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    public ResourceSourceAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>Gets the resource source alias.</summary>
    public string Name { get; }
}
