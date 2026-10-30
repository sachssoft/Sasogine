using Sachssoft.Engine;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides optional support for restoring a definition's settings to their defaults.
/// </summary>
/// <remarks>
/// Implementations define which settings participate in the reset and restore them
/// on the existing instance. Externally managed identifiers and relationships should
/// be preserved unless resetting them is explicitly part of the definition's contract.
/// </remarks>
public interface IDefinitionResettable : IDefinition
{
    /// <summary>
    /// Restores the supported settings of this definition to their default values.
    /// </summary>
    void Reset();
}
