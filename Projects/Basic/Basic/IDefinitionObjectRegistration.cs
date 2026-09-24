using System;

namespace Sachssoft.Engine;

/// <summary>
/// Defines a dynamic registration for creating engine objects from definitions.
/// </summary>
/// <typeparam name="TDefinition">
/// The base definition type supported by the registration.
/// </typeparam>
/// <typeparam name="TObject">
/// The base engine object type created by the registration.
/// </typeparam>
public interface IDefinitionObjectRegistration<in TDefinition, out TObject>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    /// <summary>
    /// Gets the concrete definition type handled by the registration.
    /// </summary>
    Type DefinitionType { get; }

    /// <summary>
    /// Gets the concrete engine object type created by the registration.
    /// </summary>
    Type ObjectType { get; }

    /// <summary>
    /// Creates an engine object from the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    TObject Create(TDefinition definition);
}