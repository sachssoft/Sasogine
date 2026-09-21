using Sachssoft.Sasodoc;
using Sachssoft.Sasogine.Documents.Serialization;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Provides serialization support for vector segment definitions
/// containing a variable number of control nodes.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of vector variable segment definition to serialize.
/// </typeparam>
public class VectorVariableSegmentSerializationBase<TDefinition> :
    SerializationBase<TDefinition>
    where TDefinition : VectorVariableSegmentDefinition
{
    private readonly VectorNodeSerialization _nodeSerialization;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VectorVariableSegmentSerializationBase{TDefinition}"/> class.
    /// </summary>
    public VectorVariableSegmentSerializationBase()
    {
        _nodeSerialization = new VectorNodeSerialization();
    }

    /// <inheritdoc/>
    public override void Deserialize(
        TDefinition target,
        FormatReaderBase reader)
    {
        if (reader.Contains(
            nameof(VectorVariableSegmentDefinition.ControlNodes)))
        {
            var nodeReaders = reader.ReadArray(
                nameof(VectorVariableSegmentDefinition.ControlNodes));

            target.ControlNodes.Clear();

            foreach (var nodeReader in nodeReaders)
            {
                var node = new VectorNodeDefinition();

                _nodeSerialization.Deserialize(
                    node,
                    nodeReader);

                target.ControlNodes.Add(node);
            }
        }
    }

    /// <inheritdoc/>
    public override void Serialize(
        TDefinition source,
        FormatWriterBase writer)
    {
        var nodeWriters =
            new List<FormatWriterBase>();

        foreach (var node in source.ControlNodes)
        {
            var nodeWriter = writer.CreateWriter();

            _nodeSerialization.Serialize(
                node,
                nodeWriter);

            nodeWriters.Add(nodeWriter);
        }

        writer.WriteArray(
            nameof(VectorVariableSegmentDefinition.ControlNodes),
            nodeWriters.ToArray());
    }
}