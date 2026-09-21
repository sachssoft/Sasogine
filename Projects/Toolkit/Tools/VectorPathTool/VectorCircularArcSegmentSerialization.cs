using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Documents.Serialization;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Provides serialization support for
/// <see cref="VectorCircularArcSegmentDefinition"/> instances.
/// </summary>
public sealed class VectorCircularArcSegmentSerialization :
    SerializationBase<VectorCircularArcSegmentDefinition>
{
    private readonly VectorNodeSerialization _nodeSerialization;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VectorCircularArcSegmentSerialization"/> class.
    /// </summary>
    public VectorCircularArcSegmentSerialization()
    {
        _nodeSerialization = new VectorNodeSerialization();
    }

    /// <inheritdoc/>
    public override void Deserialize(
        VectorCircularArcSegmentDefinition target,
        FormatReaderBase reader)
    {
        if (reader.Contains(
            nameof(VectorCircularArcSegmentDefinition.ControlNode)))
        {
            var nodeReader = reader.Read(
                nameof(VectorCircularArcSegmentDefinition.ControlNode));

            _nodeSerialization.Deserialize(
                target.ControlNode,
                nodeReader!);
        }
    }

    /// <inheritdoc/>
    public override void Serialize(
        VectorCircularArcSegmentDefinition source,
        FormatWriterBase writer)
    {
        var nodeWriter = writer.CreateWriter();

        _nodeSerialization.Serialize(
            source.ControlNode,
            nodeWriter);

        writer.Write(
            nameof(VectorCircularArcSegmentDefinition.ControlNode),
            nodeWriter);
    }
}