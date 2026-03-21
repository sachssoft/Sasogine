using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Styles
{

    // AOT-Freundliche Registrierung von TextureRegion-Fabriken
    public static class BrushRegistry
    {
        private static readonly Dictionary<Type, BrushFactoryDelegate> _factoriesByType = new();
        private static readonly Dictionary<string, BrushFactoryDelegate> _factoriesByKey = new(StringComparer.OrdinalIgnoreCase);

        static BrushRegistry()
        {
            Register<SolidColorBrush>(
                nameof(SolidColorBrush),
                (sheet, map) => new SolidColorBrush(map.Get("Color").ConvertTo<Color>())
            );

            //Register<ThreePatchRegion>("ThreePatchRegion", () => new ThreePatchRegion());
            //Register<TiledRegion>("TiledRegion", () => new TiledRegion());
        }

        /// <summary>
        /// Register a factory with a string key.
        /// </summary>
        public static void Register<T>(string key, BrushFactoryDelegate factory)
            where T : IBrush
        {
            if (_factoriesByKey.ContainsKey(key))
                throw new InvalidOperationException($"A factory for key '{key}' is already registered.");

            _factoriesByKey[key] = factory;
        }

        public static bool IsRegistered(string key)
        {
            return _factoriesByKey.ContainsKey(key);
        }

        /// <summary>
        /// Create instance by type.
        /// </summary>
        public static IBrush CreateInstance(Stylesheet sheet, Type type, PropertyMap map)
        {
            if (_factoriesByType.TryGetValue(type, out var factory))
                return factory.Invoke(sheet, map);

            throw new InvalidOperationException($"No factory registered for region type '{type.FullName}'.");
        }

        /// <summary>
        /// Create instance by key.
        /// </summary>
        public static IBrush CreateInstance(Stylesheet sheet, string key, PropertyMap map)
        {
            if (_factoriesByKey.TryGetValue(key, out var factory))
                return factory.Invoke(sheet, map);

            throw new InvalidOperationException($"No factory registered for key '{key}'.");
        }
    }
}
