using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Graphics.Rendering.Batches;
using Sachssoft.Sasogine.Input;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools;

public class SelectionTool : ToolBase
{
    internal readonly ShapeBatch _lineBatch;
    internal readonly ShapeBatch _pointBatch;
    private readonly ShapeBatch _fillBatch;
    private readonly BasicShader _lineShader;
    private readonly BasicShader _pointShader;
    private readonly BasicShader _fillShader;

    internal Vector2 _cursorPosition;
    internal bool _isInViewport;
    internal SelectionToolInteractions? _interactions;

    private Vector2 _lastCursorPosition;
    private SelectionToolNode? _selectedNode;
    private ISelectionTarget2? _activeTarget;
    private ISelectionTarget2Definition? _activeDefinition;

    private SelectionToolLayer? _layer = null;
    private bool _invalidateLayer = false;


    public SelectionTool(IEnumerable targetsSource, GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(targetsSource);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        TargetsSource = targetsSource;

        _lineBatch = new ShapeBatch(graphicsDevice);
        _pointBatch = new ShapeBatch(graphicsDevice);
        _fillBatch = new ShapeBatch(graphicsDevice);

        _lineShader = new BasicShader { GraphicsDevice = graphicsDevice };
        _pointShader = new BasicShader { GraphicsDevice = graphicsDevice };
        _fillShader = new BasicShader { GraphicsDevice = graphicsDevice };
    }

    public SelectionTool(
        IEnumerable targetsSource,
        GraphicsDevice graphicsDevice,
        bool move = true,
        bool resize = true,
        bool rotation = true)
        : this(targetsSource, graphicsDevice)
    {
        if (rotation)
            Layer = new SelectionToolTransformLayer();
        else if (resize)
            Layer = new SelectionToolMoveResizeLayer();
        else if (move)
            Layer = new SelectionToolMoveLayer();
    }

    public SelectionToolLayer? Layer
    {
        get => _layer;
        set
        {
            if (_layer == value) return;
            _layer = value;
            _invalidateLayer = true;
        }
    }
    public SelectionToolNode? SelectedNode
    {
        get => _selectedNode;
        internal set => _selectedNode = value;
    }

    public IEnumerable TargetsSource { get; }
    public bool EnableSnap { get; set; } = true;
    public Size2 GridSize { get; set; } = new Size2(10f);
    public Color SelectionColor { get; set; } = Color.DodgerBlue;
    public Color HandleColor { get; set; } = Color.White;
    public float LineThickness { get; set; } = 2f;
    public float HandleSize { get; set; } = 8f;

    public virtual void SetInteractions(SelectionToolInteractions interactions)
    {
        ArgumentNullException.ThrowIfNull(interactions);
        _interactions = interactions;
    }

    public virtual void SetCursorPosition(Vector2 position, bool isInViewport = true)
    {
        _cursorPosition = position;
        _isInViewport = isInViewport;
    }

    public virtual void Update(SceneUpdateContext context)
    {
        if (!_isInViewport || _interactions == null)
            return;

        if (_invalidateLayer)
        {
            _invalidateLayer = false;
            UpdateTargetInvalidation();
        }

        var action = _interactions.Action;

        if (_interactions.Cancel.HasFlag(InteractionFlags.WasJustReleased))
        {
            CancelInteraction();
            return;
        }

        if (action.HasFlag(InteractionFlags.WasJustPressed))
        {
            HandleActionPressed();
            _lastCursorPosition = _cursorPosition;
        }

        if (action.HasFlag(InteractionFlags.IsPressed))
        {
            var delta = _cursorPosition - _lastCursorPosition;

            if (_selectedNode != null &&
                (_activeTarget != null || _activeDefinition != null) &&
                Layer != null)
            {
                Layer.OnNodeInteract(
                    GetLayerContext(),
                    _selectedNode,
                    _activeTarget,
                    _activeDefinition,
                    GetOtherSelectedTargets(_activeTarget),
                    GetOtherSelectedTargetDefinitions(_activeDefinition),
                    _cursorPosition,
                    delta);
            }

            _lastCursorPosition = _cursorPosition;
        }

        if (action.HasFlag(InteractionFlags.WasJustReleased))
            HandleActionReleased();

        UpdateTargetInvalidation();
    }

