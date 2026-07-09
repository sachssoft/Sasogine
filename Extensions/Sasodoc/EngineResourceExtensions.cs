using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Common.Localization;
using Sachssoft.Sasogine.Components.Models;
using Sachssoft.Sasogine.Resources.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public static class EngineResourceExtensions
    {
        private static readonly Dictionary<string, CultureInfo> _cultureCache =
            CultureInfo.GetCultures(CultureTypes.AllCultures)
                .ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

        #region Size
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
        public static AssetFile<T>? ReadAssetFile<T>(
            this FormatReaderBase reader,
            string property,
            AssetFile<T>? fallback = null
        )
            where T : class, IAsset
        {
            if (!reader.Contains(property))
                return fallback;

            var relativeFilePath = reader.ReadString(context: property);

            return new AssetFile<T>(relativeFilePath);
        }

        public static void WriteAssetFile<T>(
            this FormatWriterBase writer,
            string property,
            AssetFile<T>? value
        )
            where T : class, IAsset
        {
            if (value == null)
                return;

            writer.WriteString(context: property, value.FullRelativePath);
        }
        #endregion
    }
}
