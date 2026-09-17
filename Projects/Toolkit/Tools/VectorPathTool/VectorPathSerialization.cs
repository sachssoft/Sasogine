using Sachssoft.Sasodoc;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Vector;

public sealed class VectorPathSerialization : SerializationBase<VectorPathDefinition>
{
    private const string TypeKey = "type";
    private readonly VectorNodeSerialization _vectorNodeSerialization = new();

    public VectorPathSerialization()
        : this(new VectorSegmentSerializationRegistry())
    {
    }

    public VectorPathSerialization(VectorSegmentSerializationRegistry? segmentRegistry)
    {
        SegmentRegistry = segmentRegistry ?? new VectorSegmentSerializationRegistry();
    }

    public VectorSegmentSerializationRegistry SegmentRegistry { get; }

    public override void Deserialize(VectorPathDefinition target, FormatReaderBase reader)
    {
        target.IsClosed = reader.ReadBoolean(nameof(VectorPathDefinition.IsClosed), target.IsClosed);

        if (reader.Contains(nameof(VectorPathDefinition.Start)))
        {
            var startReader = reader.Read(nameof(VectorPathDefinition.Start));
            _vectorNodeSerialization.Deserialize(target.Start, startReader!);
        }

        if (reader.Contains(nameof(VectorPathDefinition.Segments)))
        {
            var segmentReaders = reader.ReadArray(nameof(VectorPathDefinition.Segments));
            target.Segments.Clear();
            foreach (var segmentReader in segmentReaders)
            {
                if (!segmentReader.Contains(TypeKey))
                    continue;

                var typeName = segmentReader.ReadString(TypeKey, null);

                if (string.IsNullOrEmpty(typeName) || !SegmentRegistry.Contains(typeName))
                    continue;

                var entry = SegmentRegistry.Get(typeName);
                var definition = entry.CreateDefinition();
                entry.Serialization.Deserialize(definition, segmentReader);
                target.Segments.Add(definition);
            }
        }
    }

    public override void Serialize(VectorPathDefinition source, FormatWriterBase writer)
    {
        writer.WriteBoolean(nameof(VectorPathDefinition.IsClosed), source.IsClosed);

        var startWriter = writer.CreateWriter();
        _vectorNodeSerialization.Serialize(source.Start, startWriter);
        writer.Write(nameof(VectorPathDefinition.Start), startWriter);

        var segmentWriters = new List<FormatWriterBase>();
        foreach (var segment in source.Segments)
        {
            var segmentWriter = writer.CreateWriter();
            var serialization = SegmentRegistry.Get(segment.GetType()).Serialization;

            serialization.Serialize(segment, segmentWriter);
            segmentWriters.Add(segmentWriter);
        }
        writer.WriteArray(nameof(VectorPathDefinition.Segments), segmentWriters.ToArray());
    }
}
