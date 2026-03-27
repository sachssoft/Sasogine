using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine
{
    public class GameRegistry
    {
        private readonly Dictionary<string, Func<object>> _stringFactories = new();
        private readonly Dictionary<int, Func<object>> _intFactories = new();
        private readonly Dictionary<Type, Func<object>> _typeFactories = new();

        // 1) String-ID
        public void Register(string id, Type type, Func<object> factory)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _stringFactories[id] = factory;
        }

        public void Register<T>(string id, Func<T> factory) where T : class
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _stringFactories[id] = () => factory();
        }

        // 2) Int-ID
        public void Register(int id, Type type, Func<object> factory)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _intFactories[id] = factory;
        }

        public void Register<T>(int id, Func<T> factory) where T : class
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _intFactories[id] = () => factory();
        }

        // 3) Typ-only (nur Klassen mit parameterlosem Konstruktor)
        public void Register<T>() where T : class, new()
        {
            _typeFactories[typeof(T)] = () => new T();
        }

        public void Register<T>(Func<T> factory) where T : class
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _typeFactories[typeof(T)] = () => factory();
        }

        // String-ID
        public object Create(string id)
        {
            if (_stringFactories.TryGetValue(id, out var factory))
                return factory();

            throw new InvalidOperationException($"No factory registered with string ID '{id}'");
        }

        // Int-ID
        public object Create(int id)
        {
            if (_intFactories.TryGetValue(id, out var factory))
                return factory();

            throw new InvalidOperationException($"No factory registered with int ID '{id}'");
        }

        // Typ-only
        public T Create<T>() where T : class
        {
            if (_typeFactories.TryGetValue(typeof(T), out var factory))
                return (T)factory();

            throw new InvalidOperationException($"No factory registered for type {typeof(T).FullName}");
        }

        public bool IsRegistered(string id) => _stringFactories.ContainsKey(id);
        public bool IsRegistered(int id) => _intFactories.ContainsKey(id);
        public bool IsRegistered<T>() => _typeFactories.ContainsKey(typeof(T));
        public bool IsRegistered(Type type) => _typeFactories.ContainsKey(type);
    }
}