using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Documents.Serialization;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Provides serialization support for
/// <see cref="VectorQuadraticBezierSegmentDefinition"/> instances.
/// </summary>
public sealed class VectorQuadraticBezierSegmentSerialization :
    SerializationBase<VectorQuadraticBezierSegmentDefinition>
{
    private readonly VectorNodeSerialization _nodeSerialization;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VectorQuadraticBezierSegmentSerialization"/> class.
    /// </summary>
    public VectorQuadraticBezierSegmentSerialization()
    {
        _nodeSerialization = new VectorNodeSerialization();
    }

    /// <inheritdoc/>
    public override void Deserialize(
        VectorQuadraticBezierSegmentDefinition target,
        FormatReaderBase reader)
    {
        if (reader.Contains(
            nameof(VectorQuadraticBezierSegmentDefinition.ControlNode)))
        {
            var nodeReader = reader.Read(
                nameof(VectorQuadraticBezierSegmentDefinition.ControlNode));

            _nodeSerialization.Deserialize(
                target.ControlNode,
                nodeReader!);
        }
    }

    /// <inheritdoc/>
    public override void Serialize(
        VectorQuadraticBezierSegmentDefinition source,
        FormatWriterBase writer)
    {
        var nodeWriter = writer.CreateWriter();

        _nodeSerialization.Serialize(
            source.ControlNode,
            nodeWriter);

        writer.Write(
            nameof(VectorQuadraticBezierSegmentDefinition.ControlNode),
            nodeWriter);
    }
}