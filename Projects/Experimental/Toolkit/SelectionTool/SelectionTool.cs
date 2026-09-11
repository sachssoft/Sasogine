using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Rendering.Cameras;
using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Components.Tools;
using Sachssoft.Sasogine.Experimental.Components.Tools.Selection;
using Sachssoft.Sasogine.Input;
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
public class SelectionTool : ToolBase, INotifyTransformChanged
{
    private readonly ShapeBatch _lineBatch;
    private readonly ShapeBatch _pointBatch;
    private readonly ShapeBatch _fillBatch;

    private readonly BasicShader _lineShader;
    private readonly BasicShader _pointShader;
    private readonly BasicShader _fillShader;

    private Point2 _cursorPosition;
    private bool _isInViewport;
    private ToolInteractions? _interactions;

    private Point2 _lastCursorPosition;
    private SelectionToolNode? _selectedNode;
    private ISelectionTarget2? _activeTarget;
    private ISelectionTarget2Definition? _activeDefinition;

    private SelectionToolLayer? _layer;
    private bool _invalidateLayer;

    private bool _transformCompleted;

    private bool _isAreaSelecting;
    private Point2 _areaSelectionStart;
    private Point2 _areaSelectionEnd;

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
    /// Gets a value indicating whether a selection transform interaction is currently active.
    /// </summary>
    public bool IsTransforming { get; private set; }

    /// <summary>
    /// Occurs when a selection transform interaction starts.
    /// </summary>
    public event EventHandler? TransformStarted;

    /// <summary>
    /// Occurs while a selection transform interaction changes one or more targets.
    /// </summary>
    public event EventHandler? TransformChanged;

    /// <summary>
    /// Occurs when a selection transform interaction completes.
    /// </summary>
    public event EventHandler? TransformCompleted;

