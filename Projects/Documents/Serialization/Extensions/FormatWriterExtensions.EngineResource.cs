using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Documents.Serialization
{
    /// <summary>
    /// Provides markup serialization extensions for reading EngineResource values.
    /// </summary>
    public static partial class FormatWriterExtensions
    {

        ///// <summary>
        ///// Writes a CulturedValue value to the specified markup property.
        ///// </summary>
        ///// <typeparam name="T">The generic T type.</typeparam>
        ///// <param name="writer">The markup writer.</param>
        ///// <param name="property">The property name.</param>
        ///// <param name="value">The value to write.</param>
        ///// <param name="writeCulturedItem">The callback used to write an individual culture-specific value.</param>
        //public static void WriteCulturedValue<T>(
        //    this FormatWriterBase writer,
        //    string property,
        //    CulturedValue<T>? value,
        //    Action<FormatWriterBase, string, T?> writeCulturedItem
        //)
        //{
        //    if (value == null)
        //        return;

        //    var writers = new List<FormatWriterBase>();
        //    foreach (var culture in value.Cultures)
        //    {
        //        var cultureWriter = writer.CreateWriter();
        //        cultureWriter.WriteString("Culture", culture.Name);
        //        writeCulturedItem(cultureWriter, "Value", value.Get(culture));
        //        writers.Add(cultureWriter);
        //    }

        //    writer.WriteArray(property, writers.ToArray());
        //}


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
    }
}
