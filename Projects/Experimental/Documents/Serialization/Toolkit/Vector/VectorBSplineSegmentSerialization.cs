using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using Sachssoft.Sasogine.Extensions.Sasodoc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Documents.Serialization.Toolkit;

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
