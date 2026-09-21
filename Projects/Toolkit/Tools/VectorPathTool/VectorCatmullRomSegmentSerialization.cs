using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Components.Tools;

public sealed class VectorCatmullRomSegmentSerialization : VectorVariableSegmentSerializationBase<VectorCatmullRomSegmentDefinition>
{
    public override void Deserialize(VectorCatmullRomSegmentDefinition target, FormatReaderBase reader)
    {
        base.Deserialize(target, reader);

        target.IsClosed = reader.ReadBoolean(nameof(VectorCatmullRomSegmentDefinition.IsClosed), target.IsClosed);
    }

    public override void Serialize(VectorCatmullRomSegmentDefinition source, FormatWriterBase writer)
    {
        base.Serialize(source, writer);

        writer.WriteBoolean(nameof(VectorCatmullRomSegmentDefinition.IsClosed), source.IsClosed);
    }
}