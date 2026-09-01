using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Experimental.Components
{
    /// <summary>
    /// Provides a base implementation for components with supporting services.
    /// </summary>
    public abstract class ComponentBase :
        IComponent,
        IUpdatableComponent,
        IServiceProvider
    {
        private readonly List<IComponentService> _componentServices = new();

        /// <summary>
        /// Updates the component and its registered services.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        public virtual void Update(SceneUpdateContext context)
        {
            for (int i = 0; i < _componentServices.Count; i++)
            {
                if (_componentServices[i] is IUpdatableComponent updatableService)
                {
                    updatableService.Update(context);
                }
            }
        }

        /// <summary>
        /// Gets a registered service of the specified type.
        /// </summary>
        /// <param name="serviceType">
        /// The type of service to retrieve.
        /// </param>
        /// <returns>
        /// The registered service.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// No service of the specified type is registered.
        /// </exception>
        public IComponentService GetService(Type serviceType)
        {
            ArgumentNullException.ThrowIfNull(serviceType);

            if (TryGetService(serviceType, out var service))
                return service;

            throw new InvalidOperationException(
                $"No component service of type '{serviceType.FullName}' is registered.");
        }

        /// <summary>
        /// Gets a registered service of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type of service to retrieve.
        /// </typeparam>
        /// <returns>
        /// The registered service.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// No service of the specified type is registered.
        /// </exception>
        public T GetService<T>()
            where T : class, IComponentService
        {
            if (TryGetService<T>(out var service))
                return service;

            throw new InvalidOperationException(
                $"No component service of type '{typeof(T).FullName}' is registered.");
        }

        /// <summary>
        /// Attempts to get a registered service of the specified type.
        /// </summary>
        /// <param name="serviceType">
        /// The type of service to retrieve.
        /// </param>
        /// <param name="service">
        /// When this method returns, contains the registered service if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the service was found; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> is <see langword="null"/>.
        /// </exception>
        public bool TryGetService(
            Type serviceType,
            [MaybeNullWhen(false)] out IComponentService service)
        {
            ArgumentNullException.ThrowIfNull(serviceType);

            for (int i = 0; i < _componentServices.Count; i++)
            {
                IComponentService value = _componentServices[i];

                if (serviceType.IsInstanceOfType(value))
                {
                    service = value;
                    return true;
                }
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Attempts to get a registered service of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type of service to retrieve.
        /// </typeparam>
        /// <param name="service">
        /// When this method returns, contains the registered service if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the service was found; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetService<T>(
            [MaybeNullWhen(false)] out T service)
            where T : class, IComponentService
        {
            for (int i = 0; i < _componentServices.Count; i++)
            {
                if (_componentServices[i] is T value)
                {
                    service = value;
                    return true;
                }
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Adds a service to this component.
        /// </summary>
        /// <param name="service">
        /// The service to add.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// A service of the same type is already registered.
        /// </exception>
        protected void AddService(IComponentService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            Type serviceType = service.GetType();

            for (int i = 0; i < _componentServices.Count; i++)
            {
                if (_componentServices[i].GetType() == serviceType)
                {
                    throw new InvalidOperationException(
                        $"A component service of type '{serviceType.FullName}' is already registered.");
                }
            }

            _componentServices.Add(service);
        }

        object? IServiceProvider.GetService(Type serviceType)
        {
            return TryGetService(
                serviceType,
                out var service)
                ? service
                : null;
        }
    }
}