using Sachssoft.Documents;
using Sachssoft.Engine.Assets;
using Sachssoft.Engine.Common;
using Sachssoft.Engine.Resources.Localization;
using System;
using System.Collections.Generic;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Provides markup serialization extensions for writing EngineResource values.
/// </summary>
public static partial class FormatWriterExtensions
{
    /// <summary>
    /// Writes a <see cref="MultilingualValue{T}"/> to the specified property.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value stored for each language.
    /// </typeparam>
    /// <param name="writer">
    /// The format writer.
    /// </param>
    /// <param name="property">
    /// The property name.
    /// </param>
    /// <param name="value">
    /// The multilingual value to write.
    /// </param>
    /// <param name="writeLanguageItem">
    /// The callback used to write an individual language-specific value.
    /// </param>
    public static void WriteMultilingualValue<T>(
        this FormatWriterBase writer,
        string property,
        MultilingualValue<T>? value,
        Action<FormatWriterBase, string, T?> writeLanguageItem)
    {
        if (value is null)
            return;

        var writers = new List<FormatWriterBase>();

        foreach (var language in value.Languages)
        {
            var languageWriter = writer.CreateWriter();

            languageWriter.WriteString(
                "Language",
                language);

            writeLanguageItem(
                languageWriter,
                "Value",
                value.Get(language));

            writers.Add(languageWriter);
        }

        writer.WriteArray(
            property,
            writers.ToArray());
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
        Reference<T>? value)
        where T : class, IEngineReferenceable
    {
        if (value is null)
            return;

        writer.WriteString(
            property,
            value.Id);
    }

    /// <summary>
    /// Writes an AssetFile value to the specified markup property.
    /// </summary>
    /// <typeparam name="T">The generic T type.</typeparam>
    /// <param name="writer">The markup writer.</param>
    /// <param name="property">The property name.</param>
    /// <param name="value">The value to write.</param>
    public static void WriteAssetFile<T>(
        this FormatWriterBase writer,
        string property,
        AssetFile<T>? value)
        where T : class, IAssetDefinition
    {
        if (value is null)
            return;

        writer.WriteString(
            property,
            value.FullRelativePath);
    }
}