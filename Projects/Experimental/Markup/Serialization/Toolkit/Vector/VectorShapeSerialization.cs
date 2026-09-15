using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using Sachssoft.Sasogine.Extensions.Sasodoc;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Markup.Serialization.Toolkit;

public sealed class VectorShapeSerialization : SerializationBase<VectorShapeDefinition>
{
    private readonly VectorPathSerialization _pathSerialization;

    public VectorShapeSerialization()
        : this(new VectorSegmentSerializationRegistry())
    {
    }

    public VectorShapeSerialization(VectorSegmentSerializationRegistry? segmentRegistry)
    {
        _pathSerialization = new VectorPathSerialization(segmentRegistry);
    }

    public override void Deserialize(VectorShapeDefinition target, FormatReaderBase reader)
    {
        target.IsLocked = reader.ReadBoolean(nameof(VectorShapeDefinition.IsLocked), target.IsLocked);

        if (reader.Contains(nameof(VectorShapeDefinition.Paths)))
        {
            var pathReaders = reader.ReadArray(nameof(VectorShapeDefinition.Paths));
            target.Paths.Clear();
            foreach (var pathReader in pathReaders)
            {
                var path = new VectorPathDefinition();
                _pathSerialization.Deserialize(path, pathReader);
                target.Paths.Add(path);
            }
        }
    }

    public override void Serialize(VectorShapeDefinition source, FormatWriterBase writer)
    {
        writer.WriteBoolean(nameof(VectorShapeDefinition.IsLocked), source.IsLocked);

        var pathWriters = new List<FormatWriterBase>();
        foreach (var path in source.Paths)
        {
            var pathWriter = writer.CreateWriter();
            _pathSerialization.Serialize(path, pathWriter);
            pathWriters.Add(pathWriter);
        }
        writer.WriteArray(nameof(VectorShapeDefinition.Paths), pathWriters.ToArray());
    }
}
