using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Documents.Serialization;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Provides serialization support for
/// <see cref="VectorShapeDefinition"/> instances.
/// </summary>
public sealed class VectorShapeSerialization :
    SerializationBase<VectorShapeDefinition>
{
    private readonly VectorPathSerialization _pathSerialization;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VectorShapeSerialization"/> class using a default
    /// <see cref="VectorSegmentSerializationRegistry"/>.
    /// </summary>
    public VectorShapeSerialization()
        : this(new VectorSegmentSerializationRegistry())
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VectorShapeSerialization"/> class using the specified
    /// segment serialization registry.
    /// </summary>
    /// <param name="segmentRegistry">
    /// The registry used to resolve serialization handlers for vector
    /// segment definitions. If <see langword="null"/>, a new registry
    /// is created by the path serialization.
    /// </param>
    public VectorShapeSerialization(
        VectorSegmentSerializationRegistry? segmentRegistry)
    {
        _pathSerialization =
            new VectorPathSerialization(segmentRegistry);
    }

    /// <inheritdoc/>
    public override void Deserialize(
        VectorShapeDefinition target,
        FormatReaderBase reader)
    {
        target.IsLocked = reader.ReadBoolean(
            nameof(VectorShapeDefinition.IsLocked),
            target.IsLocked);

        if (reader.Contains(
            nameof(VectorShapeDefinition.Paths)))
        {
            var pathReaders = reader.ReadArray(
                nameof(VectorShapeDefinition.Paths));

            target.Paths.Clear();

            foreach (var pathReader in pathReaders)
            {
                var path = new VectorPathDefinition();

                _pathSerialization.Deserialize(
                    path,
                    pathReader);

                target.Paths.Add(path);
            }
        }
    }

    /// <inheritdoc/>
    public override void Serialize(
        VectorShapeDefinition source,
        FormatWriterBase writer)
    {
        writer.WriteBoolean(
            nameof(VectorShapeDefinition.IsLocked),
            source.IsLocked);

        var pathWriters =
            new List<FormatWriterBase>();

        foreach (var path in source.Paths)
        {
            var pathWriter = writer.CreateWriter();

            _pathSerialization.Serialize(
                path,
                pathWriter);

            pathWriters.Add(pathWriter);
        }

        writer.WriteArray(
            nameof(VectorShapeDefinition.Paths),
            pathWriters.ToArray());
    }
}