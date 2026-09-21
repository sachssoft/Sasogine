using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Common.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Represents a vector path consisting of a start node and a sequence
/// of vector segments.
/// </summary>
/// <remarks>
/// <para>
/// A vector path starts at <see cref="Start"/> and continues through
/// the ordered collection of <see cref="Segments"/>.
/// </para>
/// <para>
/// Runtime vector segments are created from the segment definitions
/// contained in the associated <see cref="VectorPathDefinition"/>.
/// Segment creation is delegated to the
/// <see cref="VectorSegmentRegistry"/> provided by the parent
/// <see cref="VectorShape"/>.
/// </para>
/// </remarks>
public class VectorPath : EngineObject<VectorPathDefinition>
{
    private readonly DefinitionBindingCollection<VectorSegmentDefinition, IVectorSegment> _segments;
    private readonly IList<IVectorSegment> _mutableSegments;
    private readonly VectorNode _start;

    private VectorShape? _owner;

    //private VectorNode _start;
    //private bool _isClosed;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorPath"/> class
    /// using a new default definition.
    /// </summary>
    public VectorPath()
        : this(new VectorPathDefinition())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorPath"/> class
    /// using the specified start position, selection state, and closed state.
    /// </summary>
    /// <param name="startPosition">
    /// The initial position of the start node.
    /// </param>
    /// <param name="isStartSelected">
    /// A value indicating whether the start node is initially selected.
    /// </param>
    /// <param name="isClosed">
    /// A value indicating whether the path is closed.
    /// </param>
    public VectorPath(
        Point2 startPosition,
        bool isStartSelected,
        bool isClosed)
        : this(
              CreateDefinition(
                  startPosition,
                  isStartSelected,
                  isClosed,
                  out var definition))
    {

        //_start = start;
        //_isClosed = isClosed;
        //Segments = new VectorSegmentCollection(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorPath"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition describing the vector path.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="definition"/> is
    /// <see langword="null"/>.
    /// </exception>
    public VectorPath(VectorPathDefinition definition)
        : this(definition, null)
    {
    }

    internal VectorPath(
        VectorPathDefinition definition,
        VectorShape? owner)
        : base(definition)
    {
        _owner = owner;
        _start = new VectorNode(definition.Start);

        VectorSegmentRegistry segmentRegistry =
            owner?.SegmentRegistry ?? VectorSegmentRegistry.Default;

        _segments = DefinitionBindingConnection.Create(
            this,
            definition.Segments,
            DefinitionBindingFactoryBuilder<VectorSegmentDefinition, IVectorSegment>
                .Create()
                .WithCreateInstance(d => segmentRegistry.CreateInstance(d))
                .WithAttachInstance((d, o) =>
                {
                    if (o is IVectorSegmentInternal segment)
                        segment.Owner = this;
                })
                .WithReleaseInstance((d, o) =>
                {
                    if (o is IVectorSegmentInternal segment)
                        segment.Owner = null;
                })
                .Build());

        _mutableSegments = DefinitionBindingConnection.Connect(
            this,
            _segments);
    }

    /// <summary>
    /// Gets the vector shape that owns this path.
    /// </summary>
    /// <remarks>
    /// The owning shape provides shared context for the path, including
    /// access to the <see cref="VectorSegmentRegistry"/> used to create
    /// runtime segment instances.
    /// </remarks>
    public VectorShape? Owner
    {
        get => _owner;
        internal set
        {
            if (ReferenceEquals(_owner, value))
                return;

            if (_owner is not null && value is not null)
            {
                throw new InvalidOperationException(
                    "The vector path already belongs to another vector shape.");
            }

            VectorShape? oldOwner = _owner;
            _owner = value;

            OnOwnerChanged(oldOwner, value);
        }
    }

    /// <summary>
    /// Gets the start node of the vector path.
    /// </summary>
    public VectorNode Start => _start;

    /// <summary>
    /// Gets whether the path is closed by connecting its endpoint
    /// to the start node.
    /// </summary>
    public bool IsClosed => Definition.IsClosed;

    /// <summary>
    /// Gets the ordered collection of vector segments that make up
    /// the vector path.
    /// </summary>
    /// <remarks>
    /// The runtime collection is derived from the segment definitions
    /// contained in <see cref="VectorPathDefinition.Segments"/>.
    /// </remarks>
    //public VectorSegmentCollection Segments { get; }
    public IReadOnlyList<IVectorSegment> Segments => _segments;

    internal IList<IVectorSegment> MutableSegments => _mutableSegments;


    ///// <inheritdoc/>
    //protected override void ConfigureFromDefinition()
    //{
    //    base.ConfigureFromDefinition();

    //    if (!ReferenceEquals(_start.Definition, Definition.Start))
    //        _start = new VectorNode(Definition.Start);
    //    else
    //        _start.Reload();

    //    _isClosed = Definition.IsClosed;
    //}

    /// <summary>
    /// Generates a sampled representation of the complete vector path.
    /// </summary>
    /// <param name="sampleLength">
    /// The desired approximate distance between consecutive sampled vertices.
    /// </param>
    /// <returns>
    /// An array containing the sampled vertices of the vector path in
    /// path order, beginning with the start node.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="sampleLength"/> is less than or equal
    /// to zero.
    /// </exception>
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

    /// <summary>
    /// Called when the owning vector shape changes.
    /// </summary>
    /// <param name="oldOwner">
    /// The previous owner, or <see langword="null"/> if the path had no owner.
    /// </param>
    /// <param name="newOwner">
    /// The new owner, or <see langword="null"/> if the path no longer has an owner.
    /// </param>
    protected virtual void OnOwnerChanged(
        VectorShape? oldOwner,
        VectorShape? newOwner)
    {
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

        Definition.Segments.Reverse();
        //Segments.Reverse();

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

    private static void ReverseSegment(IVectorSegment segment)
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

                //case VectorArcSegment arc:
                //    arc.Definition.Sweep = !arc.Sweep;
                //    arc.Reload();
                //    break;
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

    private static VectorPathDefinition CreateDefinition(
        Point2 startPosition,
        bool isStartSelected,
        bool isClosed,
        out VectorPathDefinition definition)
    {
        definition = new VectorPathDefinition
        {
            IsClosed = isClosed
        };

        definition.Start.Position = startPosition;
        definition.Start.IsSelected = isStartSelected;

        return definition;
    }

}