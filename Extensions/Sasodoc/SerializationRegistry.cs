using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public class SerializationRegistry<TSerialization, TDefinition>
        where TSerialization : SerializationBase<TDefinition>
        where TDefinition : class, IEngineObjectDefinition
    {
        private readonly object _lock = new();

        private readonly Dictionary<Type, Entry> _byType = new();
        private readonly Dictionary<string, Entry> _byName =
            new(StringComparer.OrdinalIgnoreCase);

        private record Entry(
            TSerialization Serialization,
            string Name,
            Func<TDefinition> Factory
        );

        public TSerialization? GetSerialization(Type assetType)
        {
            if (assetType == null) return null;

            lock (_lock)
            {
                return _byType.TryGetValue(assetType, out var entry)
                    ? entry.Serialization
                    : null;
            }
        }

        public TDefinition? CreateDefinition(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            lock (_lock)
            {
                return _byName.TryGetValue(name, out var entry)
                    ? entry.Factory()
                    : null;
            }
        }

        public ISerialization? GetSerialization(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            lock (_lock)
            {
                return _byName.TryGetValue(name, out var entry)
                    ? entry.Serialization
                    : null;
            }
        }

        public string? GetName(Type assetType)
        {
            if (assetType == null) return null;

            lock (_lock)
            {
                return _byType.TryGetValue(assetType, out var entry)
                    ? entry.Name
                    : null;
            }
        }

        public void Register(
            string name,
            TSerialization serialization,
            Func<TDefinition> factory)
        {
            if (serialization == null)
                throw new ArgumentNullException(nameof(serialization));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            var type = typeof(TDefinition);

            lock (_lock)
            {
                if (_byType.ContainsKey(type))
                    throw new InvalidOperationException($"Type already registered: {type}");

                if (_byName.ContainsKey(name))
                    throw new InvalidOperationException($"Name already registered: {name}");

                var entry = new Entry(
                    serialization,
                    name,
                    factory
                );

                _byType[type] = entry;
                _byName[name] = entry;
            }
        }
    }
}
