using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the common contract for a vector segment that forms part of a
/// <see cref="VectorPath"/>.
/// </summary>
/// <remarks>
/// A vector segment represents a portion of a vector path between a starting
/// position and its <see cref="Node"/>. Implementations may provide additional
/// control nodes depending on the segment type and can generate sampled vertices
/// used to approximate the segment geometry.
/// </remarks>
public interface IVectorSegment : IEngineObject
{
    /// <summary>
    /// Gets the vector path that owns this segment.
    /// </summary>
    /// <value>
    /// The owning <see cref="VectorPath"/>, or <see langword="null"/> if the
    /// segment is not currently attached to a path.
    /// </value>
    VectorPath? Path { get; }

    /// <summary>
    /// Gets the end node of the vector segment.
    /// </summary>
    /// <value>
    /// The <see cref="VectorNode"/> representing the end of the segment.
    /// </value>
    VectorNode Node { get; }

    /// <summary>
    /// Gets the control nodes used to define the shape of the vector segment.
    /// </summary>
    /// <returns>
    /// A read-only list containing the control nodes used by the segment.
    /// The returned list may be empty when the segment type does not require
    /// additional control nodes.
    /// </returns>
    IReadOnlyList<VectorNode> GetControlNodes();

    /// <summary>
    /// Generates sampled vertices representing the geometry of the vector segment.
    /// </summary>
    /// <param name="startPosition">
    /// The position from which the segment begins.
    /// </param>
    /// <param name="sampleLength">
    /// The target sampling length used to determine the distribution of vertices
    /// along the segment.
    /// </param>
    /// <returns>
    /// An array of <see cref="Point2"/> values representing the sampled geometry
    /// of the segment.
    /// </returns>
    Point2[] GetVertices(Point2 startPosition, float sampleLength);

    /// <summary>
    /// Gets the definition that contains the configurable data for this vector segment.
    /// </summary>
    /// <value>
    /// The <see cref="VectorSegmentDefinition"/> associated with this segment.
    /// </value>
    new VectorSegmentDefinition Definition { get; }
}