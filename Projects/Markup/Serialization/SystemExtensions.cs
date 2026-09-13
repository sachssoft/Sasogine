using Sachssoft.Sasodoc;
using System;

namespace Sachssoft.Sasogine.Markup.Serialization
{
    /// <summary>
    /// Provides markup serialization extensions for common system types.
    /// </summary>
    public static class SystemExtensions
    {
        #region Uri
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
        /// Writes a Uri value to the specified markup property.
        /// </summary>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteUri(this FormatWriterBase writer, string property, Uri? value)
        {
            writer.WriteString(property, value?.ToString());
        }
        #endregion

        #region Version
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

        /// <summary>
        /// Writes a Version value to the specified markup property.
        /// </summary>
        /// <param name="writer">The markup writer.</param>
        /// <param name="property">The property name.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteVersion(this FormatWriterBase writer, string property, Version? value)
        {
            writer.WriteString(property, value?.ToString());
        }
        #endregion
    }
}
