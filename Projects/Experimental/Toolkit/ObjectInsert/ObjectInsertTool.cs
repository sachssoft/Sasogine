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

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Provides a tool for inserting objects by clicking or dragging in a viewport.
/// </summary>
public sealed class ObjectInsertTool : ToolBase
{
    private readonly ShapeBatch _lineBatch;
    private readonly BasicShader _lineShader;

    private Vector2 _cursorPosition;
    private Vector2 _insertStart;
    private Vector2 _insertEnd;
    private bool _isInViewport;
    private bool _isInserting;
    private ObjectInsertToolInteractions? _interactions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectInsertTool"/> class.
    /// </summary>
    /// <param name="objectsSource">The collection that receives inserted objects.</param>
    /// <param name="graphicsDevice">The graphics device used to create rendering resources.</param>
    public ObjectInsertTool(
        IList objectsSource,
        GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(objectsSource);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        ObjectsSource = objectsSource;
        _lineBatch = new ShapeBatch(graphicsDevice);
        _lineShader = new BasicShader
        {
            GraphicsDevice = graphicsDevice
        };
    }

    /// <summary>
    /// Gets the collection that receives inserted objects.
    /// </summary>
    public IList ObjectsSource { get; }

    /// <summary>
    /// Gets or sets the factory used to create an object for insertion.
    /// </summary>
    public Func<object>? ObjectFactory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether grid-based snapping is enabled.
    /// </summary>
    public bool EnableGridSnap { get; set; } = true;

    /// <summary>
    /// Gets or sets the horizontal and vertical step used for grid-based snapping.
    /// </summary>
    public Size2 GridSnapStep { get; set; } = new Size2(10f);

    /// <summary>
    /// Gets or sets the color used to draw the insertion outline.
    /// </summary>
    public Color Color { get; set; } = Color.DodgerBlue;

    /// <summary>
    /// Gets or sets the thickness of the insertion outline.
    /// </summary>
    public float LineThickness { get; set; } = 2f;

    /// <inheritdoc/>
    public override void Update(SceneUpdateContext context)
    {
        if (_interactions == null)
            return;

        if (_interactions.Cancel.HasFlag(InteractionFlags.WasJustReleased))
        {
            CancelInsertion();
            return;
        }

        var action = _interactions.Action;

        if (action.HasFlag(InteractionFlags.WasJustPressed) &&
            _isInViewport)
        {
            _insertStart = SnapPosition(_cursorPosition);
            _insertEnd = _insertStart;
            _isInserting = true;
        }

        if (action.HasFlag(InteractionFlags.IsPressed) &&
            _isInserting)
        {
            _insertEnd = SnapPosition(_cursorPosition);
        }

        if (action.HasFlag(InteractionFlags.WasJustReleased) &&
            _isInserting)
        {
            if (_isInViewport)
            {
                _insertEnd = SnapPosition(_cursorPosition);
                InsertObject();
            }

            CancelInsertion();
        }
    }

    /// <inheritdoc/>
    public override void Draw(SceneDrawContext context)
    {
        if (!_isInserting ||
            _insertStart == _insertEnd)
        {
            return;
        }

        using var scope = new RenderScope(
            context.GraphicsDevice,
            new RenderOptions
            {
                CullMode = CullMode.None,
                Depth = DepthMode.Disabled,
                AlphaBlend = true
            });

        _lineShader.Color = Color;
        _lineShader.Opacity = 1f;
        _lineShader.Camera = context.ViewCamera;
        _lineShader.Apply();

        _lineBatch.Begin(
            shader: _lineShader,
            camera: context.ViewCamera);

        var bounds = GetInsertionBounds();

        _lineBatch.AddLine(
            [
                new Vector2(bounds.X, bounds.Y),
                new Vector2(bounds.X + bounds.Width, bounds.Y),
                new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height),
                new Vector2(bounds.X, bounds.Y + bounds.Height),
                new Vector2(bounds.X, bounds.Y)
            ],
            LineThickness);

        _lineBatch.End();
    }

    /// <summary>
    /// Sets the interaction bindings used by the object insertion tool.
    /// </summary>
    public void SetInteractions(ObjectInsertToolInteractions interactions)
    {
        ArgumentNullException.ThrowIfNull(interactions);
        _interactions = interactions;
    }

    /// <summary>
    /// Sets the current cursor position and viewport state.
    /// </summary>
    /// <param name="position">The current cursor position.</param>
    /// <param name="isInViewport">
    /// Indicates whether the cursor is currently inside the active viewport.
    /// </param>
    public void SetCursorPosition(
        Vector2 position,
        bool isInViewport = true)
    {
        _cursorPosition = position;
        _isInViewport = isInViewport;
    }

    private void InsertObject()
    {
        if (ObjectFactory == null)
            return;

        var value = ObjectFactory();

        if (value == null)
        {
            throw new InvalidOperationException(
                "The object factory returned null.");
        }

        GetTargetPair(
            value,
            out var target,
            out var definition);

        if (target == null &&
            definition == null)
        {
            throw new InvalidOperationException(
                "The created object must provide an ISelectionTarget2 " +
                "or ISelectionTarget2Definition.");
        }

        if (_insertStart == _insertEnd)
        {
            ApplyPosition(
                target,
                definition,
                _insertStart);
        }
        else
        {
            var bounds = GetInsertionBounds();

            ApplyPosition(
                target,
                definition,
                bounds.Location);

            ApplySize(
                target,
                definition,
                new Size2(bounds.Width, bounds.Height));
        }

        ObjectsSource.Add(value);
    }

    private Bounds2 GetInsertionBounds()
    {
        var position = Vector2.Min(
            _insertStart,
            _insertEnd);

        var maximum = Vector2.Max(
            _insertStart,
            _insertEnd);

        return new Bounds2(
            position,
            maximum - position);
    }

    private Vector2 SnapPosition(Vector2 position)
    {
        if (!EnableGridSnap)
            return position;

        if (GridSnapStep.Width > 0f)
        {
            position.X = MathF.Round(
                position.X / GridSnapStep.Width) *
                GridSnapStep.Width;
        }

        if (GridSnapStep.Height > 0f)
        {
            position.Y = MathF.Round(
                position.Y / GridSnapStep.Height) *
                GridSnapStep.Height;
        }

        return position;
    }

    private static void ApplyPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Vector2 position)
    {
        if (target is ISelectionMovable2 movable)
            movable.Position = position;

        if (definition is ISelectionMovable2Definition movableDefinition)
            movableDefinition.Position = position;
    }

    private static void ApplySize(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Size2 size)
    {
        if (target != null)
            target.Size = size;

        if (definition != null)
            definition.Size = size;
    }

    private static void GetTargetPair(
        object value,
        out ISelectionTarget2? target,
        out ISelectionTarget2Definition? definition)
    {
        target = value as ISelectionTarget2;
        definition = value as ISelectionTarget2Definition;

        if (value is IEngineObject engineObject &&
            engineObject.Definition is ISelectionTarget2Definition targetDefinition)
        {
            definition = targetDefinition;
        }
    }

    private void CancelInsertion()
    {
        _isInserting = false;
    }
}