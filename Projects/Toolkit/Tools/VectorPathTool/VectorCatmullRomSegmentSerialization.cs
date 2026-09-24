using Sachssoft.Documents;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Provides serialization support for
/// <see cref="VectorCatmullRomSegmentDefinition"/> instances.
/// </summary>
public sealed class VectorCatmullRomSegmentSerialization :
    VectorVariableSegmentSerializationBase<VectorCatmullRomSegmentDefinition>
{
    /// <inheritdoc/>
    public override void Deserialize(
        VectorCatmullRomSegmentDefinition target,
        FormatReaderBase reader)
    {
        base.Deserialize(target, reader);

        target.IsClosed = reader.ReadBoolean(
            nameof(VectorCatmullRomSegmentDefinition.IsClosed),
            target.IsClosed);
    }

    /// <inheritdoc/>
    public override void Serialize(
        VectorCatmullRomSegmentDefinition source,
        FormatWriterBase writer)
    {
        base.Serialize(source, writer);

        writer.WriteBoolean(
            nameof(VectorCatmullRomSegmentDefinition.IsClosed),
            source.IsClosed);
    }
}