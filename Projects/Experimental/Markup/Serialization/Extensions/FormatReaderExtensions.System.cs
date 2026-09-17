using Sachssoft.Sasodoc;
using System;

namespace Sachssoft.Sasogine.Markup.Serialization
{
    /// <summary>
    /// Provides markup serialization extensions for reading System values.
    /// </summary>
    public static partial class FormatReaderExtensions
    {
        /// <summary>
        /// Reads a Uri value from the specified markup property.
        /// </summary>
        /// <param name="reader">The markup reader.</param>
        /// <param name="property">The property name.</param>
        /// <param name="fallback">The value returned when the property cannot be read.</param>
        /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
        public static Uri? ReadUri(this FormatReaderBase reader, string property, Uri? fallback)
        {
            var value = reader.ReadString(property, fallback?.ToString());
            return Uri.TryCreate(value, UriKind.Absolute, out var result) ? result : fallback;
        }


        /// <summary>
        /// Reads a Version value from the specified markup property.
        /// </summary>
        /// <param name="reader">The markup reader.</param>
        /// <param name="property">The property name.</param>
        /// <param name="fallback">The value returned when the property cannot be read.</param>
        /// <returns>The deserialized value, or the supplied fallback when no usable value is available.</returns>
        public static Version? ReadVersion(this FormatReaderBase reader, string property, Version? fallback)
        {
            var value = reader.ReadString(property, fallback?.ToString());
            return Version.TryParse(value, out var result) ? result : fallback;
        }
    }
}
