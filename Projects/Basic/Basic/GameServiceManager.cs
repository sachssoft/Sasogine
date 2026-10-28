
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
/// The manager is not thread-safe.
/// </remarks>
public sealed class GameServiceManager : IServiceProvider
{
    private readonly Dictionary<Type, ServiceEntry> _services = new();
    private readonly HashSet<object> _executed = new(ReferenceEqualityComparer.Instance);
    private bool _locked;

    /// <summary>Gets whether service registrations are locked.</summary>
    public bool IsLocked => _locked;

    /// <summary>Permanently prevents further service registrations.</summary>
    public void Lock() => _locked = true;

    /// <summary>
    /// Registers an existing instance as a singleton service.
    /// </summary>
    /// <typeparam name="TInterface">The type under which the service is registered.</typeparam>
    /// <param name="instance">The service instance.</param>
    /// <exception cref="ArgumentNullException">The instance is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are locked.</exception>
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
    /// <typeparam name="TInterface">The type under which the service is registered.</typeparam>
    /// <param name="factory">The factory used to create a new service instance.</param>
    /// <exception cref="ArgumentNullException">The factory is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are locked.</exception>
    public void AddTransient<TInterface>(Func<TInterface> factory)
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
    /// <typeparam name="TInterface">The type of the service.</typeparam>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// The service is not registered or multiple compatible registrations exist.
    /// </exception>
    public TInterface Get<TInterface>()
        where TInterface : class
    {
        return (TInterface?)GetService(typeof(TInterface))
            ?? throw new InvalidOperationException(
                $"Service {typeof(TInterface).Name} not registered.");
    }

    /// <summary>
    /// Attempts to retrieve a registered service.
    /// </summary>
    /// <typeparam name="TInterface">The type of the service.</typeparam>
    /// <param name="service">Receives the resolved service when found.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Multiple compatible registrations exist.</exception>
    public bool TryGet<TInterface>(out TInterface? service)
        where TInterface : class
    {
        service = GetService(typeof(TInterface)) as TInterface;
        return service is not null;
    }

    /// <summary>
    /// Gets a registered service by its exact type or a compatible interface or base type.
    /// </summary>
    /// <param name="serviceType">The type of the service to retrieve.</param>
    /// <returns>The resolved service instance, or null when unavailable.</returns>
    /// <exception cref="ArgumentNullException">The service type is null.</exception>
    /// <exception cref="InvalidOperationException">Multiple compatible registrations exist.</exception>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if (_services.TryGetValue(serviceType, out var entry))
            return entry.GetInstance();

        ServiceEntry? match = null;

        foreach (var registration in _services)
        {
            if (!serviceType.IsAssignableFrom(registration.Key))
                continue;

            if (match is not null)
                throw new InvalidOperationException(
                    $"Multiple service registrations match {serviceType.Name}.");

            match = registration.Value;
        }

        return match?.GetInstance();
    }

    /// <summary>
    /// Invokes an action for every registered singleton service matching the specified type.
    /// </summary>
    /// <typeparam name="T">The required service type or interface.</typeparam>
    /// <param name="action">The action to invoke.</param>
    /// <exception cref="ArgumentNullException">The action is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are not locked.</exception>
    /// <remarks>
    /// The action may execute repeatedly. Instances registered under multiple
    /// types may be invoked more than once.
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
    /// <param name="action">The action to invoke.</param>
    /// <exception cref="ArgumentNullException">The action is null.</exception>
    /// <exception cref="InvalidOperationException">Service registrations are not locked.</exception>
    /// <remarks>
    /// Successful executions are tracked by object reference across all calls.
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

    private void Register(Type type, ServiceEntry entry)
    {
        EnsureUnlocked();
        _services[type] = entry;
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
