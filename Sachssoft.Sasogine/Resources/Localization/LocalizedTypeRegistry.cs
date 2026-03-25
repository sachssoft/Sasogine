using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources.Localization
{
    /// <summary>
    /// Registry for localized types with mandatory string typeName and lazy instance creation.
    /// Thread-safe.
    /// </summary>
    public sealed class LocalizedTypeRegistry
    {
        private readonly ConcurrentDictionary<string, RegisteredTypeEntry> _registeredByName =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<Type, RegisteredTypeEntry> _registeredByType =
            new();

        /// <summary>
        /// Register a localized type with a mandatory string key (typeName) and class type T.
        /// </summary>
        public void Register<T>(string typeName)
            where T : class, ILocalizedEntry, new()
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

            var type = typeof(T);

            var entry = new RegisteredTypeEntry(typeName, type, () => new T());

            if (!_registeredByName.TryAdd(typeName, entry))
                throw new InvalidOperationException($"TypeName '{typeName}' is already registered.");

            if (!_registeredByType.TryAdd(type, entry))
                throw new InvalidOperationException($"Type '{type.FullName}' is already registered.");
        }

        /// <summary>
        /// Create an instance by Type. Throws if not registered.
        /// </summary>
        public ILocalizedEntry Create(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!_registeredByType.TryGetValue(type, out var entry))
                throw new KeyNotFoundException($"Type '{type.FullName}' is not registered.");

            return entry.Factory();
        }

        /// <summary>
        /// Create an instance by mandatory typeName. Throws if not registered.
        /// </summary>
        public ILocalizedEntry Create(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentNullException(nameof(typeName));

            if (!_registeredByName.TryGetValue(typeName, out var entry))
                throw new KeyNotFoundException($"TypeName '{typeName}' is not registered.");

            return entry.Factory();
        }

        /// <summary>
        /// Check if a Type is registered
        /// </summary>
        public bool IsRegistered(Type type) => type != null && _registeredByType.ContainsKey(type);

        /// <summary>
        /// Check if a TypeName is registered
        /// </summary>
        public bool IsRegistered(string typeName) => !string.IsNullOrWhiteSpace(typeName) && _registeredByName.ContainsKey(typeName);

        /// <summary>
        /// Internal registration entry
        /// </summary>
        private sealed class RegisteredTypeEntry
        {
            public string TypeName { get; }
            public Type Type { get; }
            public Func<ILocalizedEntry> Factory { get; }

            public RegisteredTypeEntry(string typeName, Type type, Func<ILocalizedEntry> factory)
            {
                TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
                Type = type ?? throw new ArgumentNullException(nameof(type));
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }
        }
    }
}