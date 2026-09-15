using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using Sachssoft.Sasogine.Extensions.Sasodoc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Markup.Serialization.Toolkit;

public sealed class VectorCircularArcSegmentSerialization : SerializationBase<VectorCircularArcSegmentDefinition>
{
    private VectorNodeSerialization _nodeSerialization;

    public VectorCircularArcSegmentSerialization()
    {
        _nodeSerialization = new VectorNodeSerialization();
    }

    public override void Deserialize(VectorCircularArcSegmentDefinition target, FormatReaderBase reader)
    {
        if (reader.Contains(nameof(VectorCircularArcSegmentDefinition.ControlNode)))
        {
            var nodeReader = reader.Read(nameof(VectorCircularArcSegmentDefinition.ControlNode));
            _nodeSerialization.Deserialize(target.ControlNode, nodeReader);
        }
    }

    public override void Serialize(VectorCircularArcSegmentDefinition source, FormatWriterBase writer)
    {
        var nodeWriter = writer.CreateWriter();
        _nodeSerialization.Serialize(source.ControlNode, nodeWriter);
        writer.Write(nameof(VectorCircularArcSegmentDefinition.ControlNode), nodeWriter);
    }
}
