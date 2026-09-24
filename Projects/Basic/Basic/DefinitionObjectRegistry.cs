using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine;

/// <summary>
/// Provides an AOT-safe registry for creating engine objects from definitions.
/// </summary>
/// <typeparam name="TDefinition">
/// The base definition type supported by the registry.
/// </typeparam>
/// <typeparam name="TObject">
/// The base engine object type supported by the registry.
/// </typeparam>
public class DefinitionObjectRegistry<TDefinition, TObject>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    private readonly Dictionary<RuntimeTypeHandle, Registration> _registrations =
        new();

    /// <summary>
    /// Gets the number of registered object factories.
    /// </summary>
    public int Count => _registrations.Count;

    /// <summary>
    /// Registers an object factory for a definition type.
    /// </summary>
    public void Register<TConcreteDefinition, TConcreteObject>(
        Func<TConcreteDefinition, TConcreteObject> factory)
        where TConcreteDefinition : class, TDefinition
        where TConcreteObject : class, TObject
    {
        ArgumentNullException.ThrowIfNull(factory);

        RuntimeTypeHandle key =
            typeof(TConcreteDefinition).TypeHandle;

        Registration registration = new(
            definition => factory((TConcreteDefinition)definition),
            typeof(TConcreteObject));

        if (!_registrations.TryAdd(
                key,
                registration))
        {
            throw CreateAlreadyRegisteredException(
                typeof(TConcreteDefinition));
        }
    }

    /// <summary>
    /// Attempts to register an object factory for a definition type.
    /// </summary>
    public bool TryRegister<TConcreteDefinition, TConcreteObject>(
        Func<TConcreteDefinition, TConcreteObject> factory)
        where TConcreteDefinition : class, TDefinition
        where TConcreteObject : class, TObject
    {
        ArgumentNullException.ThrowIfNull(factory);

        RuntimeTypeHandle key =
            typeof(TConcreteDefinition).TypeHandle;

        return _registrations.TryAdd(
            key,
            new Registration(
                definition => factory((TConcreteDefinition)definition),
                typeof(TConcreteObject)));
    }

    /// <summary>
    /// Determines whether an object factory is registered for a definition type.
    /// </summary>
    public bool IsRegistered<TConcreteDefinition>()
        where TConcreteDefinition : class, TDefinition
    {
        return _registrations.ContainsKey(
            typeof(TConcreteDefinition).TypeHandle);
    }

    /// <summary>
    /// Determines whether an object factory is registered for a definition type.
    /// </summary>
    public bool IsRegistered(
        Type definitionType)
    {
        ValidateDefinitionType(definitionType);

        return _registrations.ContainsKey(
            definitionType.TypeHandle);
    }

    /// <summary>
    /// Creates an engine object from the specified definition.
    /// </summary>
    public TConcreteObject Create<TConcreteDefinition, TConcreteObject>(
        TConcreteDefinition definition)
        where TConcreteDefinition : class, TDefinition
        where TConcreteObject : class, TObject
    {
        ArgumentNullException.ThrowIfNull(definition);

        RuntimeTypeHandle key =
            typeof(TConcreteDefinition).TypeHandle;

        if (!_registrations.TryGetValue(
                key,
                out Registration registration))
        {
            throw CreateNotRegisteredException(
                typeof(TConcreteDefinition));
        }

        TObject instance =
            CreateInstance(
                registration,
                definition);

        if (instance is not TConcreteObject result)
        {
            throw CreateInvalidFactoryResultException(
                typeof(TConcreteObject),
                instance.GetType());
        }

        return result;
    }

    /// <summary>
    /// Creates an engine object from the specified definition.
    /// </summary>
    public TObject Create(
        TDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Type definitionType =
            definition.GetType();

        if (!_registrations.TryGetValue(
                definitionType.TypeHandle,
                out Registration registration))
        {
            throw CreateNotRegisteredException(
                definitionType);
        }

        return CreateInstance(
            registration,
            definition);
    }

    /// <summary>
    /// Attempts to create an engine object from the specified definition.
    /// </summary>
    public bool TryCreate(
        TDefinition definition,
        [NotNullWhen(true)] out TObject? instance)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Type definitionType =
            definition.GetType();

        if (!_registrations.TryGetValue(
                definitionType.TypeHandle,
                out Registration registration))
        {
            instance = null;
            return false;
        }

        instance = CreateInstance(
            registration,
            definition);

        return true;
    }

    /// <summary>
    /// Attempts to get the object type registered for a definition type.
    /// </summary>
    public bool TryGetObjectType(
        Type definitionType,
        [NotNullWhen(true)] out Type? objectType)
    {
        ValidateDefinitionType(definitionType);

        if (_registrations.TryGetValue(
                definitionType.TypeHandle,
                out Registration registration))
        {
            objectType = registration.ObjectType;
            return true;
        }

        objectType = null;
        return false;
    }

    /// <summary>
    /// Registers a built-in object factory for a definition type.
    /// </summary>
    protected void RegisterBuiltIn<TConcreteDefinition, TConcreteObject>(
        Func<TConcreteDefinition, TConcreteObject> factory)
        where TConcreteDefinition : class, TDefinition
        where TConcreteObject : class, TObject
    {
        Register<TConcreteDefinition, TConcreteObject>(
            factory);
    }

    /// <summary>
    /// Registers a dynamic registration.
    /// </summary>
    /// <param name="registration">
    /// The registration to add.
    /// </param>
    protected void RegisterRegistration(
        IDefinitionObjectRegistration<TDefinition, TObject> registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        ValidateRegistration(registration);

        Registration value = new(
            registration.Create,
            registration.ObjectType);

        if (!_registrations.TryAdd(
                registration.DefinitionType.TypeHandle,
                value))
        {
            throw CreateAlreadyRegisteredException(
                registration.DefinitionType);
        }
    }

    /// <summary>
    /// Attempts to register a dynamic registration.
    /// </summary>
    protected bool TryRegisterRegistration(
        IDefinitionObjectRegistration<TDefinition, TObject> registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        ValidateRegistration(registration);

        return _registrations.TryAdd(
            registration.DefinitionType.TypeHandle,
            new Registration(
                registration.Create,
                registration.ObjectType));
    }

    /// <summary>
    /// Removes the registration for the specified definition type.
    /// </summary>
    protected bool RemoveRegistration(
        Type definitionType)
    {
        ValidateDefinitionType(definitionType);

        return _registrations.Remove(
            definitionType.TypeHandle);
    }

    /// <summary>
    /// Creates an exception indicating that no registration exists.
    /// </summary>
    protected static InvalidOperationException CreateNotRegisteredException(
        Type definitionType)
    {
        return new InvalidOperationException(
            $"No object factory is registered for definition " +
            $"'{definitionType.FullName}'.");
    }

    private static TObject CreateInstance(
        Registration registration,
        TDefinition definition)
    {
        TObject? instance =
            registration.Factory(definition);

        if (instance is null)
        {
            throw new InvalidOperationException(
                $"The registered object factory for definition " +
                $"'{definition.GetType().FullName}' returned null.");
        }

        if (!registration.ObjectType.IsInstanceOfType(instance))
        {
            throw CreateInvalidFactoryResultException(
                registration.ObjectType,
                instance.GetType());
        }

        return instance;
    }

    private static void ValidateDefinitionType(
        Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        if (!typeof(TDefinition).IsAssignableFrom(definitionType))
        {
            throw new ArgumentException(
                $"Definition type '{definitionType.FullName}' must implement " +
                $"'{typeof(TDefinition).FullName}'.",
                nameof(definitionType));
        }

        if (definitionType.IsAbstract ||
            definitionType.IsInterface)
        {
            throw new ArgumentException(
                $"Definition type '{definitionType.FullName}' must be a concrete type.",
                nameof(definitionType));
        }
    }

    private static void ValidateRegistration(
        IDefinitionObjectRegistration<TDefinition, TObject> registration)
    {
        ValidateDefinitionType(
            registration.DefinitionType);

        if (!typeof(TObject).IsAssignableFrom(
                registration.ObjectType))
        {
            throw new ArgumentException(
                $"Object type '{registration.ObjectType.FullName}' must implement " +
                $"'{typeof(TObject).FullName}'.",
                nameof(registration));
        }

        if (registration.ObjectType.IsAbstract ||
            registration.ObjectType.IsInterface)
        {
            throw new ArgumentException(
                $"Object type '{registration.ObjectType.FullName}' must be a concrete type.",
                nameof(registration));
        }
    }

    private static InvalidOperationException CreateAlreadyRegisteredException(
        Type definitionType)
    {
        return new InvalidOperationException(
            $"An object factory for definition " +
            $"'{definitionType.FullName}' is already registered.");
    }

    private static InvalidOperationException CreateInvalidFactoryResultException(
        Type expectedType,
        Type actualType)
    {
        return new InvalidOperationException(
            $"The registered factory returned object type " +
            $"'{actualType.FullName}', but '{expectedType.FullName}' was expected.");
    }

    private readonly struct Registration
    {
        public Registration(
            Func<TDefinition, TObject> factory,
            Type objectType)
        {
            ArgumentNullException.ThrowIfNull(factory);
            ArgumentNullException.ThrowIfNull(objectType);

            Factory = factory;
            ObjectType = objectType;
        }

        public Func<TDefinition, TObject> Factory { get; }

        public Type ObjectType { get; }
    }
}