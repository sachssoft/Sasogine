using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using Sachssoft.Sasogine.Extensions.Sasodoc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Markup.Serialization.Toolkit;

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