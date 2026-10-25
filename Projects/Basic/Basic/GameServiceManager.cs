using System;
using System.Collections.Generic;

namespace Sachssoft.Engine;

/// <summary>
/// Provides registration and resolution of application services.
/// Supports singleton instances and transient factories for interfaces and concrete types.
/// </summary>
public sealed class GameServiceManager : IServiceProvider
{
    private readonly Dictionary<Type, ServiceEntry> _services = new();

    /// <summary>
    /// Registers an existing instance as a singleton service.
    /// Replaces an existing registration for the same service type.
    /// </summary>
    /// <typeparam name="TService">The type under which the service is registered.</typeparam>
    /// <param name="instance">The service instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> is <see langword="null"/>.
    /// </exception>
    public void AddSingleton<TService>(TService instance) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        _services[typeof(TService)] = new ServiceEntry
        {
            Instance = instance
        };
    }

    /// <summary>
    /// Registers a transient service factory.
    /// Replaces an existing registration for the same service type.
    /// </summary>
    /// <typeparam name="TService">The type under which the service is registered.</typeparam>
    /// <param name="factory">The factory used to create service instances.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public void AddTransient<TService>(Func<TService> factory) where TService : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        _services[typeof(TService)] = new ServiceEntry
        {
            Factory = () => factory()
        };
    }

    /// <summary>
    /// Registers an existing singleton instance if the service type is not already registered.
    /// </summary>
    /// <typeparam name="TService">The type under which the service is registered.</typeparam>
    /// <param name="instance">The service instance.</param>
    /// <returns>
    /// <see langword="true"/> if the service was registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> is <see langword="null"/>.
    /// </exception>
    public bool TryAddSingleton<TService>(TService instance) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        return _services.TryAdd(typeof(TService), new ServiceEntry
        {
            Instance = instance
        });
    }

    /// <summary>
    /// Registers a transient factory if the service type is not already registered.
    /// </summary>
    /// <typeparam name="TService">The type under which the service is registered.</typeparam>
    /// <param name="factory">The factory used to create service instances.</param>
    /// <returns>
    /// <see langword="true"/> if the service was registered;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public bool TryAddTransient<TService>(Func<TService> factory) where TService : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        return _services.TryAdd(typeof(TService), new ServiceEntry
        {
            Factory = () => factory()
        });
    }

    /// <summary>
    /// Gets a registered service.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the service is not registered.
    /// </exception>
    public TService Get<TService>() where TService : class
    {
        return (TService?)GetService(typeof(TService))
            ?? throw new InvalidOperationException(
                $"Service {typeof(TService).Name} not registered.");
    }

    /// <summary>
    /// Attempts to resolve a registered service.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <param name="service">
    /// Receives the resolved service, or <see langword="null"/> if unavailable.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the service was resolved successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet<TService>(out TService? service) where TService : class
    {
        service = GetService(typeof(TService)) as TService;
        return service is not null;
    }

    /// <summary>
    /// Resolves a registered service by its service type.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>
    /// The resolved service instance, or <see langword="null"/> if unavailable.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="serviceType"/> is <see langword="null"/>.
    /// </exception>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        return _services.TryGetValue(serviceType, out var entry)
            ? entry.GetInstance()
            : null;
    }

    private sealed class ServiceEntry
    {
        public object? Instance { get; init; }

        public Func<object>? Factory { get; init; }

        public object GetInstance()
        {
            return Factory is not null ? Factory() : Instance!;
        }
    }
}