using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
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

        public TObject CreateInstance(string name)
        {
            if (!TryCreateInstance(name, out var instance))
                throw new KeyNotFoundException(
                    $"No factory registered with name '{name}'.");

            return instance;
        }

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
                if (!_byName.TryGetValue(name, out entry))
                    return false;
            }

            var created = entry.Factory();

            if (created is null)
                return false;

            instance = created;
            return true;
        }

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

        public string? GetName<TImplementation>()
            where TImplementation : TObject
        {
            return GetName(typeof(TImplementation));
        }

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

        public bool IsRegistered(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            lock (_lock)
            {
                return _byName.ContainsKey(name);
            }
        }

        public bool IsRegistered<TImplementation>()
            where TImplementation : TObject
        {
            lock (_lock)
            {
                return _byType.ContainsKey(typeof(TImplementation));
            }
        }

        public IReadOnlyList<string> GetRegisteredNames()
        {
            lock (_lock)
            {
                return _byName.Keys.ToArray();
            }
        }
    }
}