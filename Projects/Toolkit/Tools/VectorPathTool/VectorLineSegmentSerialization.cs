using Sachssoft.Documents;
using Sachssoft.Documents.Serialization;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Provides serialization support for
/// <see cref="VectorLineSegmentDefinition"/> instances.
/// </summary>
public sealed class VectorLineSegmentSerialization :
    SerializationBase<VectorLineSegmentDefinition>
{
    /// <inheritdoc/>
    public override void Deserialize(
        VectorLineSegmentDefinition target,
        FormatReaderBase reader)
    {
    }

    /// <inheritdoc/>
    public override void Serialize(
        VectorLineSegmentDefinition source,
        FormatWriterBase writer)
    {
    }
}