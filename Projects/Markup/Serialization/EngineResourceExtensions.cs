using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Common.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sachssoft.Sasogine.Markup.Serialization
{
    /// <summary>
    /// Provides markup serialization extensions for Sasogine resource and reference types.
    /// </summary>
    public static class EngineResourceExtensions
    {
        private static readonly Dictionary<string, CultureInfo> _cultureCache =
            CultureInfo.GetCultures(CultureTypes.AllCultures)
                .ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

        #region Size
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
        /// Writes a CulturedValue value to the specified markup property.
        /// </summary>
        /// <typeparam name="T">The generic T type.</typeparam>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writeCulturedItem">The callback used to write an individual culture-specific value.</param>
        public static void WriteCulturedValue<T>(
            this FormatWriterBase writer,
            string property,
            CulturedValue<T>? value,
            Action<FormatWriterBase, string, T?> writeCulturedItem
        )
        {
            if (value == null)
                return;

            var writers = new List<FormatWriterBase>();
            foreach (var culture in value.Cultures)
            {
                var cultureWriter = writer.CreateWriter();
                cultureWriter.WriteString("Culture", culture.Name);
                writeCulturedItem(cultureWriter, "Value", value.Get(culture));
                writers.Add(cultureWriter);
            }

            writer.WriteArray(property, writers.ToArray());
        }
        #endregion

        #region Reference
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
        /// Writes a Reference value to the specified markup property.
        /// </summary>
        /// <typeparam name="T">The generic T type.</typeparam>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteReference<T>(
            this FormatWriterBase writer,
            string property,
            Reference<T>? value
        )
            where T : class, IEngineReferenceable
        {
            if (value == null)
                return;

            writer.WriteString(context: property, value.Id);
        }
        #endregion

        #region AssetFile
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

        /// <summary>
        /// Writes a AssetFile value to the specified markup property.
        /// </summary>
        /// <typeparam name="T">The generic T type.</typeparam>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteAssetFile<T>(
            this FormatWriterBase writer,
            string property,
            AssetFile<T>? value
        )
            where T : class, IAssetDefinition
        {
            if (value == null)
                return;

            writer.WriteString(context: property, value.FullRelativePath);
        }
        #endregion
    }
}
