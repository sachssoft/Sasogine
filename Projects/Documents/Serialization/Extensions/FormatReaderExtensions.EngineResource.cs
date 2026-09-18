using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Resources.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sachssoft.Sasogine.Documents.Serialization
{
    /// <summary>
    /// Provides markup serialization extensions for reading EngineResource values.
    /// </summary>
    public static partial class FormatReaderExtensions
    {
        private static readonly Dictionary<string, CultureInfo> _cultureCache =
            CultureInfo.GetCultures(CultureTypes.AllCultures)
                .ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Reads a <see cref="MultilingualValue{T}"/> from the specified property.
        /// </summary>
        /// <typeparam name="T">
        /// The type of value stored for each language.
        /// </typeparam>
        /// <param name="reader">
        /// The format reader.
        /// </param>
        /// <param name="property">
        /// The property name.
        /// </param>
        /// <param name="readLanguageItem">
        /// The callback used to read an individual language-specific value.
        /// </param>
        /// <param name="invariantLanguage">
        /// The language used when no explicit language is specified.
        /// Defaults to English when not provided.
        /// </param>
        /// <param name="fallback">
        /// The value returned when the property cannot be read.
        /// </param>
        /// <returns>
        /// The deserialized multilingual value, or the supplied fallback when no
        /// usable value is available.
        /// </returns>
        public static MultilingualValue<T>? ReadMultilingualValue<T>(
            this FormatReaderBase reader,
            string property,
            Func<FormatReaderBase, string, T> readLanguageItem,
            Language? invariantLanguage = null,
            MultilingualValue<T>? fallback = null)
        {
            var readers = reader.ReadArray(property);

            if (readers is null)
                return fallback;

            var dict = new Dictionary<Language, T>();

            foreach (var languageReader in readers)
            {
                if (!languageReader.Contains("Language"))
                    continue;

                var languageName = languageReader.ReadString("Language");
                Language? language;

                if (string.IsNullOrEmpty(languageName))
                {
                    language = invariantLanguage ?? Languages.English;
                }
                else if (_languageCache.TryGetValue(languageName, out language))
                {
                }
                else
                {
                    continue;
                }

                if (!dict.ContainsKey(language))
                {
                    dict[language] = readLanguageItem(
                        languageReader,
                        "Value");
                }
            }

            return new MultilingualValue<T>(dict);
        }


        /// <summary>
        /// Reads a Reference value from the specified markup property.
        /// </summary>
        /// <typeparam name="T">The generic T type.</typeparam>
        /// <param name="reader">The markup reader.</param>
        /// <param name="property">The property name.</param>
        /// <param name="fallback">The value returned when the property cannot be read.</param>
        /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
        public static Reference<T>? ReadReference<T>(
            this FormatReaderBase reader,
            string property,
            Reference<T>? fallback = null
        )
            where T : class, IEngineReferenceable
        {
            if (!reader.Contains(property))
                return fallback;

            var id = reader.ReadString(context: property);

            return new Reference<T>
            {
                Id = id,
            };
        }


        /// <summary>
        /// Reads a AssetFile value from the specified markup property.
        /// </summary>
        /// <typeparam name="T">The generic T type.</typeparam>
        /// <param name="reader">The markup reader.</param>
        /// <param name="property">The property name.</param>
        /// <param name="fallback">The value returned when the property cannot be read.</param>
        /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
        public static AssetFile<T>? ReadAssetFile<T>(
            this FormatReaderBase reader,
            string property,
            AssetFile<T>? fallback = null
        )
            where T : class, IAssetDefinition
        {
            if (!reader.Contains(property))
                return fallback;

            var relativeFilePath = reader.ReadString(context: property);

            return new AssetFile<T>(relativeFilePath);
        }
    }
}