    protected virtual void HandleActionPressed()
    {
        if (TryHitSelectedNode(
        _cursorPosition,
        out var selectedNode,
        out var selectedTarget,
        out var selectedDefinition))
        {
            _activeTarget = selectedTarget;
            _activeDefinition = selectedDefinition;
            _selectedNode = selectedNode;

            Layer!.OnNodeInteract(
                GetLayerContext(),
                selectedNode!,
                selectedTarget,
                selectedDefinition,
                GetOtherSelectedTargets(selectedTarget),
                GetOtherSelectedTargetDefinitions(selectedDefinition),
                _cursorPosition,
                Vector2.Zero);

            return;
        }

        var hit = HitTest(_cursorPosition);

        if (hit.Targets.Count == 0)
        {
            DeselectAll();
            return;
        }

        var hitTarget = hit.Targets[0];

        if (!TryGetTargetPair(
            hitTarget,
            out var target,
            out var definition))
        {
            DeselectAll();
            return;
        }

        bool modify = _interactions!.Modify.HasFlag(
            InteractionFlags.IsPressed);

        if (modify)
        {
            if (IsSelected(hitTarget))
                Deselect(hitTarget);
            else
                AddSelection(hitTarget);

            return;
        }

        if (!IsSelected(hitTarget))
        {
            Select(hitTarget);
            return;
        }

        _activeTarget = target;
        _activeDefinition = definition;

        UpdateTargetInvalidation();

        var node = HitTestNode(_cursorPosition);

        if (node == null)
        {
            _selectedNode = null;
            return;
        }

        if (Layer != null &&
            !Layer.AllowHandle(
                node,
                target,
                definition))
        {
            _selectedNode = null;
            return;
        }

        _selectedNode = node;

        Layer?.OnNodeInteract(
            GetLayerContext(),
            node,
            target,
            definition,
            GetOtherSelectedTargets(target),
            GetOtherSelectedTargetDefinitions(definition),
            _cursorPosition,
            Vector2.Zero);
    }

    protected virtual void HandleActionReleased()
    {
        _selectedNode = null;
        _activeTarget = null;
        _activeDefinition = null;
    }

    protected virtual void CancelInteraction()
    {
        _selectedNode = null;
        _activeTarget = null;
        _activeDefinition = null;
        DeselectAll();
    }

    //protected virtual SelectionToolNode? HitTestNode(
    //    Vector2 position)
    //{
    //    if (Layer == null)
    //        return null;

    //    foreach (var node in Layer.Nodes)
    //    {
    //        //if (!node.IsVisible)
    //        //    continue;

    //        if (IsInNode(
    //            position,
    //            node,
    //            _activeTarget,
    //            _activeDefinition))
    //        {
    //            return node;
    //        }
    //    }

    //    return null;
    //}

    protected virtual SelectionToolNode? HitTestNode(Vector2 position)
    {
        if (Layer == null)
            return null;

        for (int i = Layer.Nodes.Count - 1; i >= 0; i--)
        {
            var node = Layer.Nodes[i];

            if (IsInNode(
                position,
                node,
                _activeTarget,
                _activeDefinition))
            {
                return node;
            }
        }

        return null;
    }

    private bool TryHitSelectedNode(
    Vector2 position,
    out SelectionToolNode? node,
    out ISelectionTarget2? target,
    out ISelectionTarget2Definition? definition)
    {
        node = null;
        target = null;
        definition = null;

        if (Layer == null)
            return false;

        var context = GetLayerContext();

        foreach (var pair in GetTargetPairs())
        {
            bool isSelected =
                pair.Target?.IsSelected == true ||
                pair.Definition?.IsSelected == true;

            if (!isSelected)
                continue;

            Layer.OnTargetInvalidated(
                context,
                pair.Target,
                pair.Definition);

            for (int i = Layer.Nodes.Count - 1; i >= 0; i--)
            {
                var currentNode = Layer.Nodes[i];

                if (!IsInNode(
                    position,
                    currentNode,
                    pair.Target,
                    pair.Definition))
                {
                    continue;
                }

                if (!Layer.AllowHandle(
                    currentNode,
                    pair.Target,
                    pair.Definition))
                {
                    continue;
                }

                node = currentNode;
                target = pair.Target;
                definition = pair.Definition;
                return true;
            }
        }

        return false;
    }

