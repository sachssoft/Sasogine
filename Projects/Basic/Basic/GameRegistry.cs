using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides a registry for creating objects through string identifiers,
/// integer identifiers, or their registered types.
/// </summary>
public class GameRegistry
{
    private readonly Dictionary<string, Func<object>> _stringFactories = new();
    private readonly Dictionary<int, Func<object>> _intFactories = new();
    private readonly Dictionary<Type, Func<object>> _typeFactories = new();

    /// <summary>
    /// Registers a factory using a string identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier used to resolve the factory.
    /// </param>
    /// <param name="type">
    /// The type associated with the factory.
    /// </param>
    /// <param name="factory">
    /// The factory used to create the object.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="id"/>, <paramref name="type"/>,
    /// or <paramref name="factory"/> is invalid.
    /// </exception>
    public void Register(
        string id,
        Type type,
        Func<object> factory)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(factory);

        _stringFactories[id] = factory;
    }

    /// <summary>
    /// Registers a strongly typed factory using a string identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object created by the factory.
    /// </typeparam>
    /// <param name="id">
    /// The identifier used to resolve the factory.
    /// </param>
    /// <param name="factory">
    /// The factory used to create the object.
    /// </param>
    public void Register<T>(
        string id,
        Func<T> factory)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        ArgumentNullException.ThrowIfNull(factory);

        _stringFactories[id] = () => factory();
    }

    /// <summary>
    /// Registers a factory using an integer identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier used to resolve the factory.
    /// </param>
    /// <param name="type">
    /// The type associated with the factory.
    /// </param>
    /// <param name="factory">
    /// The factory used to create the object.
    /// </param>
    public void Register(
        int id,
        Type type,
        Func<object> factory)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(factory);

        _intFactories[id] = factory;
    }

    /// <summary>
    /// Registers a strongly typed factory using an integer identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object created by the factory.
    /// </typeparam>
    /// <param name="id">
    /// The identifier used to resolve the factory.
    /// </param>
    /// <param name="factory">
    /// The factory used to create the object.
    /// </param>
    public void Register<T>(
        int id,
        Func<T> factory)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        _intFactories[id] = () => factory();
    }

    /// <summary>
    /// Registers a type using its parameterless constructor.
    /// </summary>
    /// <typeparam name="T">
    /// The type to register.
    /// </typeparam>
    public void Register<T>()
        where T : class, new()
    {
        _typeFactories[typeof(T)] = () => new T();
    }

    /// <summary>
    /// Registers a strongly typed factory for a type.
    /// </summary>
    /// <typeparam name="T">
    /// The type created by the factory.
    /// </typeparam>
    /// <param name="factory">
    /// The factory used to create the object.
    /// </param>
    public void Register<T>(
        Func<T> factory)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        _typeFactories[typeof(T)] = () => factory();
    }

    /// <summary>
    /// Creates an object using a registered string identifier.
    /// </summary>
    /// <param name="id">
    /// The registered string identifier.
    /// </param>
    /// <returns>
    /// The object created by the registered factory.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no factory is registered for the specified identifier.
    /// </exception>
    public object Create(string id)
    {
        if (_stringFactories.TryGetValue(id, out var factory))
            return factory();

        throw new InvalidOperationException(
            $"No factory registered with string ID '{id}'");
    }

    /// <summary>
    /// Creates an object using a registered integer identifier.
    /// </summary>
    /// <param name="id">
    /// The registered integer identifier.
    /// </param>
    /// <returns>
    /// The object created by the registered factory.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no factory is registered for the specified identifier.
    /// </exception>
    public object Create(int id)
    {
        if (_intFactories.TryGetValue(id, out var factory))
            return factory();

        throw new InvalidOperationException(
            $"No factory registered with int ID '{id}'");
    }

    /// <summary>
    /// Creates an object using its registered type.
    /// </summary>
    /// <typeparam name="T">
    /// The registered object type.
    /// </typeparam>
    /// <returns>
    /// The object created by the registered factory.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no factory is registered for the specified type.
    /// </exception>
    public T Create<T>()
        where T : class
    {
        if (_typeFactories.TryGetValue(typeof(T), out var factory))
            return (T)factory();

        throw new InvalidOperationException(
            $"No factory registered for type {typeof(T).FullName}");
    }

    /// <summary>
    /// Determines whether a factory is registered for a string identifier.
    /// </summary>
    public bool IsRegistered(string id) =>
        _stringFactories.ContainsKey(id);

    /// <summary>
    /// Determines whether a factory is registered for an integer identifier.
    /// </summary>
    public bool IsRegistered(int id) =>
        _intFactories.ContainsKey(id);

    /// <summary>
    /// Determines whether a factory is registered for a type.
    /// </summary>
    public bool IsRegistered<T>() =>
        _typeFactories.ContainsKey(typeof(T));

    /// <summary>
    /// Determines whether a factory is registered for the specified type.
    /// </summary>
    /// <param name="type">
    /// The type to check.
    /// </param>
    public bool IsRegistered(Type type) =>
        _typeFactories.ContainsKey(type);
}