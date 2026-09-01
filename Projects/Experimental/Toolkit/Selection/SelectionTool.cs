using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Experimental.Components.Tools.Selection;
using Sachssoft.Sasogine.Experimental.Input;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Graphics.Rendering.Batches;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools;

/// <summary>
/// Provides an interactive tool for selecting and transforming selection targets.
/// </summary>
/// <remarks>
/// The selection tool supports selecting one or multiple targets and delegates
/// transformation behavior such as moving, resizing, and rotating to the configured
/// <see cref="SelectionToolLayer"/>.
/// 
/// Targets may either implement <see cref="ISelectionTarget2"/> directly or expose
/// an <see cref="ISelectionTarget2Definition"/> through an <see cref="IEngineObject"/>.
/// </remarks>
public class SelectionTool : ToolBase
{
    private readonly ShapeBatch _lineBatch;
    private readonly ShapeBatch _pointBatch;
    private readonly ShapeBatch _fillBatch;

    private readonly BasicShader _lineShader;
    private readonly BasicShader _pointShader;
    private readonly BasicShader _fillShader;

    private Vector2 _cursorPosition;
    private bool _isInViewport;
    //private SelectionToolInteractions? _interactions;
    private ToolInteractions? _interactions;

    private Vector2 _lastCursorPosition;
    private SelectionToolNode? _selectedNode;
    private ISelectionTarget2? _activeTarget;
    private ISelectionTarget2Definition? _activeDefinition;

    private SelectionToolLayer? _layer;
    private bool _invalidateLayer;

