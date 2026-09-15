using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using Sachssoft.Sasogine.Extensions.Sasodoc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Markup.Serialization.Toolkit;

public sealed class VectorBSplineSegmentSerialization : SerializationBase<VectorBSplineSegmentDefinition>
{
    public override void Deserialize(VectorBSplineSegmentDefinition target, FormatReaderBase reader)
    {
        target.Degree = reader.ReadInt32(nameof(VectorBSplineSegmentDefinition.Degree), target.Degree);
    }

    public override void Serialize(VectorBSplineSegmentDefinition source, FormatWriterBase writer)
    {
        writer.WriteInt32(nameof(VectorBSplineSegmentDefinition.Degree), source.Degree);
    }
}
