
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine;

/// <summary>
/// Provides registration, resolution, and lifecycle execution of application services.
/// Supports singleton instances and transient factories for interfaces and concrete types.
/// </summary>
/// <remarks>
/// Service registrations can be locked before application initialization.
/// Lifecycle actions operate only on registered singleton instances.
/// The manager is not thread-safe and should be configured and executed on the application thread.
/// </remarks>
public sealed class GameServiceManager : IServiceProvider
{
    private readonly Dictionary<Type, ServiceEntry> _services = new();
    private readonly HashSet<object> _executed = new(ReferenceEqualityComparer.Instance);
    private bool _locked;

    /// <summary>Gets whether service registrations are locked.</summary>
    public bool IsLocked => _locked;

    /// <summary>
    /// Permanently prevents further service registrations.
    /// </summary>
    public void Lock() => _locked = true;

    /// <summary>
    /// Registers an existing instance as a singleton service.
    /// Replaces an existing registration for the same service type.
    /// </summary>
    /// <typeparam name="TService">The type under which the service is registered.</typeparam>
    /// <param name="instance">The service instance.</param>
    /// <exception cref="ArgumentNullException">The instance is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are locked.</exception>
    public void AddSingleton<TService>(TService instance) where TService : class
    {
        EnsureUnlocked();
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
    /// <exception cref="ArgumentNullException">The factory is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are locked.</exception>
    public void AddTransient<TService>(Func<TService> factory) where TService : class
    {
        EnsureUnlocked();
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
    /// <returns>True if the service was registered; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">The instance is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are locked.</exception>
    public bool TryAddSingleton<TService>(TService instance) where TService : class
    {
        EnsureUnlocked();
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
    /// <returns>True if the service was registered; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">The factory is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are locked.</exception>
    public bool TryAddTransient<TService>(Func<TService> factory) where TService : class
    {
        EnsureUnlocked();
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
    /// <exception cref="InvalidOperationException">The service is not registered.</exception>
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
    /// <param name="service">Receives the resolved service, or null if unavailable.</param>
    /// <returns>True if the service was resolved successfully; otherwise, false.</returns>
    public bool TryGet<TService>(out TService? service) where TService : class
    {
        service = GetService(typeof(TService)) as TService;
        return service is not null;
    }

    /// <summary>
    /// Resolves a registered service by its service type.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>The resolved service instance, or null if unavailable.</returns>
    /// <exception cref="ArgumentNullException">The service type is null.</exception>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        return _services.TryGetValue(serviceType, out var entry)
            ? entry.GetInstance()
            : null;
    }

    /// <summary>
    /// Invokes an action for every registered singleton service matching the specified type.
    /// </summary>
    /// <typeparam name="T">The required service type or interface.</typeparam>
    /// <param name="action">The action to invoke for each matching service.</param>
    /// <exception cref="ArgumentNullException">The action is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are not locked.</exception>
    /// <remarks>
    /// The action can be executed repeatedly. Services registered under multiple
    /// types may be invoked multiple times during the same execution.
    /// </remarks>
    public void ForEach<T>(Action<T> action) where T : class
    {
        ArgumentNullException.ThrowIfNull(action);
        EnsureLocked();

        foreach (var entry in _services.Values)
        {
            if (entry.Instance is T service)
                action(service);
        }
    }

    /// <summary>
    /// Invokes an action once per unique singleton instance matching the specified type.
    /// </summary>
    /// <typeparam name="T">The required service type or interface.</typeparam>
    /// <param name="action">The action to invoke for each matching service.</param>
    /// <exception cref="ArgumentNullException">The action is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are not locked.</exception>
    /// <remarks>
    /// Successful execution is tracked by object reference across all calls and service types.
    /// Failed actions are not recorded and may be retried.
    /// Transient services are excluded.
    /// </remarks>
    public void ForEachOnce<T>(Action<T> action) where T : class
    {
        ArgumentNullException.ThrowIfNull(action);
        EnsureLocked();

        foreach (var entry in _services.Values)
        {
            if (entry.Instance is not T service || _executed.Contains(service))
                continue;

            action(service);
            _executed.Add(service);
        }
    }

    private void EnsureUnlocked()
    {
        if (_locked)
            throw new InvalidOperationException("Service registrations are locked.");
    }

    private void EnsureLocked()
    {
        if (!_locked)
            throw new InvalidOperationException("Service registrations must be locked before execution.");
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
