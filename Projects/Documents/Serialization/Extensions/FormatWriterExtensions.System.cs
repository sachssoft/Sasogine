using System;

namespace Sachssoft.Documents.Serialization;

/// <summary>
/// Provides markup serialization extensions for reading System values.
/// </summary>
public static partial class FormatWriterExtensions
{

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
}
