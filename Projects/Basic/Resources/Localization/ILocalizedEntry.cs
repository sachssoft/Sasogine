using System.Globalization;

namespace Sachssoft.Sasogine.Resources.Localization
{
    /// <summary>
    /// Represents a single localized entry which can provide a value depending on the quantity (for pluralization) 
    /// and supports lazy loading from a resource manager.
    /// </summary>
    public interface ILocalizedEntry
    {
        /// <summary>
        /// Indicates whether the entry has been loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Loads the entry from the provided resource manager using the given culture and data.
        /// </summary>
        /// <param name="culture">The culture to load the entry for.</param>
        /// <param name="resourceManager">The resource manager to use for loading associated resources.</param>
        /// <param name="data">The data describing the localized entry.</param>
        void Load(CultureInfo culture, AssetStore resourceManager, LocalizedEntryData data);

        /// <summary>
        /// Gets the value of the entry, selecting the appropriate plural form based on quantity.
        /// </summary>
        /// <param name="quantity">Anzahl der Objekte, bestimmt die passende Pluralform.</param>
        /// <returns>The localized value corresponding to the given quantity.</returns>
        object? GetValue(int quantity); // Quantity: Anzahl der Objekte, bestimmt die passende Pluralform.
    }
}
