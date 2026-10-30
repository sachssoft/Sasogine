using Sachssoft.Engine;
using System;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides optional support for copying settings from another definition.
/// </summary>
/// <remarks>
/// Implementations define which source types and settings they support. Validate
/// the source before modifying the target, and copy owned mutable settings rather
/// than sharing them unintentionally. Unselected target settings remain unchanged.
/// </remarks>
public interface IDefinitionSettingsCopyable : IDefinition
{
    /// <summary>
    /// Copies the supported settings from the specified definition into this instance.
    /// </summary>
    /// <param name="source">The definition providing the settings.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The source definition or its settings are not supported by this implementation.
    /// </exception>
    void CopySettingsFrom(IDefinition source);
}
