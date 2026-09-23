using System;

namespace Sachssoft.Engine.Common;

/// <summary>
/// Provides a dynamic registry for creating engine objects from definitions
/// using registrations that can be added and removed at runtime.
/// </summary>
/// <typeparam name="TDefinition">
/// The base definition type supported by the registry.
/// </typeparam>
/// <typeparam name="TObject">
/// The base engine object type supported by the registry.
/// </typeparam>
public class DynamicDefinitionObjectRegistry<TDefinition, TObject> :
    DefinitionObjectRegistry<TDefinition, TObject>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    /// <summary>
    /// Registers a dynamic object registration.
    /// </summary>
    /// <param name="registration">
    /// The registration to add.
    /// </param>
    public void Register(
        IDefinitionObjectRegistration<TDefinition, TObject> registration)
    {
        RegisterRegistration(
            registration);
    }

    /// <summary>
    /// Attempts to register a dynamic object registration.
    /// </summary>
    /// <param name="registration">
    /// The registration to add.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the registration was added;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryRegister(
        IDefinitionObjectRegistration<TDefinition, TObject> registration)
    {
        return TryRegisterRegistration(
            registration);
    }

    /// <summary>
    /// Removes the registration for the specified definition type.
    /// </summary>
    /// <param name="definitionType">
    /// The concrete definition type.
    /// </param>
    public void Unregister(
        Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        if (!RemoveRegistration(definitionType))
        {
            throw CreateNotRegisteredException(
                definitionType);
        }
    }

    /// <summary>
    /// Removes a dynamic object registration.
    /// </summary>
    /// <param name="registration">
    /// The registration to remove.
    /// </param>
    public void Unregister(
        IDefinitionObjectRegistration<TDefinition, TObject> registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        Unregister(
            registration.DefinitionType);
    }

    /// <summary>
    /// Attempts to remove the registration for the specified definition type.
    /// </summary>
    /// <param name="definitionType">
    /// The concrete definition type.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the registration was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryUnregister(
        Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        return RemoveRegistration(
            definitionType);
    }

    /// <summary>
    /// Attempts to remove a dynamic object registration.
    /// </summary>
    /// <param name="registration">
    /// The registration to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the registration was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryUnregister(
        IDefinitionObjectRegistration<TDefinition, TObject> registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        return TryUnregister(
            registration.DefinitionType);
    }
}