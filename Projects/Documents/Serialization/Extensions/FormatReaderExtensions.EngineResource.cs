using Sachssoft.Engine;
using Sachssoft.Engine.Assets;
using Sachssoft.Engine.Resources.Localization;
using System;
using System.Collections.Generic;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Provides markup serialization extensions for reading EngineResource values.
/// </summary>
public static partial class FormatReaderExtensions
{
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
    /// <param name="fallback">
    /// The value returned when the property cannot be read.
    /// </param>
    /// <param name="invariantLanguage">
    /// The language used when no explicit language is specified.
    /// Defaults to English when not provided.
    /// </param>
    /// <returns>
    /// The deserialized multilingual value, or the supplied fallback when no
    /// usable value is available.
    /// </returns>
    public static MultilingualValue<T>? ReadMultilingualValue<T>(
        this FormatReaderBase reader,
        string property,
        Func<FormatReaderBase, string, T> readLanguageItem,
        MultilingualValue<T>? fallback = null,
        Language? invariantLanguage = null)
    {
        var readers = reader.ReadArray(property);

        if (readers is null)
            return fallback;

        var dict = new Dictionary<Language, T>();

        foreach (var languageReader in readers)
        {
            if (!languageReader.Contains("Language"))
                continue;

            var languageName =
                languageReader.ReadString("Language");

            Language? language =
                string.IsNullOrEmpty(languageName)
                    ? invariantLanguage ?? Languages.English
                    : Language.Find(languageName);

            if (language is null)
                continue;

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
    /// <param name="fallback">
    /// The value returned when the property cannot be read.
    /// </param>
    /// <returns>
    /// The deserialized value, or the supplied fallback when no usable
    /// value is available.
    /// </returns>
    public static Reference<T>? ReadReference<T>(
        this FormatReaderBase reader,
        string property,
        Reference<T>? fallback = null)
        where T : class, IEngineReferenceable
    {
        if (!reader.Contains(property))
            return fallback;

        var id = reader.ReadString(property);

        return new Reference<T>
        {
            Id = id,
        };
    }

    /// <summary>
    /// Reads an AssetFile value from the specified markup property.
    /// </summary>
    /// <typeparam name="T">The generic T type.</typeparam>
    /// <param name="reader">The markup reader.</param>
    /// <param name="property">The property name.</param>
    /// <param name="fallback">
    /// The value returned when the property cannot be read.
    /// </param>
    /// <returns>
    /// The deserialized value, or the supplied fallback when no usable
    /// value is available.
    /// </returns>
    public static AssetFile<T>? ReadAssetFile<T>(
        this FormatReaderBase reader,
        string property,
        AssetFile<T>? fallback = null)
        where T : class, IAssetDefinition
    {
        if (!reader.Contains(property))
            return fallback;

        var relativeFilePath =
            reader.ReadString(property);

        if (string.IsNullOrEmpty(relativeFilePath))
            return null;

        return new AssetFile<T>(relativeFilePath);
    }
}