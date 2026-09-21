using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Documents.Serialization;

namespace Sachssoft.Sasogine.Components.Tools;

public sealed class VectorNodeSerialization : SerializationBase<VectorNodeDefinition>
{
    public override void Deserialize(VectorNodeDefinition target, FormatReaderBase reader)
    {
        target.IsSelected = reader.ReadBoolean(nameof(VectorNodeDefinition.IsSelected), target.IsSelected);
        target.Position = reader.ReadPoint2(nameof(VectorNodeDefinition.Position), target.Position);
    }

    public override void Serialize(VectorNodeDefinition source, FormatWriterBase writer)
    {
        writer.WriteBoolean(nameof(VectorNodeDefinition.IsSelected), source.IsSelected);
        writer.WritePoint2(nameof(VectorNodeDefinition.Position), source.Position);
    }
}