    protected virtual bool IsInNode(
        Vector2 position,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        var nodePosition = GetNodeWorldPosition(
            node,
            target,
            definition);

        return position.X >= nodePosition.X &&
               position.X <= nodePosition.X + node.Size.Width &&
               position.Y >= nodePosition.Y &&
               position.Y <= nodePosition.Y + node.Size.Height;
    }

    protected virtual void UpdateTargetInvalidation()
    {
        if (Layer == null)
            return;

        if (_activeTarget == null && _activeDefinition == null)
            return;

        Layer.OnTargetInvalidated(
            GetLayerContext(),
            _activeTarget,
            _activeDefinition);
    }

    private IEnumerable<ISelectionTarget2> GetOtherSelectedTargets(
        ISelectionTarget2? target)
    {
        foreach (var item in GetTargetPairs())
        {
            if (item.Target == null)
                continue;

            if (target != null &&
                ReferenceEquals(item.Target, target))
                continue;

            if (item.Target.IsSelected)
                yield return item.Target;
        }
    }

    private IEnumerable<ISelectionTarget2Definition> GetOtherSelectedTargetDefinitions(
        ISelectionTarget2Definition? definition)
    {
        foreach (var item in GetTargetPairs())
        {
            if (item.Definition == null)
                continue;

            if (definition != null &&
                ReferenceEquals(item.Definition, definition))
                continue;

            if (item.Definition.IsSelected)
                yield return item.Definition;
        }
    }

    private IEnumerable<TargetPair> GetTargetPairs()
    {
        foreach (var item in TargetsSource)
        {
            if (item is ISelectionTarget2 target)
            {
                ISelectionTarget2Definition? definition = null;

                if (item is IEngineObject engineObject &&
                    engineObject.Definition is ISelectionTarget2Definition targetDefinition)
                {
                    definition = targetDefinition;
                }

                yield return new TargetPair(target, definition);
                continue;
            }

            if (item is IEngineObject engineObject2 &&
                engineObject2.Definition is ISelectionTarget2Definition definition2)
            {
                yield return new TargetPair(null, definition2);
            }
        }
    }

    private bool TryGetTargetPair(
        object targetObject,
        out ISelectionTarget2? target,
        out ISelectionTarget2Definition? definition)
    {
        target = null;
        definition = null;

        foreach (var pair in GetTargetPairs())
        {
            if (pair.Target != null &&
                ReferenceEquals(pair.Target, targetObject))
            {
                target = pair.Target;
                definition = pair.Definition;
                return true;
            }

            if (pair.Definition != null &&
                ReferenceEquals(pair.Definition, targetObject))
            {
                target = pair.Target;
                definition = pair.Definition;
                return true;
            }
        }

        return false;
    }

    protected virtual Vector2 GetNodeWorldPosition(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Vector2 position = Vector2.Zero;

        if (target is ISelectionMovable2 movable)
            position = movable.Position;
        else if (definition is ISelectionMovable2Definition movableDefinition)
            position = movableDefinition.Position;

        return position + node.Position;
    }

    public virtual void Draw(SceneDrawContext context)
    {
        using var scope = new RenderScope(
            context.GraphicsDevice,
            new RenderOptions
            {
                CullMode = CullMode.None,
                Depth = DepthMode.Disabled,
                AlphaBlend = true
            });

        _fillShader.Color = SelectionColor;
        _fillShader.Opacity = 1f;
        _fillShader.Camera = context.ViewCamera;
        _fillShader.Apply();

        _pointShader.Color = HandleColor;
        _pointShader.Opacity = 1f;
        _pointShader.Camera = context.ViewCamera;
        _pointShader.Apply();

        _lineShader.Color = SelectionColor;
        _lineShader.Opacity = 1f;
        _lineShader.Camera = context.ViewCamera;
        _lineShader.Apply();

        _lineBatch.Begin(
            shader: _lineShader,
            camera: context.ViewCamera);

        _pointBatch.Begin(
            shader: _pointShader,
            camera: context.ViewCamera);

        _fillBatch.Begin(
            shader: _fillShader,
            camera: context.ViewCamera);

        DrawSelections();
        DrawNodes();

        _fillBatch.End();
        _lineBatch.End();
        _pointBatch.End();
    }

