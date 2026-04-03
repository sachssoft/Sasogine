using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public class ResourceRegistry
    {
        private readonly Dictionary<string, IResourceFactory> _typeFactories = new();

        public ResourceRegistry()
        {
            RegisterDefaults();
        }

        // Registrierung über IResourceFactory
        public void Register<T>(string name, IResourceFactory factory)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Resource name cannot be null or empty.", nameof(name));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (_typeFactories.ContainsKey(name))
                throw new InvalidOperationException($"A factory for '{name}' is already registered.");

            _typeFactories.Add(name, factory);
        }

        // Registrierung über Delegate
        public void Register<T>(string name, Func<Skin, PropertyMap, T> factory) where T : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            Register<T>(name, new ResourceFactoryWrapper<T>(factory));
        }

#if SASOGINE
        protected virtual void RegisterDefaults()
        {
            Register<TextureAtlas>(nameof(TextureAtlas), (s, v, ta) =>
            {

            });
        }
#else
        protected virtual void RegisterDefaults()
        {
            // Später für andere Plattformen/engines implementieren
        }
#endif

        // Create über Type
        public object Create(string name, Type type, Skin skin, PropertyMap values)
        {
            if (!_typeFactories.TryGetValue(name, out var factory))
                throw new InvalidOperationException($"No resource factory registered for '{name}'.");

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var instance = factory.Create(type, skin, values);

            if (instance == null)
                throw new InvalidOperationException($"Factory for '{name}' returned null.");

            if (!type.IsAssignableFrom(instance.GetType()))
                throw new InvalidOperationException($"Factory for '{name}' returned invalid type '{instance.GetType().Name}', expected '{type.Name}'.");

            return instance;
        }

        // Typsichere Variante
        public T Create<T>(string name, Skin skin, PropertyMap values) where T : class
        {
            var result = Create(name, typeof(T), skin, values);

            return (T)result;
        }

        // Wrapper für Delegate
        private class ResourceFactoryWrapper<T> : IResourceFactory where T : class
        {
            private readonly Func<Skin, PropertyMap, T> _factory;

            public ResourceFactoryWrapper(Func<Skin, PropertyMap, T> factory)
            {
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public object Create(Type targetType, Skin skin, PropertyMap values)
            {
                var result = _factory.Invoke(skin, values);

                if (result == null)
                    throw new InvalidOperationException($"Factory returned null for type '{typeof(T).Name}'.");

                if (!targetType.IsAssignableFrom(result.GetType()))
                    throw new InvalidOperationException($"Factory returned type '{result.GetType().Name}', expected '{targetType.Name}'.");

                return result;
            }
        }
    }
}