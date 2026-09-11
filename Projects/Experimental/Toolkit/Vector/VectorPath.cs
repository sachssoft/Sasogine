using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a vector path consisting of a start node and a sequence
    /// of vector segments.
    /// </summary>
    public class VectorPath : EngineObject<VectorPathDefinition>
    {
        private VectorNode _start;
        private bool _isClosed;
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorPath"/> class.
        /// </summary>
        public VectorPath()
            : this(new VectorPathDefinition())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorPath"/> class
        /// using the specified start node and closed state.
        /// </summary>
        public VectorPath(VectorNode start, bool isClosed)
            : base(new VectorPathDefinition
            {
                Start = start?.Definition ?? throw new ArgumentNullException(nameof(start)),
                IsClosed = isClosed
            })
        {
            _start = start;
            _isClosed = isClosed;
            Segments = new VectorSegmentCollection(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorPath"/> class
        /// using the specified definition.
        /// </summary>
        public VectorPath(VectorPathDefinition definition)
            : base(definition)
        {
            _start = new VectorNode(definition.Start);
            _isClosed = definition.IsClosed;
            Segments = new VectorSegmentCollection(this);
        }

        public VectorShape? Shape { get; internal set; }

        /// <summary>
        /// Gets or sets the start node of the vector path.
        /// </summary>
        public VectorNode Start => _start;

        /// <summary>
        /// Gets or sets whether the path is closed by connecting its endpoint
        /// to the start node.
        /// </summary>
        public bool IsClosed => _isClosed;

        /// <summary>
        /// Gets the segments that make up the vector path.
        /// </summary>
        public VectorSegmentCollection Segments { get; }

        /// <inheritdoc/>
        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();

            if (!ReferenceEquals(_start.Definition, Definition.Start))
                _start = new VectorNode(Definition.Start);
            else
                _start.Reload();

            _isClosed = Definition.IsClosed;
        }

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

            Start.Definition.Position = positions[^1];

            Start.Reload();
            Start.Definition.IsSelected = selections[^1];
            Start.Reload();

            for (int i = 0; i < Segments.Count; i++)
            {
                int sourceIndex = positions.Length - i - 2;
                Segments[i].Node.Definition.Position = positions[sourceIndex];
                Segments[i].Node.Reload();
                Segments[i].Node.Definition.IsSelected = selections[sourceIndex];
                Segments[i].Node.Reload();
            }
        }

        private static void ReverseSegment(VectorSegment segment)
        {
            switch (segment)
            {
                case VectorCubicBezierSegment cubic:
                    SwapNodeValues(cubic.GetControlNodes()[0], cubic.GetControlNodes()[1]);
                    break;

                case VectorBSplineSegment spline:
                    spline.ControlNodes.Reverse();
                    break;

                case VectorCatmullRomSegment catmullRom:
                    catmullRom.ControlNodes.Reverse();
                    break;

                case VectorArcSegment arc:
                    arc.Definition.Sweep = !arc.Sweep;
                    arc.Reload();
                    break;
            }
        }

        private static void SwapNodeValues(VectorNode first, VectorNode second)
        {
            Point2 position = first.Position;
            bool isSelected = first.IsSelected;

            first.Definition.Position = second.Position;

            first.Reload();
            first.Definition.IsSelected = second.IsSelected;
            first.Reload();
            second.Definition.Position = position;
            second.Reload();
            second.Definition.IsSelected = isSelected;
            second.Reload();
        }

    }
}