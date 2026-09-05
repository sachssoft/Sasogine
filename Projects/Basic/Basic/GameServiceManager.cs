using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides registration and resolution of application services.
/// </summary>
public sealed class GameServiceManager : IServiceProvider
{
    private readonly Dictionary<Type, ServiceEntry> _services = new();

    /// <summary>
    /// Registers an existing instance as a singleton service.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The interface type under which the service is registered.
    /// </typeparam>
    /// <param name="instance">
    /// The service instance.
    /// </param>
    public void AddSingleton<TInterface>(TInterface instance)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        Register(
            typeof(TInterface),
            new ServiceEntry
            {
                Lifetime = ServiceLifetime.Singleton,
                Instance = instance
            });
    }

    /// <summary>
    /// Registers a transient service factory.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The interface type under which the service is registered.
    /// </typeparam>
    /// <param name="factory">
    /// The factory used to create a new service instance.
    /// </param>
    public void AddTransient<TInterface>(
        Func<TInterface> factory)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        Register(
            typeof(TInterface),
            new ServiceEntry
            {
                Lifetime = ServiceLifetime.Transient,
                Factory = () => factory()
            });
    }

    /// <summary>
    /// Gets a registered service.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The interface type of the service.
    /// </typeparam>
    /// <returns>
    /// The registered service instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the service is not registered.
    /// </exception>
    public TInterface Get<TInterface>()
        where TInterface : class
    {
        if (_services.TryGetValue(typeof(TInterface), out var entry))
            return (TInterface)entry.GetInstance();

        throw new InvalidOperationException(
            $"Service {typeof(TInterface).Name} not registered.");
    }

    /// <summary>
    /// Attempts to retrieve a registered service.
    /// </summary>
    /// <typeparam name="TInterface">
    /// The interface type of the service.
    /// </typeparam>
    /// <param name="service">
    /// Receives the registered service when found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the service is registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet<TInterface>(
        out TInterface? service)
        where TInterface : class
    {
        var type = typeof(TInterface);

        if (!type.IsInterface)
            throw new InvalidOperationException(
                "Only interfaces can be resolved.");

        if (_services.TryGetValue(type, out var entry))
        {
            service = (TInterface)entry.GetInstance();
            return true;
        }

        service = null;
        return false;
    }

    /// <summary>
    /// Gets a registered service by its service type.
    /// </summary>
    /// <param name="serviceType">
    /// The type of the service to retrieve.
    /// </param>
    /// <returns>
    /// The registered service instance, or <see langword="null"/>
    /// when no service is registered for the specified type.
    /// </returns>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if (_services.TryGetValue(serviceType, out var entry))
            return entry.GetInstance();

        return null;
    }

    private void Register(
        Type type,
        ServiceEntry entry)
    {
        if (!type.IsInterface)
            throw new InvalidOperationException(
                "Only interfaces are allowed.");

        _services[type] = entry;
    }

    private enum ServiceLifetime
    {
        Singleton,
        Transient
    }

    private sealed class ServiceEntry
    {
        public ServiceLifetime Lifetime { get; init; }

        public object? Instance { get; init; }

        public Func<object>? Factory { get; init; }

        public object GetInstance()
        {
            if (Lifetime == ServiceLifetime.Singleton)
                return Instance!;

            return Factory!();
        }
    }
}