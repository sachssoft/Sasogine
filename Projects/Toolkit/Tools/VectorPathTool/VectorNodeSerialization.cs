using Sachssoft.Documents;
using Sachssoft.Documents.Serialization;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Provides serialization support for
/// <see cref="VectorNodeDefinition"/> instances.
/// </summary>
public sealed class VectorNodeSerialization :
    SerializationBase<VectorNodeDefinition>
{
    /// <inheritdoc/>
    public override void Deserialize(
        VectorNodeDefinition target,
        FormatReaderBase reader)
    {
        target.IsSelected = reader.ReadBoolean(
            nameof(VectorNodeDefinition.IsSelected),
            target.IsSelected);

        target.Position = reader.ReadPoint2(
            nameof(VectorNodeDefinition.Position),
            target.Position);
    }

    /// <inheritdoc/>
    public override void Serialize(
        VectorNodeDefinition source,
        FormatWriterBase writer)
    {
        writer.WriteBoolean(
            nameof(VectorNodeDefinition.IsSelected),
            source.IsSelected);

        writer.WritePoint2(
            nameof(VectorNodeDefinition.Position),
            source.Position);
    }
}