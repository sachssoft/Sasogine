using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Documents.Serialization;

namespace Sachssoft.Sasogine.Components.Tools;

public sealed class VectorCubicBezierSegmentSerialization : SerializationBase<VectorCubicBezierSegmentDefinition>
{
    private VectorNodeSerialization _nodeSerialization;

    public VectorCubicBezierSegmentSerialization()
    {
        _nodeSerialization = new VectorNodeSerialization();
    }

    public override void Deserialize(VectorCubicBezierSegmentDefinition target, FormatReaderBase reader)
    {
        if (reader.Contains(nameof(VectorCubicBezierSegmentDefinition.ControlNode0)))
        {
            var nodeReader0 = reader.Read(nameof(VectorCubicBezierSegmentDefinition.ControlNode0));
            _nodeSerialization.Deserialize(target.ControlNode0, nodeReader0);
        }

        if (reader.Contains(nameof(VectorCubicBezierSegmentDefinition.ControlNode1)))
        {
            var nodeReader1 = reader.Read(nameof(VectorCubicBezierSegmentDefinition.ControlNode1));
            _nodeSerialization.Deserialize(target.ControlNode1, nodeReader1);
        }
    }

    public override void Serialize(VectorCubicBezierSegmentDefinition source, FormatWriterBase writer)
    {
        var nodeWriter0 = writer.CreateWriter();
        _nodeSerialization.Serialize(source.ControlNode0, nodeWriter0);
        writer.Write(nameof(VectorCubicBezierSegmentDefinition.ControlNode0), nodeWriter0);

        var nodeWriter1 = writer.CreateWriter();
        _nodeSerialization.Serialize(source.ControlNode1, nodeWriter1);
        writer.Write(nameof(VectorCubicBezierSegmentDefinition.ControlNode1), nodeWriter1);
    }
}
