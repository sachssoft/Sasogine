using Sachssoft.Engine;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides access to optional definition cloning, settings-copying, and reset capabilities.
/// </summary>
/// <remarks>
/// Operations dispatch through explicit interfaces without reflection, serialization,
/// or automatic property discovery. Exceptions raised by implementations propagate
/// to the caller, including when using a try method.
/// </remarks>
public static class DefinitionExtensions
{
    /// <summary>
    /// Creates an independent deep copy using the definition's clone implementation.
    /// </summary>
    /// <param name="definition">The definition to clone.</param>
    /// <returns>The new definition returned by the clone implementation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="definition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The definition does not implement <see cref="IDefinitionCloneable"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The clone implementation returns <see langword="null"/> or the original instance.
    /// </exception>
    public static IDefinition Clone(this IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!TryClone(definition, out var clone))
            throw new NotSupportedException($"The definition does not implement {nameof(IDefinitionCloneable)}.");

        return clone;
    }

    /// <summary>
    /// Creates an independent deep copy when the definition supports cloning.
    /// </summary>
    /// <param name="definition">The optional definition to clone.</param>
    /// <param name="clone">
    /// The new definition on success; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the definition implements <see cref="IDefinitionCloneable"/>
    /// and creates an independent instance; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The clone implementation returns <see langword="null"/> or the original instance.
    /// </exception>
    /// <remarks>
    /// A null definition or missing interface returns <see langword="false"/>.
    /// Exceptions raised by the clone implementation are not caught.
    /// </remarks>
    public static bool TryClone(
        this IDefinition? definition,
        [NotNullWhen(true)] out IDefinition? clone)
    {
        clone = null;

        if (definition is not IDefinitionCloneable cloneable)
            return false;

        var result = cloneable.Clone();

        if (result is null || ReferenceEquals(result, definition))
            throw new InvalidOperationException("The clone implementation must return a new, non-null definition instance.");

        clone = result;
        return true;
    }

    /// <summary>
    /// Copies supported source settings into an existing definition.
    /// </summary>
    /// <param name="target">The definition receiving the settings.</param>
    /// <param name="source">The definition providing the settings.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="target"/> or <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The target does not implement <see cref="IDefinitionSettingsCopyable"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The source definition or its settings are rejected by the target implementation.
    /// </exception>
    public static void CopySettingsFrom(this IDefinition target, IDefinition source)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        if (target is not IDefinitionSettingsCopyable copyable)
            throw new NotSupportedException($"The definition does not implement {nameof(IDefinitionSettingsCopyable)}.");

        copyable.CopySettingsFrom(source);
    }

    /// <summary>
    /// Copies source settings when the target supports settings copying.
    /// </summary>
    /// <param name="target">The optional definition receiving the settings.</param>
    /// <param name="source">The optional definition providing the settings.</param>
    /// <returns>
    /// <see langword="true"/> when both definitions are non-null, the target implements
    /// <see cref="IDefinitionSettingsCopyable"/>, and its implementation completes;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The source definition or its settings are rejected by the target implementation.
    /// </exception>
    /// <remarks>
    /// Null arguments or a missing target interface return <see langword="false"/>.
    /// Exceptions raised by the settings-copying implementation are not caught.
    /// </remarks>
    public static bool TryCopySettingsFrom(this IDefinition? target, IDefinition? source)
    {
        if (source is null || target is not IDefinitionSettingsCopyable copyable)
            return false;

        copyable.CopySettingsFrom(source);
        return true;
    }

    /// <summary>
    /// Restores the definition's supported settings to their default values.
    /// </summary>
    /// <param name="definition">The existing definition to reset.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="definition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The definition does not implement <see cref="IDefinitionResettable"/>.
    /// </exception>
    /// <remarks>
    /// The reset implementation determines which settings are restored. This method
    /// does not replace the definition or discover default values automatically.
    /// </remarks>
    public static void Reset(this IDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition is not IDefinitionResettable resettable)
            throw new NotSupportedException($"The definition does not implement {nameof(IDefinitionResettable)}.");

        resettable.Reset();
    }

    /// <summary>
    /// Restores default settings when the definition supports resetting.
    /// </summary>
    /// <param name="definition">The optional definition to reset.</param>
    /// <returns>
    /// <see langword="true"/> when the definition implements <see cref="IDefinitionResettable"/>
    /// and its reset implementation completes; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// A null definition or missing interface returns <see langword="false"/>.
    /// Exceptions raised by the reset implementation are not caught.
    /// </remarks>
    public static bool TryReset(this IDefinition? definition)
    {
        if (definition is not IDefinitionResettable resettable)
            return false;

        resettable.Reset();
        return true;
    }
}