    /// <summary>
    /// Returns whether a transform has completed since the previous call and clears the state.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a transform completed since the previous call; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool ConsumeTransformCompleted()
    {
        if (!_transformCompleted)
            return false;

        _transformCompleted = false;
        return true;
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
    protected override void ApplyContext(ToolContext context)
    {
        _interactions = context.Interactions;

        var cursorPosition =
            context.CursorState.GetWorldPosition(
                context.Camera);

        _cursorPosition = new Point2(
            cursorPosition.X,
            cursorPosition.Y);

        _isInViewport =
            context.CursorState.IsInViewport;
    }

    /// <summary>
    /// Updates selection and transformation interactions.
    /// </summary>
    /// <param name="context">The current scene update context.</param>
    public override void Update(SceneUpdateContext context)
    {
        if (_interactions == null)
            return;

        var action = _interactions.Action;

        if (_interactions.Cancel.HasFlag(
            InteractionFlags.WasJustReleased))
        {
            CancelInteraction();
            return;
        }

        if (!_isInViewport)
        {
            if (action.HasFlag(
                InteractionFlags.WasJustReleased))
            {
                HandleActionReleased();
            }

            return;
        }

        if (_invalidateLayer)
        {
            _invalidateLayer = false;
            UpdateTargetInvalidation();
        }

        if (action.HasFlag(
            InteractionFlags.WasJustPressed))
        {
            HandleActionPressed();
            _lastCursorPosition = _cursorPosition;
        }

        if (action.HasFlag(
            InteractionFlags.IsPressed))
        {
            if (_isAreaSelecting)
            {
                _areaSelectionEnd = _cursorPosition;
            }
            else
            {
                Vector2 delta =
                    _cursorPosition -
                    _lastCursorPosition;

                if (_selectedNode != null &&
                    (_activeTarget != null ||
                     _activeDefinition != null) &&
                    Layer != null)
                {
                    Layer.OnNodeInteract(
                        GetLayerContext(),
                        _selectedNode,
                        _activeTarget,
                        _activeDefinition,
                        GetOtherSelectedTargets(
                            _activeTarget),
                        GetOtherSelectedTargetDefinitions(
                            _activeDefinition),
                        _cursorPosition,
                        delta);

                    if (IsTransforming &&
                        delta != Vector2.Zero)
                    {
                        NotifyTransformChanged();
                    }
                }
            }

            _lastCursorPosition = _cursorPosition;
        }

        if (action.HasFlag(
            InteractionFlags.WasJustReleased))
        {
            HandleActionReleased();
        }

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

            BeginTransform();

            Layer!.OnNodeInteract(
                GetLayerContext(),
                selectedNode!,
                selectedTarget,
                selectedDefinition,
                GetOtherSelectedTargets(
                    selectedTarget),
                GetOtherSelectedTargetDefinitions(
                    selectedDefinition),
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

                if (!_interactions!.Modifier.HasFlag(
                    InteractionFlags.IsPressed))
                {
                    DeselectAll();
                }

                return;
            }

            DeselectAll();
            return;
        }

        var hitTarget = hit.Targets[0];

        if (!TryGetTargetEntry(
            hitTarget,
            out var target,
            out var definition))
        {
            DeselectAll();
            return;
        }

        bool modify =
            _interactions!.Modifier.HasFlag(
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
            Select(hitTarget);

        _activeTarget = target;
        _activeDefinition = definition;

        UpdateTargetInvalidation();

        var node = HitTestNode(
            _cursorPosition);

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
        BeginTransform();

        Layer?.OnNodeInteract(
            GetLayerContext(),
            node,
            target,
            definition,
            GetOtherSelectedTargets(
                target),
            GetOtherSelectedTargetDefinitions(
                definition),
            _cursorPosition,
            Vector2.Zero);
    }

    private void HandleActionReleased()
    {
        if (_isAreaSelecting)
            EndAreaSelection();

        if (IsTransforming)
            CompleteTransform();

        _selectedNode = null;
        _activeTarget = null;
        _activeDefinition = null;
    }

    private void CancelInteraction()
    {
        _isAreaSelecting = false;
        IsTransforming = false;

        _selectedNode = null;
        _activeTarget = null;
        _activeDefinition = null;

        DeselectAll();
    }

    private void BeginTransform()
    {
        if (IsTransforming)
            return;

        IsTransforming = true;
        NotifyTransformStarted();
        TransformStarted?.Invoke(this, EventArgs.Empty);
    }

    private void NotifyTransformChanged()
    {
        NotifySelectedTargets(
            static notify => notify.OnTransformChanged());

        TransformChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CompleteTransform()
    {
        IsTransforming = false;
        _transformCompleted = true;

        NotifySelectedTargets(
            static notify => notify.OnTransformCompleted());

        TransformCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void NotifyTransformStarted()
    {
        NotifySelectedTargets(
            static notify => notify.OnTransformStarted());
    }

    private void NotifySelectedTargets(
        Action<ITransformChangeObserver> notifyAction)
    {
        foreach (var entry in GetTargetEntries())
        {
            bool isSelected =
                entry.Target?.IsSelected == true ||
                entry.TargetDefinition?.IsSelected == true;

            if (!isSelected)
                continue;

            if (entry.TransformObserver != null)
                notifyAction(entry.TransformObserver);
        }
    }

    private SelectionToolNode? HitTestNode(
        Point2 position)
    {
        if (Layer == null)
            return null;

        for (int i = Layer.Nodes.Count - 1;
             i >= 0;
             i--)
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
        Point2 position,
        out SelectionToolNode? node,
        out ISelectionTarget2? target,
        out ISelectionTarget2Definition? definition)
    {
        node = null;
        target = null;
        definition = null;

        if (Layer == null)
            return false;

        var layerContext =
            GetLayerContext();

        foreach (var entry in GetTargetEntries())
        {
            bool isSelected =
                entry.Target?.IsSelected == true ||
                entry.TargetDefinition?.IsSelected == true;

            if (!isSelected)
                continue;

            Layer.OnTargetInvalidated(
                layerContext,
                entry.Target,
                entry.TargetDefinition);

            for (int i = Layer.Nodes.Count - 1;
                 i >= 0;
                 i--)
            {
                var currentNode =
                    Layer.Nodes[i];

                if (!IsInNode(
                    position,
                    currentNode,
                    entry.Target,
                    entry.TargetDefinition))
                {
                    continue;
                }

                if (!Layer.AllowHandle(
                    currentNode,
                    entry.Target,
                    entry.TargetDefinition))
                {
                    continue;
                }

                node = currentNode;
                target = entry.Target;
                definition = entry.TargetDefinition;

                return true;
            }
        }

        return false;
    }

    private bool IsInNode(
        Point2 position,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        var nodePosition =
            GetNodeWorldPosition(
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

        return
            position.X >= nodePosition.X &&
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

    private IEnumerable<ISelectionTarget2>
        GetOtherSelectedTargets(
            ISelectionTarget2? target)
    {
        foreach (var entry in GetTargetEntries())
        {
            if (entry.Target == null)
                continue;

            if (target != null &&
                ReferenceEquals(
                    entry.Target,
                    target))
            {
                continue;
            }

            if (entry.Target.IsSelected)
                yield return entry.Target;
        }
    }

    private IEnumerable<ISelectionTarget2Definition>
        GetOtherSelectedTargetDefinitions(
            ISelectionTarget2Definition? definition)
    {
        foreach (var entry in GetTargetEntries())
        {
            if (entry.TargetDefinition == null)
                continue;

            if (definition != null &&
                ReferenceEquals(
                    entry.TargetDefinition,
                    definition))
            {
                continue;
            }

            if (entry.TargetDefinition.IsSelected)
                yield return entry.TargetDefinition;
        }
    }

    private IEnumerable<SelectionTargetEntry> GetTargetEntries()
    {
        foreach (var item in TargetsSource)
        {
            if (item is IEngineObject engineObject)
            {
                var entry = new SelectionTargetEntry(engineObject);

                if (entry.Target != null ||
                    entry.TargetDefinition != null)
                {
                    yield return entry;
                }

                continue;
            }

            if (item is ISelectionTarget2 target)
                yield return new SelectionTargetEntry(target);
        }
    }

    private bool TryGetTargetEntry(
        object targetObject,
        out ISelectionTarget2? target,
        out ISelectionTarget2Definition? definition)
    {
        target = null;
        definition = null;

        foreach (var entry in GetTargetEntries())
        {
            if (entry.Target != null &&
                ReferenceEquals(
                    entry.Target,
                    targetObject))
            {
                target = entry.Target;
                definition = entry.TargetDefinition;
                return true;
            }

            if (entry.TargetDefinition != null &&
                ReferenceEquals(
                    entry.TargetDefinition,
                    targetObject))
            {
                target = entry.Target;
                definition = entry.TargetDefinition;
                return true;
            }
        }

        return false;
    }

    private Point2 GetNodeWorldPosition(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Point2 position = Point2.Zero;

        if (target is ISelectionMovable2 movable)
        {
            position = movable.Position;
        }
        else if (definition
            is ISelectionMovable2Definition
                movableDefinition)
        {
            position =
                movableDefinition.Position;
        }

        Size2 halfSize =
            node.Size / 2f;

        Point2 point = new Point2(
            node.Position.X + halfSize.Width,
            node.Position.Y + halfSize.Height);

        if (Layer != null)
        {
            point = Layer.Transform(
                point,
                target,
                definition);
        }

        return new Point2(
            position.X +
                point.X -
                halfSize.Width,
            position.Y +
                point.Y -
                halfSize.Height);
    }

    /// <summary>
    /// Draws the current selections and their interaction handles.
    /// </summary>
    /// <param name="context">
    /// The current scene drawing context.
    /// </param>
    public override void Draw(
        SceneDrawContext context)
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

        var layerContext =
            GetLayerContext();

        foreach (var entry in GetTargetEntries())
        {
            bool isSelected =
                entry.Target?.IsSelected == true ||
                entry.TargetDefinition?.IsSelected == true;

            if (!isSelected)
                continue;

            Layer.OnTargetInvalidated(
                layerContext,
                entry.Target,
                entry.TargetDefinition);

            foreach (var node in Layer.Nodes)
            {
                if (!node.IsVisible)
                    continue;

                var position =
                    GetNodeWorldPosition(
                        node,
                        entry.Target,
                        entry.TargetDefinition);

                DrawNode(
                    node,
                    position);
            }
        }
    }

    /// <summary>
    /// Draws a selection interaction node.
    /// </summary>
    /// <param name="node">
    /// The interaction node to draw.
    /// </param>
    /// <param name="position">
    /// The world-space top-left position of the interaction node.
    /// </param>
    protected virtual void DrawNode(
        SelectionToolNode node,
        Point2 position)
    {
        var size = node.Size;

        switch (node.Shape)
        {
            case SelectionToolNodeShape.Quad:
                _pointBatch.AddFillRectangle(
                    new Bounds2(
                        position,
                        size));
                break;

            case SelectionToolNodeShape.Circle:
                _pointBatch.AddFillEllipse(
                    new Bounds2(
                        position,
                        size));
                break;
        }
    }

    private void DrawSelections()
    {
        foreach (var entry in GetTargetEntries())
        {
            if (entry.Target != null &&
                entry.Target.IsSelected)
            {
                DrawSelection(
                    entry.Target);
            }
            else if (entry.TargetDefinition != null &&
                     entry.TargetDefinition.IsSelected)
            {
                DrawSelection(
                    entry.TargetDefinition);
            }
        }
    }

    /// <summary>
    /// Draws the selection outline for the specified target.
    /// </summary>
    /// <param name="obj">
    /// The target or target definition whose selection outline should be drawn.
    /// </param>
    protected virtual void DrawSelection(
        object obj)
    {
        Point2 position = Point2.Zero;
        Size2 size = Size2.Zero;

        ISelectionTarget2? target = null;
        ISelectionTarget2Definition?
            definition = null;

        if (obj is ISelectionTarget2
            selectionTarget)
        {
            target = selectionTarget;
            size = selectionTarget.Size;

            if (selectionTarget
                is ISelectionMovable2 movable)
            {
                position = movable.Position;
            }
        }
        else if (obj
            is ISelectionTarget2Definition
                selectionDefinition)
        {
            definition = selectionDefinition;
            size = selectionDefinition.Size;

            if (selectionDefinition
                is ISelectionMovable2Definition
                    movableDefinition)
            {
                position =
                    movableDefinition.Position;
            }
        }
        else
        {
            return;
        }

        float offset =
            LineThickness / 2f;

        Point2 TransformPoint(
            float x,
            float y)
        {
            Point2 point =
                new Point2(x, y);

            if (Layer != null)
            {
                point = Layer.Transform(
                    point,
                    target,
                    definition);
            }

            return new Point2(
                position.X + point.X,
                position.Y + point.Y);
        }

        Point2 topLeft =
            TransformPoint(
                -offset,
                -offset);

        Point2 topRight =
            TransformPoint(
                size.Width + offset,
                -offset);

        Point2 bottomRight =
            TransformPoint(
                size.Width + offset,
                size.Height + offset);

        Point2 bottomLeft =
            TransformPoint(
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

        Bounds2 bounds =
            GetAreaSelectionBounds();

        _lineBatch.AddLine(
            new[]
            {
                new Point2(
                    bounds.X,
                    bounds.Y),

                new Point2(
                    bounds.X + bounds.Width,
                    bounds.Y),

                new Point2(
                    bounds.X + bounds.Width,
                    bounds.Y + bounds.Height),

                new Point2(
                    bounds.X,
                    bounds.Y + bounds.Height),

                new Point2(
                    bounds.X,
                    bounds.Y)
            },
            LineThickness);
    }

    /// <summary>
    /// Selects the specified target and deselects all other targets.
    /// </summary>
    /// <param name="target">
    /// The target or target definition to select.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified target is not contained in
    /// <see cref="TargetsSource"/>.
    /// </exception>
    public void Select(object target)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        if (!TryGetTargetEntry(
            target,
            out var activeTarget,
            out var activeDefinition))
        {
            throw new ArgumentException(
                "The specified target is not contained in the target source.",
                nameof(target));
        }

        foreach (var entry in GetTargetEntries())
        {
            if (entry.Target != null)
            {
                entry.Target.IsSelected =
                    ReferenceEquals(
                        entry.Target,
                        activeTarget);
            }

            if (entry.TargetDefinition != null)
            {
                entry.TargetDefinition.IsSelected =
                    ReferenceEquals(
                        entry.TargetDefinition,
                        activeDefinition);
            }
        }

        _activeTarget = activeTarget;
        _activeDefinition = activeDefinition;
        _selectedNode = null;
    }

    /// <summary>
    /// Adds the specified target to the current selection.
    /// </summary>
    /// <param name="target">
    /// The target or target definition to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified target is not contained in
    /// <see cref="TargetsSource"/>.
    /// </exception>
    public void AddSelection(object target)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        foreach (var entry in GetTargetEntries())
        {
            if (ReferenceEquals(
                entry.Target,
                target))
            {
                entry.Target!.IsSelected = true;
                _activeTarget = entry.Target;
                _activeDefinition = entry.TargetDefinition;
                _selectedNode = null;

                UpdateTargetInvalidation();
                return;
            }

            if (ReferenceEquals(
                entry.TargetDefinition,
                target))
            {
                entry.TargetDefinition!.IsSelected = true;
                _activeTarget = entry.Target;
                _activeDefinition = entry.TargetDefinition;
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
    /// <param name="target">
    /// The target or target definition to deselect.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    public void Deselect(object target)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        SetSelected(
            target,
            false);

        if (ReferenceEquals(
                _activeTarget,
                target) ||
            ReferenceEquals(
                _activeDefinition,
                target))
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
        foreach (var entry in GetTargetEntries())
        {
            if (entry.Target != null)
                entry.Target.IsSelected = false;

            if (entry.TargetDefinition != null)
                entry.TargetDefinition.IsSelected = false;
        }

        _activeTarget = null;
        _activeDefinition = null;
        _selectedNode = null;
    }

    private bool IsSelected(
        object target)
    {
        if (target is ISelectionTarget2
            selectionTarget)
        {
            return selectionTarget.IsSelected;
        }

        if (target
            is ISelectionTarget2Definition
                definition)
        {
            return definition.IsSelected;
        }

        return false;
    }

    private void SetSelected(
        object target,
        bool selected)
    {
        if (target is ISelectionTarget2
            selectionTarget)
        {
            selectionTarget.IsSelected = selected;
            return;
        }

        if (target
            is ISelectionTarget2Definition
                definition)
        {
            definition.IsSelected = selected;
        }
    }

    /// <summary>
    /// Performs a hit test against all available selection targets.
    /// </summary>
    /// <param name="touchedPosition">
    /// The position to test.
    /// </param>
    /// <returns>
    /// A result containing all targets intersecting the specified position.
    /// </returns>
    public SelectionTargetHitTestResult HitTest(
        Point2 touchedPosition)
    {
        var targets =
            new List<object>();

        foreach (var entry in GetTargetEntries())
        {
            if (!IsInTarget(
                touchedPosition,
                entry.Target,
                entry.TargetDefinition))
            {
                continue;
            }

            if (entry.Target != null)
                targets.Add(entry.Target);
            else if (entry.TargetDefinition != null)
                targets.Add(entry.TargetDefinition);
        }

        return new SelectionTargetHitTestResult(
            targets);
    }

    private bool IsInTarget(
        Point2 position,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (target == null &&
            definition == null)
        {
            return false;
        }

        Size2 targetSize =
            target?.Size ??
            definition!.Size;

        if (targetSize.Width <= 0f ||
            targetSize.Height <= 0f)
        {
            return false;
        }

        Point2 targetPosition =
            Point2.Zero;

        if (target is ISelectionMovable2
            movable)
        {
            targetPosition =
                movable.Position;
        }
        else if (definition
            is ISelectionMovable2Definition
                movableDefinition)
        {
            targetPosition =
                movableDefinition.Position;
        }

        Point2 localPosition =
            new Point2(
                position.X -
                    targetPosition.X,
                position.Y -
                    targetPosition.Y);

        if (Layer != null)
        {
            localPosition =
                Layer.InverseTransform(
                    localPosition,
                    target,
                    definition);
        }

        return
            localPosition.X >= 0f &&
            localPosition.X <= targetSize.Width &&
            localPosition.Y >= 0f &&
            localPosition.Y <= targetSize.Height;
    }

    private SelectionToolLayerContext
        GetLayerContext()
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
        _areaSelectionEnd =
            _cursorPosition;

        Bounds2 bounds =
            GetAreaSelectionBounds();

        foreach (var entry in GetTargetEntries())
        {
            if (!IsInAreaSelection(
                bounds,
                entry.Target,
                entry.TargetDefinition))
            {
                continue;
            }

            if (entry.Target != null)
                entry.Target.IsSelected = true;

            if (entry.TargetDefinition != null)
                entry.TargetDefinition.IsSelected = true;
        }

        _isAreaSelecting = false;
    }

    private Bounds2 GetAreaSelectionBounds()
    {
        float minX =
            float.Min(
                _areaSelectionStart.X,
                _areaSelectionEnd.X);

        float minY =
            float.Min(
                _areaSelectionStart.Y,
                _areaSelectionEnd.Y);

        float maxX =
            float.Max(
                _areaSelectionStart.X,
                _areaSelectionEnd.X);

        float maxY =
            float.Max(
                _areaSelectionStart.Y,
                _areaSelectionEnd.Y);

        return new Bounds2(
            new Point2(
                minX,
                minY),
            new Size2(
                maxX - minX,
                maxY - minY));
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

        Size2 size =
            target?.Size ??
            definition!.Size;

        Point2 position =
            Point2.Zero;

        if (target is ISelectionMovable2
            movable)
        {
            position =
                movable.Position;
        }
        else if (definition
            is ISelectionMovable2Definition
                movableDefinition)
        {
            position =
                movableDefinition.Position;
        }

        Point2 TransformPoint(
            float x,
            float y)
        {
            Point2 point =
                new Point2(x, y);

            if (Layer != null)
            {
                point = Layer.Transform(
                    point,
                    target,
                    definition);
            }

            return new Point2(
                position.X + point.X,
                position.Y + point.Y);
        }

        Point2 topLeft =
            TransformPoint(
                0f,
                0f);

        Point2 topRight =
            TransformPoint(
                size.Width,
                0f);

        Point2 bottomRight =
            TransformPoint(
                size.Width,
                size.Height);

        Point2 bottomLeft =
            TransformPoint(
                0f,
                size.Height);

        float minX = float.Min(
            float.Min(
                topLeft.X,
                topRight.X),
            float.Min(
                bottomLeft.X,
                bottomRight.X));

        float minY = float.Min(
            float.Min(
                topLeft.Y,
                topRight.Y),
            float.Min(
                bottomLeft.Y,
                bottomRight.Y));

        float maxX = float.Max(
            float.Max(
                topLeft.X,
                topRight.X),
            float.Max(
                bottomLeft.X,
                bottomRight.X));

        float maxY = float.Max(
            float.Max(
                topLeft.Y,
                topRight.Y),
            float.Max(
                bottomLeft.Y,
                bottomRight.Y));

        if (AllowAreaSelectionIntersection)
        {
            return
                area.X <= maxX &&
                area.X + area.Width >= minX &&
                area.Y <= maxY &&
                area.Y + area.Height >= minY;
        }

        return
            minX >= area.X &&
            maxX <= area.X + area.Width &&
            minY >= area.Y &&
            maxY <= area.Y + area.Height;
    }

    private readonly struct SelectionTargetEntry
    {
        public SelectionTargetEntry(IEngineObject engineObject)
        {
            Source = engineObject;
            Target = engineObject as ISelectionTarget2;
            TargetDefinition =
                engineObject.Definition as ISelectionTarget2Definition;
            TransformObserver = engineObject as ITransformChangeObserver;
            NotifyTransformChanged = engineObject as INotifyTransformChanged;
        }

        public SelectionTargetEntry(ISelectionTarget2 target)
        {
            Source = target as IEngineObject;
            Target = target;
            TargetDefinition =
                Source?.Definition as ISelectionTarget2Definition;
            TransformObserver = target as ITransformChangeObserver;
            NotifyTransformChanged = target as INotifyTransformChanged;
        }

        public IEngineObject? Source { get; }

        public ITransformChangeObserver? TransformObserver { get; }

        public INotifyTransformChanged? NotifyTransformChanged { get; }

        public ISelectionTarget2? Target { get; }

        public ISelectionTarget2Definition? TargetDefinition { get; }
    }
}