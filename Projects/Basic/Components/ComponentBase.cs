using Sachssoft.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Components
{
    /// <summary>
    /// Provides a base implementation for components with supporting services
    /// and child components.
    /// </summary>
    public abstract class ComponentBase :
        IComponent,
        IUpdatableComponent,
        IDrawableComponent,
        IServiceProvider
    {
        private readonly List<IComponentService> _componentServices = new();
        private readonly List<IComponent> _components = new();

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
                    updatableService.Update(context);
            }

            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is IUpdatableComponent updatableComponent)
                    updatableComponent.Update(context);
            }
        }

        /// <summary>
        /// Draws all drawable child components.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene draw operation.
        /// </param>
        public virtual void Draw(SceneDrawContext context)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is IDrawableComponent drawableComponent)
                    drawableComponent.Draw(context);
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is <see langword="null"/>.
        /// </exception>
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
            return TryGetService(serviceType, out var service)
                ? service
                : null;
        }

        /// <summary>
        /// Adds a child component of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type under which the component is registered.
        /// </typeparam>
        /// <param name="component">
        /// The component to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="component"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// A component matching the specified type is already registered.
        /// </exception>
        protected void AddComponent<T>(T component)
            where T : class, IComponent
        {
            ArgumentNullException.ThrowIfNull(component);

            if (TryGetComponent<T>(out _))
            {
                throw new InvalidOperationException(
                    $"A component of type '{typeof(T).FullName}' is already registered.");
            }

            _components.Add(component);
        }

        /// <summary>
        /// Removes the child component of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type of component to remove.
        /// </typeparam>
        /// <returns>
        /// <see langword="true"/> if the component was found and removed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected bool RemoveComponent<T>()
            where T : class, IComponent
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T)
                {
                    _components.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the component of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type of component to retrieve.
        /// </typeparam>
        /// <returns>
        /// The registered component.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// No component of the specified type is registered.
        /// </exception>
        public T GetComponent<T>()
            where T : class, IComponent
        {
            if (TryGetComponent<T>(out var component))
                return component;

            throw new InvalidOperationException(
                $"No component of type '{typeof(T).FullName}' is registered.");
        }

        /// <summary>
        /// Attempts to get the component of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type of component to retrieve.
        /// </typeparam>
        /// <param name="component">
        /// When this method returns, contains the component if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the component was found; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetComponent<T>(
            [MaybeNullWhen(false)] out T component)
            where T : class, IComponent
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T value)
                {
                    component = value;
                    return true;
                }
            }

            component = null;
            return false;
        }
    }
}