    private bool _isAreaSelecting;
    private Vector2 _areaSelectionStart;
    private Vector2 _areaSelectionEnd;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionTool"/> class.
    /// </summary>
    /// <param name="targetsSource">The source containing the selectable targets.</param>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the rendering resources required by the tool.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="targetsSource"/> or
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    public SelectionTool(
        IEnumerable targetsSource,
        GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(targetsSource);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        TargetsSource = targetsSource;

        _lineBatch = new ShapeBatch(graphicsDevice);
        _pointBatch = new ShapeBatch(graphicsDevice);
        _fillBatch = new ShapeBatch(graphicsDevice);

        _lineShader = new BasicShader
        {
            GraphicsDevice = graphicsDevice
        };

        _pointShader = new BasicShader
        {
            GraphicsDevice = graphicsDevice
        };

        _fillShader = new BasicShader
        {
            GraphicsDevice = graphicsDevice
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionTool"/> class
    /// with a predefined transformation layer.
    /// </summary>
    /// <param name="targetsSource">The source containing the selectable targets.</param>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the rendering resources required by the tool.
    /// </param>
    /// <param name="move">
    /// Indicates whether moving targets should be supported.
    /// </param>
    /// <param name="resize">
    /// Indicates whether resizing targets should be supported.
    /// </param>
    /// <param name="rotation">
    /// Indicates whether rotating targets should be supported.
    /// </param>
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

    /// <summary>
    /// Gets or sets the layer that defines the available transformation
    /// handles and interaction behavior.
    /// </summary>
    public SelectionToolLayer? Layer
    {
        get => _layer;
        set
        {
            if (_layer == value)
                return;

            _layer = value;
            _invalidateLayer = true;
        }
    }

    /// <summary>
    /// Gets the interaction node that is currently being manipulated.
    /// </summary>
    public SelectionToolNode? SelectedNode
    {
        get => _selectedNode;
        internal set => _selectedNode = value;
    }

    /// <summary>
    /// Gets the source containing the selectable targets.
    /// </summary>
    public IEnumerable TargetsSource { get; }

    /// <summary>
    /// Gets or sets a value indicating whether grid-based snapping is enabled.
    /// </summary>
    public bool EnableGridSnap { get; set; } = true;

    /// <summary>
    /// Gets or sets the horizontal and vertical step used for grid-based snapping.
    /// </summary>
    public Size2 GridSnapStep { get; set; } = new Size2(10f);

    /// <summary>
    /// Gets or sets a value indicating whether angle-based snapping is enabled.
    /// </summary>
    public bool EnableAngleSnap { get; set; } = true;

    /// <summary>
    /// Gets or sets the angular snapping step in radians.
    /// </summary>
    public float AngleSnapStep { get; set; } = MathHelper.ToRadians(15f);

    /// <summary>
    /// Gets or sets a value indicating whether pivot snapping is enabled.
    /// </summary>
    public bool EnablePivotSnap { get; set; } = true;

    /// <summary>
    /// Gets or sets the normalized horizontal and vertical snapping step
    /// used when modifying the transformation pivot.
    /// </summary>
    public Vector2 PivotSnapStep { get; set; } = new Vector2(0.1f);

    /// <summary>
    /// Gets or sets a value indicating whether rectangular area selection is enabled.
    /// </summary>
    public bool EnableAreaSelection { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether targets that only intersect the
    /// rectangular area selection can also be selected.
    /// </summary>
    public bool AllowAreaSelectionIntersection { get; set; }

    /// <summary>
    /// Gets or sets the color used to draw selection outlines.
    /// </summary>
    public Color SelectionColor { get; set; } = Color.DodgerBlue;

    /// <summary>
    /// Gets or sets the color used to draw interaction handles.
    /// </summary>
    public Color HandleColor { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the thickness of selection outlines.
    /// </summary>
    public float LineThickness { get; set; } = 2f;

    /// <summary>
    /// Gets or sets the size of selection interaction handles.
    /// </summary>
    public float HandleSize { get; set; } = 8f;

    /// <inheritdoc/>
    protected internal override void ApplyInteractions(ToolInteractions interactions)
    {
        _interactions = interactions;
    }

    /// <inheritdoc/>
    protected internal override void ApplyCursor(
        ICursorState cursorState,
        ICamera camera)
    {
        _cursorPosition = cursorState.GetWorldPosition(camera);
        _isInViewport = cursorState.IsInViewport;
    }

    ///// <summary>
    ///// Sets the interaction bindings used by the selection tool.
    ///// </summary>
    ///// <param name="interactions">The interaction bindings to use.</param>
    ///// <exception cref="ArgumentNullException">
    ///// Thrown when <paramref name="interactions"/> is <see langword="null"/>.
    ///// </exception>
    //public void SetInteractions(SelectionToolInteractions interactions)
    //{
    //    ArgumentNullException.ThrowIfNull(interactions);
    //    _interactions = interactions;
    //}

    ///// <summary>
    ///// Sets the current cursor position and viewport state.
    ///// </summary>
    ///// <param name="position">The current cursor position.</param>
    ///// <param name="isInViewport">
    ///// Indicates whether the cursor is currently inside the active viewport.
    ///// </param>
    //public void SetCursorPosition(
    //    Vector2 position,
    //    bool isInViewport = true)
    //{
    //    _cursorPosition = position;
    //    _isInViewport = isInViewport;
    //}

    /// <summary>
    /// Updates selection and transformation interactions.
    /// </summary>
    /// <param name="context">The current scene update context.</param>
    public override void Update(SceneUpdateContext context)
    {
        if (!_isInViewport || _interactions == null)
            return;

        if (_invalidateLayer)
        {
            _invalidateLayer = false;
            UpdateTargetInvalidation();
        }

        var action = _interactions.Primary;

        if (_interactions.Secondary.HasFlag(InteractionFlags.WasJustReleased))
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
            if (_isAreaSelecting)
            {
                _areaSelectionEnd = _cursorPosition;
            }
            else
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
            }

            _lastCursorPosition = _cursorPosition;
        }

        if (action.HasFlag(InteractionFlags.WasJustReleased))
            HandleActionReleased();

        UpdateTargetInvalidation();
    }

    private void HandleActionPressed()
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
            if (EnableAreaSelection)
            {
                BeginAreaSelection();

                if (!_interactions!.LeftShoulder.HasFlag(InteractionFlags.IsPressed))
                    DeselectAll();

                return;
            }

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

        bool modify = _interactions!.LeftShoulder.HasFlag(
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

    private void HandleActionReleased()
    {
        if (_isAreaSelecting)
            EndAreaSelection();

        _selectedNode = null;
        _activeTarget = null;
        _activeDefinition = null;
    }

    private void CancelInteraction()
    {
        _isAreaSelecting = false;

        _selectedNode = null;
        _activeTarget = null;
        _activeDefinition = null;

        DeselectAll();
    }

    private SelectionToolNode? HitTestNode(Vector2 position)
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

    private bool IsInNode(
        Vector2 position,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        var nodePosition = GetNodeWorldPosition(
            node,
            target,
            definition);

        if (Layer != null)
        {
            return Layer.HitTestNode(
                position,
                node,
                target,
                definition,
                nodePosition);
        }

        return position.X >= nodePosition.X &&
               position.X <= nodePosition.X + node.Size.Width &&
               position.Y >= nodePosition.Y &&
               position.Y <= nodePosition.Y + node.Size.Height;
    }

    private void UpdateTargetInvalidation()
    {
        if (Layer == null)
            return;

        if (_activeTarget == null &&
            _activeDefinition == null)
        {
            return;
        }

        Layer.OnTargetInvalidated(
            GetLayerContext(),
            _activeTarget,
            _activeDefinition);
    }

    private IEnumerable<ISelectionTarget2> GetOtherSelectedTargets(
        ISelectionTarget2? target)
    {
        foreach (var pair in GetTargetPairs())
        {
            if (pair.Target == null)
                continue;

            if (target != null &&
                ReferenceEquals(pair.Target, target))
            {
                continue;
            }

            if (pair.Target.IsSelected)
                yield return pair.Target;
        }
    }

    private IEnumerable<ISelectionTarget2Definition> GetOtherSelectedTargetDefinitions(
        ISelectionTarget2Definition? definition)
    {
        foreach (var pair in GetTargetPairs())
        {
            if (pair.Definition == null)
                continue;

            if (definition != null &&
                ReferenceEquals(pair.Definition, definition))
            {
                continue;
            }

            if (pair.Definition.IsSelected)
                yield return pair.Definition;
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

                yield return new TargetPair(
                    target,
                    definition);

                continue;
            }

            if (item is IEngineObject engineObject2 &&
                engineObject2.Definition is ISelectionTarget2Definition definition2)
            {
                yield return new TargetPair(
                    null,
                    definition2);
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

    private Vector2 GetNodeWorldPosition(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Vector2 position = Vector2.Zero;

        if (target is ISelectionMovable2 movable)
            position = movable.Position;
        else if (definition is ISelectionMovable2Definition movableDefinition)
            position = movableDefinition.Position;

        var halfSize = node.Size.ToVector2() / 2f;
        var point = node.Position + halfSize;

        if (Layer != null)
        {
            point = Layer.Transform(
                point,
                target,
                definition);
        }

        return position + point - halfSize;
    }

    /// <summary>
    /// Draws the current selections and their interaction handles.
    /// </summary>
    /// <param name="context">The current scene drawing context.</param>
    public override void Draw(SceneDrawContext context)
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
        DrawAreaSelection();

        _fillBatch.End();
        _lineBatch.End();
        _pointBatch.End();
    }

    private void DrawNodes()
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

                DrawNode(
                    node,
                    position);
            }
        }
    }

    /// <summary>
    /// Draws a selection interaction node.
    /// </summary>
    /// <param name="node">The interaction node to draw.</param>
    /// <param name="position">
    /// The world-space top-left position of the interaction node.
    /// </param>
    protected virtual void DrawNode(
        SelectionToolNode node,
        Vector2 position)
    {
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

    private void DrawSelections()
    {
        foreach (var pair in GetTargetPairs())
        {
            if (pair.Target != null &&
                pair.Target.IsSelected)
            {
                DrawSelection(pair.Target);
            }
            else if (pair.Definition != null &&
                     pair.Definition.IsSelected)
            {
                DrawSelection(pair.Definition);
            }
        }
    }

    /// <summary>
    /// Draws the selection outline for the specified target.
    /// </summary>
    /// <param name="obj">
    /// The target or target definition whose selection outline should be drawn.
    /// </param>
    protected virtual void DrawSelection(object obj)
    {
        Vector2 position = Vector2.Zero;
        Size2 size = Size2.Zero;

        ISelectionTarget2? target = null;
        ISelectionTarget2Definition? definition = null;

        if (obj is ISelectionTarget2 selectionTarget)
        {
            target = selectionTarget;
            size = selectionTarget.Size;

            if (selectionTarget is ISelectionMovable2 movable)
                position = movable.Position;
        }
        else if (obj is ISelectionTarget2Definition selectionDefinition)
        {
            definition = selectionDefinition;
            size = selectionDefinition.Size;

            if (selectionDefinition is ISelectionMovable2Definition movableDefinition)
                position = movableDefinition.Position;
        }
        else
        {
            return;
        }

        float offset = LineThickness / 2f;

        Vector2 TransformPoint(float x, float y)
        {
            var point = new Vector2(x, y);

            if (Layer != null)
            {
                point = Layer.Transform(
                    point,
                    target,
                    definition);
            }

            return position + point;
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

    /// <summary>
    /// Draws the current rectangular area selection.
    /// </summary>
    protected virtual void DrawAreaSelection()
    {
        if (!_isAreaSelecting)
            return;

        var bounds = GetAreaSelectionBounds();

        //_fillBatch.AddFillRectangle(bounds);

        _lineBatch.AddLine(
            new[]
            {
            new Vector2(bounds.X, bounds.Y),
            new Vector2(bounds.X + bounds.Width, bounds.Y),
            new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height),
            new Vector2(bounds.X, bounds.Y + bounds.Height),
            new Vector2(bounds.X, bounds.Y)
            },
            LineThickness);
    }

    /// <summary>
    /// Selects the specified target and deselects all other targets.
    /// </summary>
    /// <param name="target">The target or target definition to select.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified target is not contained in
    /// <see cref="TargetsSource"/>.
    /// </exception>
    public void Select(object target)
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
            {
                pair.Target.IsSelected =
                    ReferenceEquals(pair.Target, activeTarget);
            }

            if (pair.Definition != null)
            {
                pair.Definition.IsSelected =
                    ReferenceEquals(pair.Definition, activeDefinition);
            }
        }

        _activeTarget = activeTarget;
        _activeDefinition = activeDefinition;
        _selectedNode = null;
    }

    /// <summary>
    /// Adds the specified target to the current selection.
    /// </summary>
    /// <param name="target">The target or target definition to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified target is not contained in
    /// <see cref="TargetsSource"/>.
    /// </exception>
    public void AddSelection(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        foreach (var pair in GetTargetPairs())
        {
            if (ReferenceEquals(pair.Target, target))
            {
                pair.Target!.IsSelected = true;
                _activeTarget = pair.Target;
                _activeDefinition = pair.Definition;
                _selectedNode = null;

                UpdateTargetInvalidation();
                return;
            }

            if (ReferenceEquals(pair.Definition, target))
            {
                pair.Definition!.IsSelected = true;
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

    /// <summary>
    /// Removes the specified target from the current selection.
    /// </summary>
    /// <param name="target">The target or target definition to deselect.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    public void Deselect(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        SetSelected(
            target,
            false);

        if (ReferenceEquals(_activeTarget, target) ||
            ReferenceEquals(_activeDefinition, target))
        {
            _activeTarget = null;
            _activeDefinition = null;
            _selectedNode = null;
        }
    }

    /// <summary>
    /// Deselects all currently selected targets.
    /// </summary>
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

    private bool IsSelected(object target)
    {
        if (target is ISelectionTarget2 selectionTarget)
            return selectionTarget.IsSelected;

        if (target is ISelectionTarget2Definition definition)
            return definition.IsSelected;

        return false;
    }

    private void SetSelected(
        object target,
        bool selected)
    {
        if (target is ISelectionTarget2 selectionTarget)
        {
            selectionTarget.IsSelected = selected;
            return;
        }

        if (target is ISelectionTarget2Definition definition)
            definition.IsSelected = selected;
    }

    /// <summary>
    /// Performs a hit test against all available selection targets.
    /// </summary>
    /// <param name="touchedPosition">The position to test.</param>
    /// <returns>
    /// A result containing all targets intersecting the specified position.
    /// </returns>
    public SelectionTargetHitTestResult HitTest(
        Vector2 touchedPosition)
    {
        var targets = new List<object>();

        foreach (var pair in GetTargetPairs())
        {
            if (!IsInTarget(
                touchedPosition,
                pair.Target,
                pair.Definition))
            {
                continue;
            }

            if (pair.Target != null)
                targets.Add(pair.Target);
            else if (pair.Definition != null)
                targets.Add(pair.Definition);
        }

        return new SelectionTargetHitTestResult(targets);
    }

    private bool IsInTarget(
        Vector2 position,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (target == null &&
            definition == null)
        {
            return false;
        }

        var targetSize =
            target?.Size ??
            definition!.Size;

        if (targetSize.Width <= 0f ||
            targetSize.Height <= 0f)
        {
            return false;
        }

        Vector2 targetPosition = Vector2.Zero;

        if (target is ISelectionMovable2 movable)
            targetPosition = movable.Position;
        else if (definition is ISelectionMovable2Definition movableDefinition)
            targetPosition = movableDefinition.Position;

        var localPosition =
            position -
            targetPosition;

        if (Layer != null)
        {
            localPosition = Layer.InverseTransform(
                localPosition,
                target,
                definition);
        }

        return localPosition.X >= 0f &&
               localPosition.X <= targetSize.Width &&
               localPosition.Y >= 0f &&
               localPosition.Y <= targetSize.Height;
    }

    private SelectionToolLayerContext GetLayerContext()
    {
        return new SelectionToolLayerContext(
            EnableGridSnap,
            GridSnapStep,
            EnableAngleSnap,
            AngleSnapStep,
            EnablePivotSnap,
            PivotSnapStep,
            HandleSize);
    }

    private void BeginAreaSelection()
    {
        _isAreaSelecting = true;
        _areaSelectionStart = _cursorPosition;
        _areaSelectionEnd = _cursorPosition;
    }

    private void EndAreaSelection()
    {
        _areaSelectionEnd = _cursorPosition;

        var bounds = GetAreaSelectionBounds();

        foreach (var pair in GetTargetPairs())
        {
            if (!IsInAreaSelection(
                bounds,
                pair.Target,
                pair.Definition))
            {
                continue;
            }

            if (pair.Target != null)
                pair.Target.IsSelected = true;

            if (pair.Definition != null)
                pair.Definition.IsSelected = true;
        }

        _isAreaSelecting = false;
    }

    private Bounds2 GetAreaSelectionBounds()
    {
        var min = Vector2.Min(
            _areaSelectionStart,
            _areaSelectionEnd);

        var max = Vector2.Max(
            _areaSelectionStart,
            _areaSelectionEnd);

        return new Bounds2(
            min,
            max - min);
    }

    private bool IsInAreaSelection(
        Bounds2 area,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (target == null &&
            definition == null)
        {
            return false;
        }

        var size =
            target?.Size ??
            definition!.Size;

        Vector2 position = Vector2.Zero;

        if (target is ISelectionMovable2 movable)
            position = movable.Position;
        else if (definition is ISelectionMovable2Definition movableDefinition)
            position = movableDefinition.Position;

        Vector2 TransformPoint(float x, float y)
        {
            var point = new Vector2(x, y);

            if (Layer != null)
            {
                point = Layer.Transform(
                    point,
                    target,
                    definition);
            }

            return position + point;
        }

        var topLeft = TransformPoint(0f, 0f);
        var topRight = TransformPoint(size.Width, 0f);
        var bottomRight = TransformPoint(size.Width, size.Height);
        var bottomLeft = TransformPoint(0f, size.Height);

        var min = Vector2.Min(
            Vector2.Min(topLeft, topRight),
            Vector2.Min(bottomLeft, bottomRight));

        var max = Vector2.Max(
            Vector2.Max(topLeft, topRight),
            Vector2.Max(bottomLeft, bottomRight));

        if (AllowAreaSelectionIntersection)
        {
            return area.X <= max.X &&
                   area.X + area.Width >= min.X &&
                   area.Y <= max.Y &&
                   area.Y + area.Height >= min.Y;
        }

        return min.X >= area.X &&
               max.X <= area.X + area.Width &&
               min.Y >= area.Y &&
               max.Y <= area.Y + area.Height;
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