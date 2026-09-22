using Sachssoft.Documents;
using Sachssoft.Documents.Serialization;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Provides serialization support for
/// <see cref="VectorPathDefinition"/> instances.
/// </summary>
public sealed class VectorPathSerialization :
    SerializationBase<VectorPathDefinition>
{
    private const string TypeKey = "type";

    private readonly VectorNodeSerialization _vectorNodeSerialization = new();

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VectorPathSerialization"/> class using a default
    /// <see cref="VectorSegmentSerializationRegistry"/>.
    /// </summary>
    public VectorPathSerialization()
        : this(new VectorSegmentSerializationRegistry())
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VectorPathSerialization"/> class using the specified
    /// segment serialization registry.
    /// </summary>
    /// <param name="segmentRegistry">
    /// The registry used to resolve vector segment serialization handlers.
    /// If <see langword="null"/>, a new registry is created.
    /// </param>
    public VectorPathSerialization(
        VectorSegmentSerializationRegistry? segmentRegistry)
    {
        SegmentRegistry =
            segmentRegistry ??
            new VectorSegmentSerializationRegistry();
    }

    /// <summary>
    /// Gets the registry used to resolve serialization handlers for
    /// vector segment definitions.
    /// </summary>
    public VectorSegmentSerializationRegistry SegmentRegistry { get; }

    /// <inheritdoc/>
    public override void Deserialize(
        VectorPathDefinition target,
        FormatReaderBase reader)
    {
        target.IsClosed = reader.ReadBoolean(
            nameof(VectorPathDefinition.IsClosed),
            target.IsClosed);

        if (reader.Contains(nameof(VectorPathDefinition.Start)))
        {
            var startReader = reader.Read(
                nameof(VectorPathDefinition.Start));

            _vectorNodeSerialization.Deserialize(
                target.Start,
                startReader!);
        }

        if (reader.Contains(nameof(VectorPathDefinition.Segments)))
        {
            var segmentReaders = reader.ReadArray(
                nameof(VectorPathDefinition.Segments));

            target.Segments.Clear();

            foreach (var segmentReader in segmentReaders)
            {
                if (!segmentReader.Contains(TypeKey))
                    continue;

                var typeName = segmentReader.ReadString(
                    TypeKey,
                    null);

                if (string.IsNullOrEmpty(typeName) ||
                    !SegmentRegistry.Contains(typeName))
                {
                    continue;
                }

                var entry = SegmentRegistry.Get(typeName);
                var definition = entry.CreateDefinition();

                entry.Serialization.Deserialize(
                    definition,
                    segmentReader);

                target.Segments.Add(definition);
            }
        }
    }

    /// <inheritdoc/>
    public override void Serialize(
        VectorPathDefinition source,
        FormatWriterBase writer)
    {
        writer.WriteBoolean(
            nameof(VectorPathDefinition.IsClosed),
            source.IsClosed);

        var startWriter = writer.CreateWriter();

        _vectorNodeSerialization.Serialize(
            source.Start,
            startWriter);

        writer.Write(
            nameof(VectorPathDefinition.Start),
            startWriter);

        var segmentWriters = new List<FormatWriterBase>();

        foreach (var segment in source.Segments)
        {
            var segmentWriter = writer.CreateWriter();

            var serialization =
                SegmentRegistry
                    .Get(segment.GetType())
                    .Serialization;

            serialization.Serialize(
                segment,
                segmentWriter);

            segmentWriters.Add(segmentWriter);
        }

        writer.WriteArray(
            nameof(VectorPathDefinition.Segments),
            segmentWriters.ToArray());
    }
}