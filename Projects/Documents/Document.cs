using Sachssoft.Documents;
using Sachssoft.Documents.Formats.Json;
using Sachssoft.Engine.Resources;
using System;

namespace Sachssoft.Engine.Documents;

/// <summary>
/// Provides functionality for working with supported document formats.
/// </summary>
public static class Document
{
    /// <summary>
    /// Creates a document formatter for the specified format.
    /// </summary>
    /// <param name="formatType">
    /// The document format type.
    /// </param>
    /// <returns>
    /// A document formatter for the specified format.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown when the specified format is known but not yet implemented.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the specified format is unknown or unsupported.
    /// </exception>
    public static IDocumentFormatter CreateFormatter(
        DocumentFormatType formatType)
    {
        return formatType switch
        {
            DocumentFormatType.Xml => throw new NotImplementedException(
                "XML document formatting is not yet supported. " +
                "Support is planned for Sachssoft.Sasodoc 1.1.x."),

            DocumentFormatType.Json => new JsonDocumentFormatter(),

            _ => throw new NotSupportedException(
                $"Document format '{formatType}' is not supported.")
        };
    }
}