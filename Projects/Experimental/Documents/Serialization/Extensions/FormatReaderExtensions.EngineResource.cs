using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Common.Localization;
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
        /// Reads a CulturedValue value from the specified markup property.
        /// </summary>
        /// <typeparam name="T">The generic T type.</typeparam>
        /// <param name="reader">The markup reader.</param>
        /// <param name="property">The property name.</param>
        /// <param name="readCulturedItem">The callback used to read an individual culture-specific value.</param>
        /// <param name="fallback">The value returned when the property cannot be read.</param>
        /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
        public static CulturedValue<T>? ReadCulturedValue<T>(
            this FormatReaderBase reader,
            string property,
            Func<FormatReaderBase, string, T> readCulturedItem,
            CulturedValue<T>? fallback = null
        )
        {
            var readers = reader.ReadArray(property);

            if (readers == null)
                return fallback;

            var dict = new Dictionary<CultureInfo, T>();
            foreach (var cultureReader in readers)
            {
                if (!cultureReader.Contains("Culture"))
                    continue;

                var cultureName = cultureReader.ReadString("Culture");
                CultureInfo? culture;

                if (string.IsNullOrEmpty(cultureName))
                {
                    culture = CultureInfo.InvariantCulture;
                }
                else if (_cultureCache.TryGetValue(cultureName, out culture))
                { }
                else
                {
                    continue;
                }

                if (!dict.ContainsKey(culture))
                {
                    dict[culture] = readCulturedItem(cultureReader, "Value");
                }
            }

            return new CulturedValue<T>(dict);
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
