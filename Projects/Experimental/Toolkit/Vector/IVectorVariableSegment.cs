using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector;

/// <summary>
/// Defines the common contract for a vector segment that supports a variable
/// number of control nodes.
/// </summary>
/// <remarks>
/// Variable vector segments use a <see cref="VectorNodeCollection"/> to manage
/// the control nodes that define or influence the shape of the segment.
/// </remarks>
public interface IVectorVariableSegment
{
    /// <summary>
    /// Gets the collection of control nodes used to define the shape of the vector segment.
    /// </summary>
    /// <value>
    /// The <see cref="VectorNodeCollection"/> containing the control nodes of the segment.
    /// </value>
    VectorNodeCollection ControlNodes { get; }
}