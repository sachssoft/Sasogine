using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    /// <summary>
    /// Provides a thread-safe registry for serialization handlers and their
    /// associated engine object definition types.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The base type of definitions managed by this registry.
    /// </typeparam>
    public class SerializationRegistry<TDefinition>
        where TDefinition : class, IEngineObjectDefinition
    {
        private readonly object _lock = new();

        private readonly Dictionary<Type, Entry> _byType = new();
        private readonly Dictionary<string, Entry> _byName =
            new(StringComparer.OrdinalIgnoreCase);

        private sealed record Entry(
            Type DefinitionType,
            ISerialization Serialization,
            string Name,
            Func<TDefinition> Factory
        );

        /// <summary>
        /// Gets the serialization handler associated with the specified definition type.
        /// </summary>
        /// <param name="definitionType">The registered definition type.</param>
        /// <returns>
        /// The associated serialization handler, or <see langword="null"/>
        /// if the definition type is not registered.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="definitionType"/> is <see langword="null"/>.
        /// </exception>
        public ISerialization? GetSerialization(Type definitionType)
        {
            ArgumentNullException.ThrowIfNull(definitionType);

            lock (_lock)
            {
                return _byType.TryGetValue(definitionType, out var entry)
                    ? entry.Serialization
                    : null;
            }
        }

        /// <summary>
        /// Gets the serialization handler associated with the specified definition type
        /// and returns it as the requested serialization type.
        /// </summary>
        /// <typeparam name="TSerialization">
        /// The expected serialization handler type.
        /// </typeparam>
        /// <param name="definitionType">The registered definition type.</param>
        /// <returns>
        /// The serialization handler as <typeparamref name="TSerialization"/>,
        /// or <see langword="null"/> if the definition type is not registered
        /// or the handler is not compatible with the requested type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="definitionType"/> is <see langword="null"/>.
        /// </exception>
        public TSerialization? GetSerialization<TSerialization>(Type definitionType)
            where TSerialization : class, ISerialization
        {
            return GetSerialization(definitionType) as TSerialization;
        }

        /// <summary>
        /// Gets the serialization handler registered under the specified name.
        /// </summary>
        /// <param name="name">The registered name.</param>
        /// <returns>
        /// The associated serialization handler, or <see langword="null"/>
        /// if no matching registration exists.
        /// </returns>
        public ISerialization? GetSerialization(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            lock (_lock)
            {
                return _byName.TryGetValue(name.Trim(), out var entry)
                    ? entry.Serialization
                    : null;
            }
        }

        /// <summary>
        /// Gets the serialization handler registered under the specified name
        /// and returns it as the requested serialization type.
        /// </summary>
        /// <typeparam name="TSerialization">
        /// The expected serialization handler type.
        /// </typeparam>
        /// <param name="name">The registered name.</param>
        /// <returns>
        /// The serialization handler as <typeparamref name="TSerialization"/>,
        /// or <see langword="null"/> if no matching registration exists
        /// or the handler is not compatible with the requested type.
        /// </returns>
        public TSerialization? GetSerialization<TSerialization>(string name)
            where TSerialization : class, ISerialization
        {
            return GetSerialization(name) as TSerialization;
        }

        /// <summary>
        /// Creates a new definition instance using the factory registered
        /// under the specified name.
        /// </summary>
        /// <param name="name">The registered name of the definition.</param>
        /// <returns>
        /// A newly created definition instance, or <see langword="null"/>
        /// if no matching registration exists.
        /// </returns>
        public TDefinition? CreateDefinition(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Func<TDefinition>? factory;

            lock (_lock)
            {
                factory = _byName.TryGetValue(name.Trim(), out var entry)
                    ? entry.Factory
                    : null;
            }

            return factory?.Invoke();
        }

        /// <summary>
        /// Gets the registered name associated with the specified definition type.
        /// </summary>
        /// <param name="definitionType">The registered definition type.</param>
        /// <returns>
        /// The registered name, or <see langword="null"/>
        /// if the definition type is not registered.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="definitionType"/> is <see langword="null"/>.
        /// </exception>
        public string? GetName(Type definitionType)
        {
            ArgumentNullException.ThrowIfNull(definitionType);

            lock (_lock)
            {
                return _byType.TryGetValue(definitionType, out var entry)
                    ? entry.Name
                    : null;
            }
        }

        /// <summary>
        /// Gets the registered name associated with the specified definition type.
        /// </summary>
        /// <typeparam name="TConcreteDefinition">
        /// The concrete definition type whose registered name should be retrieved.
        /// </typeparam>
        /// <returns>
        /// The registered name, or <see langword="null"/>
        /// if the definition type is not registered.
        /// </returns>
        public string? GetName<TConcreteDefinition>()
            where TConcreteDefinition : class, TDefinition
        {
            return GetName(typeof(TConcreteDefinition));
        }

        /// <summary>
        /// Registers a serialization handler and factory for a concrete definition type.
        /// </summary>
        /// <typeparam name="TConcreteDefinition">
        /// The concrete definition type to register.
        /// </typeparam>
        /// <param name="name">
        /// The unique name used to identify the registration.
        /// Name matching is case-insensitive.
        /// </param>
        /// <param name="serialization">
        /// The serialization handler associated with the definition type.
        /// </param>
        /// <param name="factory">
        /// A factory used to create new instances of the definition type.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serialization"/> or <paramref name="factory"/>
        /// is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is null, empty, or consists only of white-space characters.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The definition type or name is already registered.
        /// </exception>
        public void Register<TConcreteDefinition>(
            string name,
            ISerialization serialization,
            Func<TConcreteDefinition> factory)
            where TConcreteDefinition : class, TDefinition
        {
            ArgumentNullException.ThrowIfNull(serialization);
            ArgumentNullException.ThrowIfNull(factory);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Name cannot be null, empty, or whitespace.",
                    nameof(name));
            }

            name = name.Trim();

            var definitionType = typeof(TConcreteDefinition);

            lock (_lock)
            {
                if (_byType.ContainsKey(definitionType))
                {
                    throw new InvalidOperationException(
                        $"Definition type '{definitionType.FullName}' is already registered.");
                }

                if (_byName.ContainsKey(name))
                {
                    throw new InvalidOperationException(
                        $"Name '{name}' is already registered.");
                }

                var entry = new Entry(
                    definitionType,
                    serialization,
                    name,
                    () => factory()
                );

                _byType.Add(definitionType, entry);
                _byName.Add(name, entry);
            }
        }
    }
}