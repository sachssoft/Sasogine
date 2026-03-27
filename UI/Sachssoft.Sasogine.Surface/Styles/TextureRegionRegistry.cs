using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Styles
{
    // AOT-Freundliche Registrierung von TextureRegion-Fabriken
    public static class TextureRegionRegistry
    {
        private static readonly Dictionary<Type, TextureRegionFactoryDelegate> _factoriesByType = new();
        private static readonly Dictionary<string, TextureRegionFactoryDelegate> _factoriesByKey = new(StringComparer.OrdinalIgnoreCase);

        static TextureRegionRegistry()
        {
            // Vordefinierte Regionstypen registrieren
            //Register<ColoredRegion>("ColoredRegion", () => new ColoredRegion());
            //Register<MultiTextureRegion>("MultiTextureRegion", () => new MultiTextureRegion());

            Register<NinePatchRegion>(
                "NinePatchRegion",
                (texture, map) => new NinePatchRegion(
                    texture,
                    new Microsoft.Xna.Framework.Rectangle(
                        map.Get("Left").ConvertTo<int>(0),
                        map.Get("Top").ConvertTo<int>(0),
                        map.Get("Width").ConvertTo<int>(texture.Width),
                        map.Get("Height").ConvertTo<int>(texture.Height)
                    ),
                    map.Get("NinePatchLeft").ConvertTo<int>(0),
                    map.Get("NinePatchTop").ConvertTo<int>(0),
                    map.Get("NinePatchRight").ConvertTo<int>(0),
                    map.Get("NinePatchBottom").ConvertTo<int>(0)
                )
            );

            Register<TextureRegion>(
                "TextureRegion",
                (texture, map) => new TextureRegion(
                    texture,
                    new Microsoft.Xna.Framework.Rectangle(
                        map.Get("Left").ConvertTo<int>(0),
                        map.Get("Top").ConvertTo<int>(0),
                        map.Get("Width").ConvertTo<int>(texture.Width),
                        map.Get("Height").ConvertTo<int>(texture.Height)
                    )
                )
            );

            //Register<ThreePatchRegion>("ThreePatchRegion", () => new ThreePatchRegion());
            //Register<TiledRegion>("TiledRegion", () => new TiledRegion());
        }

        /// <summary>
        /// Register a factory with a string key.
        /// </summary>
        public static void Register<T>(string key, TextureRegionFactoryDelegate factory) where T : ITextureRegion
        {
            if (_factoriesByKey.ContainsKey(key))
                throw new InvalidOperationException($"A factory for key '{key}' is already registered.");

            _factoriesByKey[key] = factory;
        }

        /// <summary>
        /// Create instance by type.
        /// </summary>
        public static ITextureRegion CreateInstance(Type type, Texture2D texture, PropertyMap map)
        {
            if (_factoriesByType.TryGetValue(type, out var factory))
                return factory.Invoke(texture, map);

            throw new InvalidOperationException($"No factory registered for region type '{type.FullName}'.");
        }

        /// <summary>
        /// Create instance by key.
        /// </summary>
        public static ITextureRegion CreateInstance(string key, Texture2D texture, PropertyMap map)
        {
            if (_factoriesByKey.TryGetValue(key, out var factory))
                return factory.Invoke(texture, map);

            throw new InvalidOperationException($"No factory registered for key '{key}'.");
        }
    }
}
