using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools
{
    public class ToolVectorPath
    {
        public ToolVectorPath()
        {
            Start = new ToolVectorNode();
        }

        public ToolVectorPath(ToolVectorNode start, bool isClosed)
        {
            Start = start;
            IsClosed = isClosed;
        }

        public ToolVectorNode Start { get; set; }

        public bool IsClosed { get; set; }

        public bool IsLocked { get; set; }

        public bool IsSelected { get; set; }

        public List<IToolVectorSegment> Segments { get; } = [];

        public Vector2[] GetVertices(float sampleLength)
        {
            var vertices = new List<Vector2>();

            Vector2 currentPosition = Start.Position;

            foreach (var segment in Segments)
            {
                var segmentVertices =
                    segment.GetVertices(
                        currentPosition,
                        sampleLength);

                vertices.AddRange(segmentVertices);

                if (segmentVertices.Length > 0)
                {
                    currentPosition =
                        segmentVertices[segmentVertices.Length - 1];
                }
            }

            if (IsClosed)
            {
                vertices.Add(Start.Position);
            }

            return vertices.ToArray();
        }

    }
}