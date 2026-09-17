using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Components.Tools.Vector;

public sealed class VectorBSplineSegmentSerialization : VectorVariableSegmentSerializationBase<VectorBSplineSegmentDefinition>
{
    public override void Deserialize(VectorBSplineSegmentDefinition target, FormatReaderBase reader)
    {
        base.Deserialize(target, reader);

        target.Degree = reader.ReadInt32(nameof(VectorBSplineSegmentDefinition.Degree), target.Degree);
    }

    public override void Serialize(VectorBSplineSegmentDefinition source, FormatWriterBase writer)
    {
        base.Serialize(source, writer);

        writer.WriteInt32(nameof(VectorBSplineSegmentDefinition.Degree), source.Degree);
    }
}
