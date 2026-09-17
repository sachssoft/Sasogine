using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using Sachssoft.Sasogine.Extensions.Sasodoc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Documents.Serialization.Toolkit;

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
