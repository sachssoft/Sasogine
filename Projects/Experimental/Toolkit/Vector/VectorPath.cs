using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a vector path consisting of a start node and a sequence
    /// of vector segments.
    /// </summary>
    public class VectorPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorPath"/> class.
        /// </summary>
        public VectorPath()
        {
            Start = new VectorNode();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorPath"/> class
        /// using the specified start node and closed state.
        /// </summary>
        /// <param name="start">
        /// The start node of the vector path.
        /// </param>
        /// <param name="isClosed">
        /// <see langword="true"/> if the path should be closed by connecting
        /// its endpoint to the start node; otherwise, <see langword="false"/>.
        /// </param>
        public VectorPath(
            VectorNode start,
            bool isClosed)
        {
            Start = start;
            IsClosed = isClosed;
        }

        /// <summary>
        /// Gets or sets the start node of the vector path.
        /// </summary>
        public VectorNode Start { get; set; }

        /// <summary>
        /// Gets or sets whether the path is closed by connecting its endpoint
        /// to the start node.
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets the segments that make up the vector path.
        /// </summary>
        public List<IVectorSegment> Segments { get; } = [];

        /// <summary>
        /// Generates a sampled representation of the complete vector path.
        /// </summary>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <returns>
        /// An array containing the sampled vertices of the vector path.
        /// </returns>
        public Point2[] GetVertices(
            float sampleLength)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            if (Segments.Count == 0)
                return [Start.Position];

            var vertices = new List<Point2>(Segments.Count + 1)
            {
                Start.Position
            };

            Point2 currentPosition = Start.Position;

            for (int i = 0; i < Segments.Count; i++)
            {
                var segment = Segments[i];
                Point2[] segmentVertices = segment.GetVertices(currentPosition, sampleLength);

                if (segmentVertices.Length > 0)
                {
                    int startIndex = segmentVertices[0] == currentPosition ? 1 : 0;

                    for (int j = startIndex; j < segmentVertices.Length; j++)
                    {
                        if (vertices.Count == 0 || vertices[^1] != segmentVertices[j])
                            vertices.Add(segmentVertices[j]);
                    }
                }

                if (vertices.Count == 0 || vertices[^1] != segment.Node.Position)
                    vertices.Add(segment.Node.Position);

                currentPosition = segment.Node.Position;
            }

            return vertices.ToArray();
        }

        internal void Reverse()
        {
            if (Segments.Count == 0)
                return;

            var positions = new Point2[Segments.Count + 1];
            var selections = new bool[Segments.Count + 1];

            positions[0] = Start.Position;
            selections[0] = Start.IsSelected;

            for (int i = 0; i < Segments.Count; i++)
            {
                positions[i + 1] = Segments[i].Node.Position;
                selections[i + 1] = Segments[i].Node.IsSelected;
                ReverseSegment(Segments[i]);
            }

            Segments.Reverse();

            Start.Position = positions[^1];
            Start.IsSelected = selections[^1];

            for (int i = 0; i < Segments.Count; i++)
            {
                int sourceIndex = positions.Length - i - 2;
                Segments[i].Node.Position = positions[sourceIndex];
                Segments[i].Node.IsSelected = selections[sourceIndex];
            }
        }

        private static void ReverseSegment(IVectorSegment segment)
        {
            switch (segment)
            {
                case VectorCubicBezierSegment cubic:
                    SwapNodeValues(cubic.ControlNodes[0], cubic.ControlNodes[1]);
                    break;

                case VectorBSplineSegment spline:
                    spline.ControlNodes.Reverse();
                    break;

                case VectorCatmullRomSegment catmullRom:
                    catmullRom.ControlNodes.Reverse();
                    break;

                case VectorArcSegment arc:
                    arc.Sweep = !arc.Sweep;
                    break;
            }
        }

        private static void SwapNodeValues(VectorNode first, VectorNode second)
        {
            Point2 position = first.Position;
            bool isSelected = first.IsSelected;

            first.Position = second.Position;
            first.IsSelected = second.IsSelected;
            second.Position = position;
            second.IsSelected = isSelected;
        }

    }
}