using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using Sachssoft.Sasogine.Extensions.Sasodoc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Documents.Serialization.Toolkit;

public sealed class VectorQuadraticBezierSegmentSerialization : SerializationBase<VectorQuadraticBezierSegmentDefinition>
{
    private VectorNodeSerialization _nodeSerialization;

    public VectorQuadraticBezierSegmentSerialization()
    {
        _nodeSerialization = new VectorNodeSerialization();
    }

    public override void Deserialize(VectorQuadraticBezierSegmentDefinition target, FormatReaderBase reader)
    {
        if (reader.Contains(nameof(VectorQuadraticBezierSegmentDefinition.ControlNode)))
        {
            var nodeReader = reader.Read(nameof(VectorQuadraticBezierSegmentDefinition.ControlNode));
            _nodeSerialization.Deserialize(target.ControlNode, nodeReader);
        }
    }

    public override void Serialize(VectorQuadraticBezierSegmentDefinition source, FormatWriterBase writer)
    {
        var nodeWriter = writer.CreateWriter();
        _nodeSerialization.Serialize(source.ControlNode, nodeWriter);
        writer.Write(nameof(VectorQuadraticBezierSegmentDefinition.ControlNode), nodeWriter);
    }
}