    protected virtual void DrawNodes()
    {
        if (Layer == null)
            return;

        var layerContext = GetLayerContext();

        foreach (var pair in GetTargetPairs())
        {
            bool isSelected =
                pair.Target?.IsSelected == true ||
                pair.Definition?.IsSelected == true;

            if (!isSelected)
                continue;

            Layer.OnTargetInvalidated(
                layerContext,
                pair.Target,
                pair.Definition);

            foreach (var node in Layer.Nodes)
            {
                if (!node.IsVisible)
                    continue;

                var position = GetNodeWorldPosition(
                    node,
                    pair.Target,
                    pair.Definition);

                var size = node.Size.ToVector2();

                switch (node.Shape)
                {
                    case SelectionToolNodeShape.Quad:
                        _pointBatch.AddFillRectangle(
                            new Bounds2(position, size));
                        break;

                    case SelectionToolNodeShape.Circle:
                        _pointBatch.AddFillEllipse(
                            new Bounds2(position, size));
                        break;
                }
            }
        }
    }

    private void DrawSelections()
    {
        foreach (var pair in GetTargetPairs())
        {
            if (pair.Target != null && pair.Target.IsSelected)
                DrawSelection(pair.Target);
            else if (pair.Definition != null && pair.Definition.IsSelected)
                DrawSelection(pair.Definition);
        }
    }

    protected virtual void DrawSelection(object obj)
    {
        Vector2 position = Vector2.Zero;
        Size2 size = Size2.Zero;
        float rotation = 0f;

        if (obj is ISelectionTarget2 target)
        {
            size = target.Size;

            if (target is ISelectionMovable2 movable)
                position = movable.Position;

            if (target is ISelectionRotatable2 rotatable)
                rotation = rotatable.Rotation;
        }
        else if (obj is ISelectionTarget2Definition definition)
        {
            size = definition.Size;

            if (definition is ISelectionMovable2Definition movableDefinition)
                position = movableDefinition.Position;

            if (definition is ISelectionRotatable2Definition rotatableDefinition)
                rotation = rotatableDefinition.Rotation;
        }
        else
        {
            return;
        }

        float offset = LineThickness / 2f;
        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        Vector2 TransformPoint(float x, float y)
        {
            return position + new Vector2(
                x * cos - y * sin,
                x * sin + y * cos);
        }

        var topLeft = TransformPoint(
            -offset,
            -offset);

        var topRight = TransformPoint(
            size.Width + offset,
            -offset);

        var bottomRight = TransformPoint(
            size.Width + offset,
            size.Height + offset);

        var bottomLeft = TransformPoint(
            -offset,
            size.Height + offset);

        _lineBatch.AddLine(
            new[]
            {
                topLeft,
                topRight,
                bottomRight,
                bottomLeft,
                topLeft
            },
            LineThickness);
    }

    public virtual void Select(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (!TryGetTargetPair(
            target,
            out var activeTarget,
            out var activeDefinition))
        {
            throw new ArgumentException(
                "The specified target is not contained in the target source.",
                nameof(target));
        }

        foreach (var pair in GetTargetPairs())
        {
            if (pair.Target != null)
                pair.Target.IsSelected =
                    ReferenceEquals(pair.Target, activeTarget);

            if (pair.Definition != null)
                pair.Definition.IsSelected =
                    ReferenceEquals(pair.Definition, activeDefinition);
        }

        _activeTarget = activeTarget;
        _activeDefinition = activeDefinition;
        _selectedNode = null;
    }

    public void AddSelection(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        foreach (var pair in GetTargetPairs())
        {
            if (ReferenceEquals(pair.Target, target))
            {
                pair.Target.IsSelected = true;
                _activeTarget = pair.Target;
                _activeDefinition = pair.Definition;
                _selectedNode = null;

                UpdateTargetInvalidation();
                return;
            }

            if (ReferenceEquals(pair.Definition, target))
            {
                pair.Definition.IsSelected = true;
                _activeTarget = pair.Target;
                _activeDefinition = pair.Definition;
                _selectedNode = null;

                UpdateTargetInvalidation();
                return;
            }
        }

        throw new ArgumentException(
            "The specified target is not contained in the target source.",
            nameof(target));
    }

