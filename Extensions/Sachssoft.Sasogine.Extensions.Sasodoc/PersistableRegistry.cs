using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public class PersistableRegistry
    {
        private readonly Dictionary<string, IEntry> _entries = new();

        public string? TypePropertyName { get; set; } = "_type";

        private interface IEntry
        {
            string Name { get; }
            Type ComponentType { get; }
            ISerialization Create();
        }

        private class Entry<T> : IEntry where T : ISerialization
        {
            public string Name { get; }
            public Type ComponentType => typeof(T);
            private readonly Func<T> _factory;

            public Entry(string name, Func<T> factory)
            {
                Name = name;
                _factory = factory;
            }

            public ISerialization Create() => _factory();
        }

        public PersistableRegistry Register<T>(string name, Func<T> factory)
            where T : ISerialization
        {
            _entries[name] = new Entry<T>(name, factory);
            return this;
        }

        public string? FindName(Type componentType)
        {
            foreach (var entry in _entries.Values)
            {
                if (entry.ComponentType == componentType)
                    return entry.Name;
            }

            return null;
        }

        public ISerialization? Create(string name)
        {
            return _entries.TryGetValue(name, out var entry)
                ? entry.Create()
                : null;
        }

        public bool TryCreate(string? name, [MaybeNullWhen(false)] out ISerialization comp)
        {
            if (!string.IsNullOrEmpty(name) &&
                _entries.TryGetValue(name, out var entry))
            {
                comp = entry.Create();
                return true;
            }

            comp = null;
            return false;
        }

        public IEnumerable<ISerialization> CreateAll()
        {
            foreach (var entry in _entries.Values)
            {
                yield return entry.Create();
            }
        }

        public IEnumerable<T> CreateAll<T>()
            where T : ISerialization
        {
            foreach (var entry in _entries.Values)
            {
                if (entry.ComponentType == typeof(T))
                    yield return (T)entry.Create();
            }
        }

    }
}
