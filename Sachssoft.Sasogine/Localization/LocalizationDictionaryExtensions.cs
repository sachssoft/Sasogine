using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Localization
{
    /// <summary>
    /// Provides strongly-typed extension methods for convenient access to localized resources
    /// (strings, textures, and sounds) from a <see cref="LocalizedDictionary"/>.
    /// </summary>
    public static class LocalizationDictionaryExtensions
    {
        /// <summary>
        /// Retrieves a localized <see cref="string"/> from the specified <see cref="LocalizedDictionary"/>.
        /// Returns <c>null</c> if the key does not exist and no default value is provided.
        /// </summary>
        /// <param name="dict">The dictionary to query.</param>
        /// <param name="key">The localization key to look up.</param>
        /// <param name="defaultValue">The default value to return if the key is not found.</param>
        /// <returns>
        /// The localized string if found; otherwise, <paramref name="defaultValue"/> or <c>null</c>.
        /// </returns>
        public static string? GetString(this LocalizedDictionary dict, string key, string? defaultValue = null)
        {
            return dict?.GetValue(key, defaultValue);
        }

        /// <summary>
        /// Retrieves a localized <see cref="Texture2D"/> from the specified <see cref="LocalizedDictionary"/>.
        /// </summary>
        /// <param name="dict">The dictionary to query.</param>
        /// <param name="key">The localization key to look up.</param>
        /// <param name="defaultValue">The default texture to return if the key is not found.</param>
        /// <returns>
        /// The localized <see cref="Texture2D"/> if found; otherwise, <paramref name="defaultValue"/> or <c>null</c>.
        /// </returns>
        public static Texture2D? GetTexture(this LocalizedDictionary dict, string key, Texture2D? defaultValue = null)
        {
            return dict?.GetValue(key, defaultValue);
        }

        /// <summary>
        /// Retrieves a localized <see cref="SoundEffect"/> from the specified <see cref="LocalizedDictionary"/>.
        /// </summary>
        /// <param name="dict">The dictionary to query.</param>
        /// <param name="key">The localization key to look up.</param>
        /// <param name="defaultValue">The default sound effect to return if the key is not found.</param>
        /// <returns>
        /// The localized <see cref="SoundEffect"/> if found; otherwise, <paramref name="defaultValue"/> or <c>null</c>.
        /// </returns>
        public static SoundEffect? GetSound(this LocalizedDictionary dict, string key, SoundEffect? defaultValue = null)
        {
            return dict?.GetValue(key, defaultValue);
        }
    }
}