    public virtual void Deselect(object target)
    {
        ArgumentNullException.ThrowIfNull(target);
        SetSelected(target, false);

        if (ReferenceEquals(_activeTarget, target) ||
            ReferenceEquals(_activeDefinition, target))
        {
            _activeTarget = null;
            _activeDefinition = null;
            _selectedNode = null;
        }
    }

    public void DeselectAll()
    {
        foreach (var pair in GetTargetPairs())
        {
            if (pair.Target != null)
                pair.Target.IsSelected = false;

            if (pair.Definition != null)
                pair.Definition.IsSelected = false;
        }

        _activeTarget = null;
        _activeDefinition = null;
        _selectedNode = null;
    }

    internal bool IsSelected(object target)
    {
        if (target is ISelectionTarget2 selectionTarget)
            return selectionTarget.IsSelected;

        if (target is ISelectionTarget2Definition definition)
            return definition.IsSelected;

        return false;
    }

    internal void SetSelected(object target, bool selected)
    {
        if (target is ISelectionTarget2 selectionTarget)
        {
            selectionTarget.IsSelected = selected;
            return;
        }

        if (target is ISelectionTarget2Definition definition)
            definition.IsSelected = selected;
    }

    private bool ContainsTarget(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        foreach (var pair in GetTargetPairs())
        {
            if (ReferenceEquals(pair.Target, target) &&
                ReferenceEquals(pair.Definition, definition))
            {
                return true;
            }
        }

        return false;
    }

    public virtual SelectionTargetHitTestResult HitTest(
        Vector2 touchedPosition)
    {
        var targets = new List<object>();

        foreach (var pair in GetTargetPairs())
        {
            if (pair.Target != null &&
                IsInTarget(touchedPosition, pair.Target))
            {
                targets.Add(pair.Target);
            }
            else if (pair.Definition != null &&
                     IsInTarget(touchedPosition, pair.Definition))
            {
                targets.Add(pair.Definition);
            }
        }

        return new SelectionTargetHitTestResult(targets);
    }

    protected virtual bool IsInTarget(
        Vector2 position,
        object target)
    {
        Vector2 targetPosition = Vector2.Zero;
        Size2 targetSize = Size2.Zero;
        float rotation = 0f;

        if (target is ISelectionTarget2 selectionTarget)
        {
            targetSize = selectionTarget.Size;

            if (selectionTarget is ISelectionMovable2 movable)
                targetPosition = movable.Position;

            if (selectionTarget is ISelectionRotatable2 rotatable)
                rotation = rotatable.Rotation;
        }
        else if (target is ISelectionTarget2Definition definition)
        {
            targetSize = definition.Size;

            if (definition is ISelectionMovable2Definition movableDefinition)
                targetPosition = movableDefinition.Position;

            if (definition is ISelectionRotatable2Definition rotatableDefinition)
                rotation = rotatableDefinition.Rotation;
        }
        else
        {
            return false;
        }

        if (targetSize.Width <= 0f ||
            targetSize.Height <= 0f)
            return false;

        var relative = position - targetPosition;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        var localPosition = new Vector2(
            relative.X * cos + relative.Y * sin,
            -relative.X * sin + relative.Y * cos);

        return localPosition.X >= 0f &&
               localPosition.X <= targetSize.Width &&
               localPosition.Y >= 0f &&
               localPosition.Y <= targetSize.Height;
    }

    internal Vector2 SnapPosition(Vector2 position)
    {
        if (!EnableSnap)
            return position;

        return new Vector2(
            MathF.Round(position.X / GridSize.Width) * GridSize.Width,
            MathF.Round(position.Y / GridSize.Height) * GridSize.Height);
    }

    protected virtual SelectionToolLayerContext GetLayerContext()
    {
        return new SelectionToolLayerContext(
            EnableSnap,
            GridSize,
            HandleSize);
    }

    private readonly struct TargetPair
    {
        public TargetPair(
            ISelectionTarget2? target,
            ISelectionTarget2Definition? definition)
        {
            Target = target;
            Definition = definition;
        }

        public ISelectionTarget2? Target { get; }
        public ISelectionTarget2Definition? Definition { get; }
    }
}