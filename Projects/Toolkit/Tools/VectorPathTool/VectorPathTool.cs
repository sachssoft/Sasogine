using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Meshes;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Graphics.Shapes;
using Sachssoft.Sasogine.Input;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Components.Tools
{
    public sealed class VectorPathTool : ToolBase
    {
        private readonly ShapeBatch _lineBatch;
        private readonly ShapeBatch _pointBatch;
        private readonly ShapeBatch _fillBatch;
        private readonly ShapeBatch _vertexBatch;
        private readonly BasicShader _lineShader;
        private readonly BasicShader _pointShader;
        private readonly BasicShader _fillShader;
        private readonly BasicShader _vertexShader;
        private readonly ToolVectorDocument _vectorDocument;

        private Matrix _transform;
        private VectorPathToolInteractions _interactions;
        private Vector2 _cursorPosition;
        private bool _isInViewport;
        private Vector2 _snappedCursorPosition;
        private bool _isPressed;
        private bool _isMoving;
        private ToolVectorNode? _selectedNode;
        private Vector2 _moveStartPosition;
        private readonly List<(ToolVectorNode Node, Vector2 Position)> _movingNodes = [];
        private bool _initialized;
        private IMesh _testQuad;
        private Box? _insertRect;

        public event EventHandler<VectorPathNodesEventArgs>? NodeSelected;
        public event EventHandler<VectorPathNodesEventArgs>? NodeMoved;
        public event EventHandler<VectorPathSegmentsEventArgs>? SegmentAdded;
        public event EventHandler<VectorPathSegmentsEventArgs>? SegmentRemoved;
        public event EventHandler<VectorPathEventArgs>? PathRemoved;
        public event EventHandler<VectorPathEventArgs>? PathConnectionChanged;

        public VectorPathTool(
            ToolVectorDocument vectorDocument,
            GraphicsDevice graphicsDevice)
        {
            _vectorDocument = vectorDocument;
            _lineBatch = new ShapeBatch(graphicsDevice);
            _pointBatch = new ShapeBatch(graphicsDevice);
            _fillBatch = new ShapeBatch(graphicsDevice);
            _vertexBatch = new ShapeBatch(graphicsDevice);
            _lineShader = new BasicShader();
            _lineShader.GraphicsDevice = graphicsDevice;
            _pointShader = new BasicShader();
            _pointShader.GraphicsDevice = graphicsDevice;
            _fillShader = new BasicShader();
            _fillShader.GraphicsDevice = graphicsDevice;
            _vertexShader = new BasicShader();
            _vertexShader.GraphicsDevice = graphicsDevice;

            _transform = Matrix.CreateScale(1f, 1f, 1f);

            _testQuad = MeshGenerator.CreateQuad(graphicsDevice);
        }

        //public VectorPathToolOperation Operation { get; private set; }

        public VectorPathToolMode Mode { get; set; } = VectorPathToolMode.Selection;
        public Func<IToolVectorSegment>? SegmentFactory { get; set; }
        public Func<Bounds, ToolVectorPath>? PathFactory { get; set; }

        public bool SnapGridEnabled { get; set; } = true;
        public bool ShowControlNodes { get; set; } = true;
        public bool ShowVertices { get; set; } = true;
        public bool ShowFill { get; set; } = true;

        public Size GridSize { get; set; } = new Size(10f);
        public Size PointSize { get; set; } = new Size(10f);
        public Size VertexSize { get; set; } = new Size(1f);

        public float LineThickness { get; set; } = 2f;
        public float ControlLineThickness { get; set; } = 1f;
        public float StrokedPointThickness { get; set; } = 2.5f;

        public float SampleLength { get; set; } = 10f;
        public bool SnapInsertedPosition { get; set; } = true;

        public Color PointColor { get; set; } = Color.Red;
        public Color LineColor { get; set; } = Color.SkyBlue;
        public Color FillColor { get; set; } = Color.Gray;
        public Color VertexColor { get; set; } = Color.Blue;

        public void Update(SceneUpdateContext context)
        {
            if (!_isInViewport)
                return;

            switch (Mode)
            {
                case VectorPathToolMode.Selection:

                    if (!_initialized)
                    {
                        // Bestehende Auswahl aus dem Document übernehmen.
                        _selectedNode = FindSelectedNode();
                        _initialized = true;
                    }

                    if (_interactions.Cancel.HasFlag(InteractionFlags.WasJustReleased))
                    {
                        DeselectAllNodes();

                        _selectedNode = null;
                        _isPressed = false;
                        _isMoving = false;

                        _movingNodes.Clear();

                        return;
                    }

                    bool multiSelection =
                        _interactions.Modify.HasFlag(InteractionFlags.IsPressed);

                    if (_interactions.Action.HasFlag(InteractionFlags.IsPressed))
                    {
                        if (!_isPressed)
                        {
                            var hit = HitTest(_cursorPosition);

                            if (_selectedNode == null)
                            {
                                _selectedNode = FindSelectedNode();
                            }

                            // Control Node
                            if (hit.ControlNode != null)
                            {
                                if (multiSelection)
                                {
                                    hit.ControlNode.IsSelected = !hit.ControlNode.IsSelected;

                                    _selectedNode = hit.ControlNode;

                                    NodeSelected?.Invoke(
                                        this,
                                        new VectorPathNodesEventArgs(GetSelectedNodes()));

                                    _isMoving = false;
                                }
                                else if (hit.ControlNode.IsSelected)
                                {
                                    _selectedNode = hit.ControlNode;

                                    _moveStartPosition = _snappedCursorPosition;

                                    StoreSelectedNodes();

                                    _isMoving = true;
                                }
                                else
                                {
                                    DeselectAllNodes();

                                    hit.ControlNode.IsSelected = true;
                                    _selectedNode = hit.ControlNode;

                                    NodeSelected?.Invoke(
                                        this,
                                        new VectorPathNodesEventArgs(GetSelectedNodes()));

                                    _isMoving = false;
                                }
                            }
                            // Normal Node
                            else if (hit.Node != null)
                            {
                                if (multiSelection)
                                {
                                    hit.Node.IsSelected = !hit.Node.IsSelected;

                                    _selectedNode = hit.Node;

                                    NodeSelected?.Invoke(
                                        this,
                                        new VectorPathNodesEventArgs(GetSelectedNodes()));

                                    _isMoving = false;
                                }
                                else if (hit.Node.IsSelected)
                                {
                                    // Bereits ausgewählter Node:
                                    // gesamte Auswahl verschieben.
                                    _selectedNode = hit.Node;

                                    _moveStartPosition = _snappedCursorPosition;

                                    StoreSelectedNodes();

                                    _isMoving = true;
                                }
                                else
                                {
                                    // Andere Node:
                                    // alte Auswahl ersetzen.
                                    DeselectAllNodes();

                                    hit.Node.IsSelected = true;
                                    _selectedNode = hit.Node;

                                    NodeSelected?.Invoke(
                                        this,
                                        new VectorPathNodesEventArgs(GetSelectedNodes()));

                                    _isMoving = false;
                                }
                            }
                            else
                            {
                                // Außerhalb eines Nodes.
                                if (!multiSelection)
                                {
                                    DeselectAllNodes();
                                    _selectedNode = null;
                                }

                                _isMoving = false;
                            }

                            _isPressed = true;
                        }

                        if (_isMoving)
                        {
                            Vector2 delta = _snappedCursorPosition - _moveStartPosition;

                            foreach (var movingNode in _movingNodes)
                            {
                                movingNode.Node.Position =
                                    movingNode.Position + delta;
                            }
                        }
                    }
                    else if (_interactions.Action.HasFlag(InteractionFlags.WasJustReleased))
                    {
                        _isPressed = false;
                        _isMoving = false;

                        if (_movingNodes.Count > 0)
                        {
                            NodeMoved?.Invoke(
                                this,
                                new VectorPathNodesEventArgs(
                                    _movingNodes
                                        .Select(x => x.Node)
                                        .ToList()));
                        }

                        _movingNodes.Clear();
                    }

                    break;

                case VectorPathToolMode.Draw:

                    //if (Operation == VectorPathToolOperation.AddPath)
                    //{
                    //    Console.Clear();

                    //    if (_interactions.Cancel.HasFlag(
                    //        InteractionFlags.WasJustReleased))
                    //    {
                    //        Operation = VectorPathToolOperation.None;
                    //        _requestPathFactory = null;
                    //        return;
                    //    }

                    //    if (_interactions.Add.HasFlag(
                    //        InteractionFlags.WasJustReleased))
                    //    {
                    //        Console.WriteLine("Add WasJustReleased");
                    //        Operation = VectorPathToolOperation.None;
                    //        _requestPathFactory = null;
                    //    }
                    //    else if (_interactions.Add.HasFlag(
                    //        InteractionFlags.IsPressed))
                    //    {
                    //        Console.WriteLine("Add IsPressed");
                    //    }

                    //    return;
                    //}

                    break;

                case VectorPathToolMode.Insert:
                    {
                        if (_interactions.Cancel.HasFlag(InteractionFlags.WasJustReleased))
                        {
                            _insertRect = null;
                            return;
                        }

                        if (_interactions.Action.HasFlag(InteractionFlags.WasJustReleased))
                        {
                            if (_insertRect.HasValue)
                            {
                                var bounds = _insertRect.Value.ToBounds();

                                if (PathFactory != null)
                                {
                                    var path = PathFactory(bounds);

                                    if (path != null)
                                        _vectorDocument.Paths.Add(path);
                                }

                                _insertRect = null;
                            }

                            return;
                        }

                        var position = GetInsertPosition(_cursorPosition);

                        if (_interactions.Action.HasFlag(InteractionFlags.IsPressed))
                        {
                            if (!_insertRect.HasValue)
                            {
                                _insertRect = new Box(
                                    position.X,
                                    position.Y,
                                    position.X,
                                    position.Y);
                            }
                            else
                            {
                                var start = _insertRect.Value.Min;

                                if (_interactions.Modify.HasFlag(InteractionFlags.IsPressed))
                                {
                                    var delta = position - start;
                                    var size = MathF.Max(MathF.Abs(delta.X), MathF.Abs(delta.Y));

                                    position = new Vector2(
                                        start.X + MathF.CopySign(size, delta.X),
                                        start.Y + MathF.CopySign(size, delta.Y));
                                }

                                _insertRect = new Box(
                                    start.X,
                                    start.Y,
                                    position.X,
                                    position.Y);
                            }
                        }

                        break;
                    }
            }
        }

        public void Draw(SceneDrawContext context)
        {
            var graphicsDevice = context.GraphicsDevice;

            using (var scope = new RenderScope(
                graphicsDevice,
                new RenderOptions
                {
                    CullMode = CullMode.None,
                    Depth = DepthMode.Disabled,
                    AlphaBlend = true
                }))
            {
                _fillShader.Color = FillColor;
                _fillShader.Opacity = 1;
                _fillShader.Camera = context.ViewCamera;
                _fillShader.Apply();

                _lineShader.Color = LineColor;
                _lineShader.Opacity = 1;
                _lineShader.Camera = context.ViewCamera;
                _lineShader.Apply();

                _pointShader.Color = PointColor;
                _pointShader.Opacity = 1;
                _pointShader.Camera = context.ViewCamera;
                _pointShader.Apply();

                _vertexShader.Color = VertexColor;
                _vertexShader.Opacity = 1;
                _vertexShader.Camera = context.ViewCamera;
                _vertexShader.Apply();

                _fillBatch.Begin(
                    shader: _fillShader,
                    camera: context.ViewCamera
                );

                _lineBatch.Begin(
                    shader: _lineShader,
                    camera: context.ViewCamera
                );

                _pointBatch.Begin(
                    shader: _pointShader,
                    camera: context.ViewCamera
                );

                _vertexBatch.Begin(
                    shader: _vertexShader,
                    camera: context.ViewCamera
                );

                if (ShowFill)
                {
                    //_fillBatch.AddFillPolygon(
                    //    _vectorDocument.GetVertices(SampleLength),
                    //    _transform);
                }

                var pointSize = PointSize.ToVector2();

                foreach (var path in _vectorDocument.Paths)
                {
                    DrawNode(path.Start.Position, path.Start.IsSelected, true);

                    for (int i = 0; i < path.Segments.Count; i++)
                    {
                        var segment = path.Segments[i];
                        var startPosition = (i == 0) ?
                            path.Start.Position : path.Segments[i - 1].Node.Position;
                        var endPosition = segment.Node.Position;

                        var vertices = segment.GetVertices(startPosition, SampleLength);

                        DrawLine(startPosition, endPosition, vertices);
                        DrawNode(endPosition, segment.Node.IsSelected);

                        if (ShowVertices)
                        {
                            foreach (var vertex in vertices)
                            {
                                DrawVertex(vertex);
                            }
                        }

                        // Control Nodes zeichnen, wenn ShowControlNodes aktiviert ist
                        if (ShowControlNodes)
                        {
                            DrawControlLine(startPosition, endPosition, segment.ControlNodes);
                            for (int j = 0; j < segment.ControlNodes.Count; j++)
                            {
                                var controlNode = segment.ControlNodes[j];
                                DrawControlNode(controlNode.Position, controlNode.IsSelected);
                            }
                        }
                    }

                    if (path.IsClosed && path.Segments.Count > 0)
                    {
                        var lastPosition = path.Segments[path.Segments.Count - 1].Node.Position;

                        DrawLine(lastPosition, path.Start.Position, []);
                    }
                }

                if (Mode == VectorPathToolMode.Insert &&
                    _insertRect.HasValue)
                {
                    // Test
                    var shader = context.DefaultMaterial.Shader;

                    shader.Texture = null;
                    shader.Color = Color.Red;
                    shader.Apply();

                    var b = _insertRect.Value.ToBounds();
                    var t =
                        Matrix.CreateScale(b.Width, b.Height, 1f) *
                        Matrix.CreateTranslation(b.X, b.Y, 0f);
                    MeshRenderer.Draw(context, _testQuad, transform: t);
                    // End Test

                    if (PathFactory != null)
                    {
                        var path = PathFactory(_insertRect.Value.ToBounds());
                        var vertices = path.GetVertices(SampleLength);
                        _lineBatch.AddLine(vertices, LineThickness);
                        Console.WriteLine("PathFactory Segments " + path.Segments.Count);
                        Console.WriteLine("PathFactory Vertices " + vertices.Length);
                    }
                }

                _fillBatch.End();
                _lineBatch.End();
                _pointBatch.End();
                _vertexBatch.End();
            }
        }

        public void SetInteractions(VectorPathToolInteractions interactions)
        {
            _interactions = interactions;
        }

        public void SetCursorPosition(Vector2 position, bool isInViewport = true)
        {
            _cursorPosition = position;
            _isInViewport = isInViewport;

            if (SnapGridEnabled)
            {
                _snappedCursorPosition = new Vector2(
                    float.Round(position.X / GridSize.Width) * GridSize.Width
                        + PointSize.Width / 2f,
                    float.Round(position.Y / GridSize.Height) * GridSize.Height
                        + PointSize.Height / 2f);
            }
            else
            {
                _snappedCursorPosition = position;
            }
        }

        // Touched Position: berührte Position wie Maus oder Touch
        public ToolVectorNodeHitTestResult HitTest(
            Vector2 touchedPosition)
        {
            foreach (var path in _vectorDocument.Paths)
            {
                // Start Node
                if (IsInEllipse(
                    touchedPosition,
                    new Bounds(
                        path.Start.Position.X,
                        path.Start.Position.Y,
                        PointSize.Width,
                        PointSize.Height)))
                {
                    return new ToolVectorNodeHitTestResult(
                        path.Start,
                        null,
                        null);
                }

                foreach (var segment in path.Segments)
                {
                    // Control Nodes
                    foreach (var controlNode in segment.ControlNodes)
                    {
                        if (IsInEllipse(
                            touchedPosition,
                            new Bounds(
                                controlNode.Position.X,
                                controlNode.Position.Y,
                                PointSize.Width,
                                PointSize.Height)))
                        {
                            return new ToolVectorNodeHitTestResult(
                                null,
                                controlNode,
                                segment);
                        }
                    }

                    // End Node
                    if (IsInEllipse(
                        touchedPosition,
                        new Bounds(
                            segment.Node.Position.X,
                            segment.Node.Position.Y,
                            PointSize.Width,
                            PointSize.Height)))
                    {
                        return new ToolVectorNodeHitTestResult(
                            segment.Node,
                            null,
                            segment);
                    }
                }
            }

            return new ToolVectorNodeHitTestResult(
                null,
                null,
                null);
        }

        public void SelectAllNodes()
        {
            SelectAllNodes(true);
        }

        public void DeselectAllNodes()
        {
            SelectAllNodes(false);
        }

        public void SwitchPathConnection(bool isClosed)
        {
            for (int i = 0; i < _vectorDocument.Paths.Count; i++)
            {
                if (_vectorDocument.Paths[i].Start.IsSelected)
                {
                    SwitchPathConnection(i, isClosed);
                    return;
                }

                foreach (var segment in _vectorDocument.Paths[i].Segments)
                {
                    if (segment.Node.IsSelected)
                    {
                        SwitchPathConnection(i, isClosed);
                        return;
                    }
                }
            }
        }

        public void SwitchPathConnection(int pathIndex, bool isClosed)
        {
            var path = _vectorDocument.Paths[pathIndex];

            if (path.IsClosed == isClosed)
                return;

            path.IsClosed = isClosed;

            PathConnectionChanged?.Invoke(
                this,
                new VectorPathEventArgs(path));
        }

        public void AddPath(
            Vector2 position,
            IEnumerable<IToolVectorSegment> segments)
        {
            var path = new ToolVectorPath
            {
                Start = new ToolVectorNode(position)
            };

            path.Segments.AddRange(segments);

            _vectorDocument.Paths.Add(path);
        }

        public void RemovePath()
        {
            for (int pathIndex = 0; pathIndex < _vectorDocument.Paths.Count; pathIndex++)
            {
                var path = _vectorDocument.Paths[pathIndex];

                if (path.Start.IsSelected ||
                    path.Segments.Any(x => x.Node.IsSelected))
                {
                    RemovePath(pathIndex);
                    return;
                }
            }
        }

        public void RemovePath(int pathIndex)
        {
            if (pathIndex < 0 ||
                pathIndex >= _vectorDocument.Paths.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(pathIndex));
            }

            var path = _vectorDocument.Paths[pathIndex];

            _vectorDocument.Paths.RemoveAt(pathIndex);

            _selectedNode = null;

            PathRemoved?.Invoke(
                this,
                new VectorPathEventArgs(path));
        }

        public void AddSegment()
        {
            for (int pathIndex = 0; pathIndex < _vectorDocument.Paths.Count; pathIndex++)
            {
                var path = _vectorDocument.Paths[pathIndex];

                if (path.Start.IsSelected)
                {
                    if (path.Segments.Count > 0)
                        AddSegment(pathIndex, 0);

                    return;
                }

                for (int segmentIndex = 0; segmentIndex < path.Segments.Count; segmentIndex++)
                {
                    if (path.Segments[segmentIndex].Node.IsSelected)
                    {
                        if (segmentIndex + 1 < path.Segments.Count)
                            AddSegment(pathIndex, segmentIndex + 1);
                        else
                            AddSegmentAfterLast(path);

                        return;
                    }
                }
            }
        }

        public void RemoveSegments()
        {
            var removedSegments = new List<IToolVectorSegment>();

            foreach (var path in _vectorDocument.Paths)
            {
                for (int i = path.Segments.Count - 1; i >= 0; i--)
                {
                    if (path.Segments[i].Node.IsSelected)
                    {
                        removedSegments.Add(path.Segments[i]);
                        path.Segments.RemoveAt(i);
                    }
                }
            }

            _selectedNode = null;

            if (removedSegments.Count > 0)
            {
                SegmentRemoved?.Invoke(
                    this,
                    new VectorPathSegmentsEventArgs(removedSegments));
            }
        }

        public void AddSegment(int selectedPathIndex = 0, int selectedSegmentIndex = 0)
        {
            var path = _vectorDocument.Paths[selectedPathIndex];

            if (selectedSegmentIndex < 0 ||
                selectedSegmentIndex >= path.Segments.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(selectedSegmentIndex));
            }

            var segmentFactory = SegmentFactory ??
                (() => new ToolVectorLineSegment());

            var targetSegment = path.Segments[selectedSegmentIndex];

            var startPosition = selectedSegmentIndex == 0
                ? path.Start.Position
                : path.Segments[selectedSegmentIndex - 1].Node.Position;

            var segment = segmentFactory();
            EnsureAddControlPoints(segment);

            segment.Node.Position = GetSegmentInsertPosition(
                startPosition,
                targetSegment.Node.Position);

            SetControlNodes(segment, startPosition);

            path.Segments.Insert(selectedSegmentIndex, segment);

            SegmentAdded?.Invoke(
                this,
                new VectorPathSegmentsEventArgs([segment]));
        }

        public void ReplaceSegment(
            int pathIndex,
            int segmentIndex)
        {
            var path = _vectorDocument.Paths[pathIndex];

            if (segmentIndex < 0 ||
                segmentIndex >= path.Segments.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentIndex));
            }

            var oldSegment = path.Segments[segmentIndex];

            var segmentFactory = SegmentFactory ??
                (() => new ToolVectorLineSegment());

            var newSegment = segmentFactory();

            newSegment.Node.Position = oldSegment.Node.Position;

            if (newSegment is ToolVectorVariableSegment variableSegment)
            {
                variableSegment.ControlNodes.Clear();

                foreach (var oldControlNode in oldSegment.ControlNodes)
                {
                    variableSegment.ControlNodes.Add(
                        new ToolVectorNode(oldControlNode.Position));
                }
            }

            path.Segments[segmentIndex] = newSegment;

            SegmentAdded?.Invoke(
                this,
                new VectorPathSegmentsEventArgs([newSegment]));
        }

        private void AddSegmentAfterLast(ToolVectorPath path)
        {
            var lastPosition = path.Segments[^1].Node.Position;
            var startPosition = path.Start.Position;

            var segmentFactory = SegmentFactory ??
                (() => new ToolVectorLineSegment());

            var segment = segmentFactory();
            EnsureAddControlPoints(segment);

            segment.Node.Position = GetSegmentInsertPosition(
                lastPosition,
                startPosition);

            SetControlNodes(segment, lastPosition);

            path.Segments.Add(segment);

            SegmentAdded?.Invoke(
                this,
                new VectorPathSegmentsEventArgs([segment]));
        }

        private Vector2 GetSegmentInsertPosition(
            Vector2 startPosition,
            Vector2 endPosition)
        {
            return SnapPosition(Vector2.Lerp(
                startPosition,
                endPosition,
                0.5f));
        }

        private Vector2 GetInsertPosition(Vector2 position)
        {
            if (!SnapGridEnabled)
                return position;

            return new Vector2(
                float.Round(position.X / GridSize.Width) * GridSize.Width,
                float.Round(position.Y / GridSize.Height) * GridSize.Height);
        }

        private Vector2 SnapPosition(Vector2 position)
        {
            if (!SnapInsertedPosition)
                return position;

            return new Vector2(
                float.Round(position.X / GridSize.Width) * GridSize.Width,
                float.Round(position.Y / GridSize.Height) * GridSize.Height);
        }

        private void SetControlNodes(
            IToolVectorSegment segment,
            Vector2 startPosition)
        {
            int count = segment.ControlNodes.Count;

            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / (count + 1f);

                var position = Vector2.Lerp(
                    startPosition,
                    segment.Node.Position,
                    t);

                segment.ControlNodes[i].Position = SnapPosition(position);
            }
        }

        private void EnsureAddControlPoints(IToolVectorSegment segment)
        {
            if (segment is ToolVectorVariableSegment variable)
            {
                variable.ControlNodes.Add(new ToolVectorNode());
                variable.ControlNodes.Add(new ToolVectorNode());
            }
        }

        private void DrawLine(
            Vector2 startPosition,
            Vector2 endPosition,
            Vector2[] innerVertices)
        {
            var offset = new Vector2(
                PointSize.Width / 2f,
                PointSize.Height / 2f);

            var vertices = new Vector2[innerVertices.Length + 2];

            vertices[0] = startPosition + offset;

            for (int i = 0; i < innerVertices.Length; i++)
            {
                vertices[i + 1] = innerVertices[i] + offset;
            }

            vertices[^1] = endPosition + offset;

            _lineBatch.AddLine(
                vertices,
                LineThickness,
                ShapeLineJoin.Round,
                ShapeLineCap.Round,
                _transform
            );
        }

        private void DrawControlLine(
            Vector2 startPosition,
            Vector2 endPosition,
            IReadOnlyList<ToolVectorNode> controlNodes)
        {
            if (controlNodes.Count == 0)
                return;

            var offset = new Vector2(
                PointSize.Width / 2f,
                PointSize.Height / 2f);

            var vertices = new Vector2[controlNodes.Count + 2];

            vertices[0] = startPosition + offset;

            for (int i = 0; i < controlNodes.Count; i++)
            {
                vertices[i + 1] = controlNodes[i].Position + offset;
            }

            vertices[^1] = endPosition + offset;

            _lineBatch.AddLine(
                vertices,
                ControlLineThickness,
                ShapeLineJoin.Round,
                ShapeLineCap.Round,
                _transform
            );
        }

        private void StoreSelectedNodes()
        {
            _movingNodes.Clear();

            foreach (var path in _vectorDocument.Paths)
            {
                if (path.Start.IsSelected)
                {
                    _movingNodes.Add(
                        (path.Start, path.Start.Position));
                }

                foreach (var segment in path.Segments)
                {
                    if (segment.Node.IsSelected)
                    {
                        _movingNodes.Add(
                            (segment.Node, segment.Node.Position));
                    }

                    foreach (var controlNode in segment.ControlNodes)
                    {
                        if (controlNode.IsSelected)
                        {
                            _movingNodes.Add(
                                (controlNode, controlNode.Position));
                        }
                    }
                }
            }
        }

        private IReadOnlyList<ToolVectorNode> GetSelectedNodes()
        {
            var nodes = new List<ToolVectorNode>();

            foreach (var path in _vectorDocument.Paths)
            {
                if (path.Start.IsSelected)
                    nodes.Add(path.Start);

                foreach (var segment in path.Segments)
                {
                    if (segment.Node.IsSelected)
                        nodes.Add(segment.Node);

                    foreach (var controlNode in segment.ControlNodes)
                    {
                        if (controlNode.IsSelected)
                            nodes.Add(controlNode);
                    }
                }
            }

            return nodes;
        }

        private void DrawVertex(
            Vector2 position)
        {
            var offset = new Vector2(
                PointSize.Width / 2f,
                PointSize.Height / 2f);

            var vertexSize = VertexSize.ToVector2();
            _vertexBatch.AddFillEllipse(
                position + offset,
                vertexSize / 2f,
                _transform
            );
        }

        private void DrawNode(
            Vector2 position,
            bool isSelected,
            bool isStart = false)
        {
            var pointSize = PointSize.ToVector2();
            if (isStart)
            {
                _pointBatch.AddStrokeEllipse(
                    new Sasogine.Common.Bounds(position.X, position.Y, pointSize.X, pointSize.Y),
                    StrokedPointThickness,
                    ShapeLineJoin.Round,
                    _transform
                );
            }
            else
            {
                _pointBatch.AddFillEllipse(
                    new Sasogine.Common.Bounds(position.X, position.Y, pointSize.X, pointSize.Y),
                    _transform
                );
            }

            if (isSelected)
            {
                var selectionPointSize = PointSize.ToVector2();
                var ringThickness = StrokedPointThickness;
                var ringGap = StrokedPointThickness / 2f;
                var ringSize = selectionPointSize +
                    new Vector2((ringThickness + ringGap) * 2f);
                _pointBatch.AddStrokeEllipse(
                    new Sasogine.Common.Bounds(
                        position.X - ringThickness - ringGap,
                        position.Y - ringThickness - ringGap,
                        ringSize.X,
                        ringSize.Y),
                    ringThickness,
                    ShapeLineJoin.Round,
                    _transform
                );
            }
        }

        private void DrawControlNode(
            Vector2 position,
            bool isSelected)
        {
            var pointSize = PointSize.ToVector2();
            _pointBatch.AddFillRectangle(
                new Sasogine.Common.Bounds(position.X, position.Y, pointSize.X, pointSize.Y),
                _transform
            );

            if (isSelected)
            {
                var selectionPointSize = PointSize.ToVector2();
                var ringThickness = StrokedPointThickness;
                var ringGap = StrokedPointThickness / 2f;
                var ringSize = selectionPointSize +
                    new Vector2((ringThickness + ringGap) * 2f);
                _pointBatch.AddStrokeRectangle(
                    new Sasogine.Common.Bounds(
                        position.X - ringThickness - ringGap,
                        position.Y - ringThickness - ringGap,
                        ringSize.X,
                        ringSize.Y),
                    ringThickness,
                    ShapeLineJoin.Round,
                    _transform
                );
            }
        }

        private void SelectAllNodes(bool isSelected)
        {
            foreach (var path in _vectorDocument.Paths)
            {
                path.Start.IsSelected = isSelected;

                foreach (var segment in path.Segments)
                {
                    segment.Node.IsSelected = isSelected;

                    foreach (var controlNode in segment.ControlNodes)
                    {
                        controlNode.IsSelected = isSelected;
                    }
                }
            }
        }

        private ToolVectorNode? FindSelectedNode()
        {
            foreach (var path in _vectorDocument.Paths)
            {
                if (path.Start.IsSelected)
                    return path.Start;

                foreach (var segment in path.Segments)
                {
                    if (segment.Node.IsSelected)
                        return segment.Node;
                }
            }

            return null;
        }

        private static bool IsInEllipse(
            Vector2 point,
            Bounds bounds)
        {
            float centerX = bounds.X + bounds.Width / 2f;
            float centerY = bounds.Y + bounds.Height / 2f;

            float radiusX = bounds.Width / 2f;
            float radiusY = bounds.Height / 2f;

            float dx = point.X - centerX;
            float dy = point.Y - centerY;

            return
                (dx * dx) / (radiusX * radiusX) +
                (dy * dy) / (radiusY * radiusY) <= 1f;
        }
    }
}

