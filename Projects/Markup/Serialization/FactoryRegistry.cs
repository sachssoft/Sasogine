using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Markup.Serialization
{
    /// <summary>
    /// Provides a thread-safe registry that maps names and implementation types to object factories.
    /// </summary>
    /// <typeparam name="TObject">
    /// The common base type produced by registered factories.
    /// </typeparam>
    public sealed class FactoryRegistry<TObject>
        where TObject : class
    {
        private readonly object _lock = new();

        private readonly Dictionary<Type, Entry> _byType = new();
        private readonly Dictionary<string, Entry> _byName =
            new(StringComparer.OrdinalIgnoreCase);

        private sealed record Entry(
            Type Type,
            string Name,
            Func<TObject> Factory
        );

        /// <summary>
        /// Registers a factory for the specified implementation type and name.
        /// </summary>
        /// <typeparam name="TImplementation">The generic TImplementation type.</typeparam>
        /// <param name="name">The registered name.</param>
        /// <param name="factory">The factory used to create instances.</param>
        public void Register<TImplementation>(
            string name,
            Func<TObject> factory)
            where TImplementation : TObject
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Name cannot be null or whitespace.",
                    nameof(name));

            ArgumentNullException.ThrowIfNull(factory);

            name = name.Trim();
            var type = typeof(TImplementation);

            lock (_lock)
            {
                if (_byType.ContainsKey(type))
                    throw new InvalidOperationException(
                        $"Type '{type.FullName}' is already registered.");

                if (_byName.ContainsKey(name))
                    throw new InvalidOperationException(
                        $"Name '{name}' is already registered.");

                var entry = new Entry(
                    type,
                    name,
                    factory);

                _byType.Add(type, entry);
                _byName.Add(name, entry);
            }
        }

        /// <summary>
        /// Creates an object using the factory registered under the specified name.
        /// </summary>
        /// <param name="name">The registered name.</param>
        /// <returns>A newly created object.</returns>
        public TObject CreateInstance(string name)
        {
            if (!TryCreateInstance(name, out var instance))
                throw new KeyNotFoundException(
                    $"No factory registered with name '{name}'.");

            return instance;
        }

        /// <summary>
        /// Attempts to create an object using the factory registered under the specified name.
        /// </summary>
        /// <param name="name">The registered name.</param>
        /// <param name="instance">When this method returns, contains the created instance when successful.</param>
        /// <returns><see langword="true"/> if an instance was created; otherwise, <see langword="false"/>.</returns>
        public bool TryCreateInstance(
            string? name,
            out TObject instance)
        {
            instance = null!;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            Entry? entry;

            lock (_lock)
            {
                if (!_byName.TryGetValue(name.Trim(), out entry))
                    return false;
            }

            var created = entry.Factory();

            if (created is null)
                return false;

            instance = created;
            return true;
        }

        /// <summary>
        /// Gets the registered name associated with the specified implementation type.
        /// </summary>
        /// <param name="type">The implementation type.</param>
        /// <returns>The registered name, or <see langword="null"/> when the type is not registered.</returns>
        public string? GetName(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            lock (_lock)
            {
                return _byType.TryGetValue(type, out var entry)
                    ? entry.Name
                    : null;
            }
        }

        /// <summary>
        /// Gets the registered name associated with the specified implementation type.
        /// </summary>
        /// <typeparam name="TImplementation">The generic TImplementation type.</typeparam>
        /// <returns>The registered name, or <see langword="null"/> when the type is not registered.</returns>
        public string? GetName<TImplementation>()
            where TImplementation : TObject
        {
            return GetName(typeof(TImplementation));
        }

        /// <summary>
        /// Attempts to get the registered name associated with the specified implementation type.
        /// </summary>
        /// <param name="type">The implementation type.</param>
        /// <param name="name">The registered name.</param>
        /// <returns><see langword="true"/> if a name was found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetName(
            Type type,
            out string name)
        {
            ArgumentNullException.ThrowIfNull(type);

            lock (_lock)
            {
                if (_byType.TryGetValue(type, out var entry))
                {
                    name = entry.Name;
                    return true;
                }
            }

            name = string.Empty;
            return false;
        }

        /// <summary>
        /// Determines whether the specified registration exists.
        /// </summary>
        /// <param name="name">The registered name.</param>
        /// <returns><see langword="true"/> if the registration exists; otherwise, <see langword="false"/>.</returns>
        public bool IsRegistered(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            lock (_lock)
            {
                return _byName.ContainsKey(name.Trim());
            }
        }

        /// <summary>
        /// Determines whether the specified registration exists.
        /// </summary>
        /// <typeparam name="TImplementation">The generic TImplementation type.</typeparam>
        /// <returns><see langword="true"/> if the registration exists; otherwise, <see langword="false"/>.</returns>
        public bool IsRegistered<TImplementation>()
            where TImplementation : TObject
        {
            lock (_lock)
            {
                return _byType.ContainsKey(typeof(TImplementation));
            }
        }

        /// <summary>
        /// Gets a snapshot containing the names of all registered factories.
        /// </summary>
        /// <returns>A snapshot of the registered names.</returns>
        public IReadOnlyList<string> GetRegisteredNames()
        {
            lock (_lock)
            {
                return _byName.Keys.ToArray();
            }
        }
    }
}
