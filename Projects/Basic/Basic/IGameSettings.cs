using Sachssoft.Sasogine.Resources;

namespace Sachssoft.Sasogine
{
    /// <summary>
    /// Defines a contract for loading, storing, and accessing
    /// application or game settings.
    /// </summary>
    public interface IGameSettings
    {
        /// <summary>
        /// Gets or sets the resource source used to load and save
        /// the settings data.
        /// </summary>
        ResourceSourceBase Source { get; set; }

        /// <summary>
        /// Loads the settings from the configured
        /// <see cref="Source"/>.
        /// </summary>
        void Load();

        /// <summary>
        /// Saves the current settings to the configured
        /// <see cref="Source"/>.
        /// </summary>
        void Save();

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">
        /// The expected type of the setting value.
        /// </typeparam>
        /// <param name="key">
        /// The key identifying the setting.
        /// </param>
        /// <returns>
        /// The value associated with <paramref name="key"/>,
        /// or the default value when no matching value exists.
        /// </returns>
        T? GetValue<T>(string key);

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the setting value.
        /// </typeparam>
        /// <param name="key">
        /// The key identifying the setting.
        /// </param>
        /// <param name="value">
        /// The value to associate with the specified key.
        /// </param>
        void SetValue<T>(string key, T? value);
    }
}