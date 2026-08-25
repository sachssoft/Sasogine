using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools
{
    public class ToolVectorDocument
    {
        public List<ToolVectorPath> Paths { get; } = [];

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