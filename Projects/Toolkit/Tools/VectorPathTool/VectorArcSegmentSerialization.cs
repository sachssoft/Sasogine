using Sachssoft.Documents;
using Sachssoft.Documents.Serialization;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Provides serialization support for <see cref="VectorArcSegmentDefinition"/> instances.
/// </summary>
public sealed class VectorArcSegmentSerialization : SerializationBase<VectorArcSegmentDefinition>
{
    private readonly VectorNodeSerialization _nodeSerialization;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorArcSegmentSerialization"/> class.
    /// </summary>
    public VectorArcSegmentSerialization()
    {
        _nodeSerialization = new VectorNodeSerialization();
    }

    /// <inheritdoc/>
    public override void Deserialize(VectorArcSegmentDefinition target, FormatReaderBase reader)
    {
        if (reader.Contains(nameof(VectorArcSegmentDefinition.ControlNode0)))
        {
            var nodeReader = reader.Read(nameof(VectorArcSegmentDefinition.ControlNode0));
            _nodeSerialization.Deserialize(target.ControlNode0, nodeReader!);
        }

        if (reader.Contains(nameof(VectorArcSegmentDefinition.ControlNode1)))
        {
            var nodeReader = reader.Read(nameof(VectorArcSegmentDefinition.ControlNode1));
            _nodeSerialization.Deserialize(target.ControlNode1, nodeReader!);
        }
    }

    /// <inheritdoc/>
    public override void Serialize(VectorArcSegmentDefinition source, FormatWriterBase writer)
    {
        var nodeWriter0 = writer.CreateWriter();
        _nodeSerialization.Serialize(source.ControlNode0, nodeWriter0);
        writer.Write(nameof(VectorArcSegmentDefinition.ControlNode0), nodeWriter0);

        var nodeWriter1 = writer.CreateWriter();
        _nodeSerialization.Serialize(source.ControlNode1, nodeWriter1);
        writer.Write(nameof(VectorArcSegmentDefinition.ControlNode1), nodeWriter1);
    }
}
