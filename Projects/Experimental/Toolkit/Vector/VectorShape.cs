using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Represents a vector shape containing a collection of vector paths.</summary>
    public class VectorShape
    {
        /// <summary>Gets the vector paths that define the shape.</summary>
        public List<VectorPath> Paths { get; } = [];

        /// <summary>Gets the sampled vertices of all vector paths that define the shape.</summary>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>A collection containing the sampled vertices of each vector path.</returns>
        public IReadOnlyList<IReadOnlyList<Vector2>> GetVertices(float sampleLength)
        {
            var polygon = new IReadOnlyList<Vector2>[Paths.Count];

            for (int i = 0; i < Paths.Count; i++)
            {
                polygon[i] = Paths[i].GetVertices(sampleLength);
            }

            return polygon;
        }
    }
}