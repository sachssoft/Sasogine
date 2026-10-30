using Sachssoft.Engine;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides optional support for creating an independent deep copy of a definition.
/// </summary>
/// <remarks>
/// Implementations must copy owned mutable data and return a new definition instance.
/// References to externally owned resources may be retained. Cloning does not imply
/// assigning new identifiers or changing the copied position.
/// </remarks>
public interface IDefinitionCloneable : IDefinition
{
    /// <summary>
    /// Creates an independent deep copy of this definition.
    /// </summary>
    /// <returns>A new definition containing independent copies of owned mutable data.</returns>
    IDefinition Clone();
}
