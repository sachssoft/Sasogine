using Sachssoft.Sasogine.Resources.Importers.Internal;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Sasogine.Resources.Importers;

/// <summary>
/// Provides a base class for importing external frame set documents.
/// </summary>
public abstract class FrameSetImporter
{
    private readonly ResourceSourceBase _resourceSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameSetImporter"/> class.
    /// </summary>
    /// <param name="resourceSource">
    /// The resource source containing the frame set document.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="resourceSource"/> is <see langword="null"/>.
    /// </exception>
    protected FrameSetImporter(ResourceSourceBase resourceSource)
    {
        _resourceSource = resourceSource
            ?? throw new ArgumentNullException(nameof(resourceSource));
    }

    /// <summary>
    /// Creates a frame set importer for the specified document format.
    /// </summary>
    /// <param name="formatType">
    /// The document format to import.
    /// </param>
    /// <param name="resourceSource">
    /// The resource source containing the frame set document.
    /// </param>
    /// <returns>
    /// A frame set importer for the specified document format.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="resourceSource"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="formatType"/> is not supported.
    /// </exception>
    public static FrameSetImporter Create(
        DocumentFormatType formatType,
        ResourceSourceBase resourceSource)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);

        return formatType switch
        {
            DocumentFormatType.Xml =>
                new XmlFrameSetImporter(resourceSource),

            DocumentFormatType.Json =>
                new JsonFrameSetImporter(resourceSource),

            _ => throw new NotSupportedException(
                $"The document format '{formatType}' is not supported.")
        };
    }

    /// <summary>
    /// Imports frame set entries from the resource source.
    /// </summary>
    /// <returns>
    /// The imported frame set entries.
    /// </returns>
    /// <exception cref="InvalidDataException">
    /// Thrown when the underlying importer returns <see langword="null"/>.
    /// </exception>
    public IEnumerable<FrameSetEntry> Import()
    {
        using Stream stream = _resourceSource.GetStream();

        foreach (FrameSetEntry entry in OnImporting(stream)
            ?? throw new InvalidDataException(
                "The frame set importer returned null."))
        {
            yield return entry;
        }
    }

    /// <summary>
    /// Imports frame set entries from the specified stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the frame set document.
    /// </param>
    /// <returns>
    /// The frame set entries imported from the stream.
    /// </returns>
    protected abstract IEnumerable<FrameSetEntry> OnImporting(
        Stream stream);
}