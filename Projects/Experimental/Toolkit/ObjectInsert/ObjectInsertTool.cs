using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Graphics.Rendering.Batches;
using Sachssoft.Sasogine.Input;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;

namespace Sachssoft.Sasogine.Experimental.Components.Tools;

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
    private object? _insertObject;
    private ToolInteractions? _interactions;
    //private ObjectInsertToolInteractions? _interactions;
    private bool _hasDragged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectInsertTool"/> class.
    /// </summary>
    /// <param name="objectsSource">
    /// The collection that receives inserted objects.
    /// </param>
    /// <param name="insertHandler">
    /// The handler used to create and manage inserted objects.
    /// </param>
    /// <param name="graphicsDevice">
    /// The graphics device used to create rendering resources.
    /// </param>
    public ObjectInsertTool(
        IList objectsSource,
        IObjectInsertHandler insertHandler,
        GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(objectsSource);
        ArgumentNullException.ThrowIfNull(insertHandler);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (objectsSource.IsReadOnly)
        {
            throw new ArgumentException(
                "The object source must be mutable.",
                nameof(objectsSource));
        }

        if (objectsSource.IsFixedSize)
        {
            throw new ArgumentException(
                "The object source must allow objects to be added.",
                nameof(objectsSource));
        }

        ObjectsSource = objectsSource;
        InsertHandler = insertHandler;

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
    /// Gets the handler used to manage object insertion operations.
    /// </summary>
    public IObjectInsertHandler InsertHandler { get; }

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

        if (_interactions.Secondary.HasFlag(InteractionFlags.WasJustReleased))
        {
            CancelInsertion();
            return;
        }

        var action = _interactions.Primary;

        if (action.HasFlag(InteractionFlags.WasJustPressed) &&
            _isInViewport &&
            !_isInserting)
        {
            BeginInsertion();
        }

        if (action.HasFlag(InteractionFlags.IsPressed) &&
            _isInserting)
        {
            DragInsertion();
        }

        if (action.HasFlag(InteractionFlags.WasJustReleased) &&
            _isInserting)
        {
            if (_isInViewport)
                CompleteInsertion();
            else
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

    /// <inheritdoc/>
    protected internal override void ApplyInteractions(ToolInteractions interactions)
    {
        _interactions = interactions;
    }

    /// <inheritdoc/>
    protected internal override void ApplyCursor(ToolCursorContext cursorContext)
    {
        _cursorPosition = cursorContext.WorldPosition;
        _isInViewport = cursorContext.IsInViewport;
    }

    ///// <summary>
    ///// Sets the interaction bindings used by the object insertion tool.
    ///// </summary>
    //public void SetInteractions(ObjectInsertToolInteractions interactions)
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

    private void BeginInsertion()
    {
        _insertStart = SnapPosition(_cursorPosition);
        _insertEnd = _insertStart;
        _hasDragged = false;

        _insertObject = InsertHandler.Create(
            CreateInsertContext());

        if (_insertObject == null)
        {
            throw new InvalidOperationException(
                "The object insert handler returned null.");
        }

        _isInserting = true;
    }

    private void DragInsertion()
    {
        if (_insertObject == null)
            return;

        _insertEnd = SnapPosition(_cursorPosition);

        if (!_hasDragged)
        {
            if (_insertEnd == _insertStart)
                return;

            _hasDragged = true;
        }

        InsertHandler.Drag(
            _insertObject,
            CreateInsertContext());
    }

    private void CompleteInsertion()
    {
        if (_insertObject == null)
            return;

        _insertEnd = SnapPosition(_cursorPosition);

        var value = _insertObject;
        var insertContext = CreateInsertContext();

        try
        {
            InsertHandler.Complete(
                value,
                insertContext);

            ObjectsSource.Add(value);
        }
        finally
        {
            ResetInsertion();
        }
    }

    private void CancelInsertion()
    {
        if (!_isInserting)
            return;

        var value = _insertObject;

        try
        {
            if (value != null)
            {
                InsertHandler.Cancel(
                    value,
                    CreateInsertContext());
            }
        }
        finally
        {
            ResetInsertion();
        }
    }

    private void ResetInsertion()
    {
        _insertObject = null;
        _isInserting = false;
        _hasDragged = false;
    }

    private ObjectInsertContext CreateInsertContext()
    {
        var bounds = GetInsertionBounds();

        return new ObjectInsertContext
        {
            Position = bounds.Location,
            Size = new Size2(
                bounds.Width,
                bounds.Height),
            StartPosition = _insertStart,
            EndPosition = _insertEnd,
            IsDrag = _insertStart != _insertEnd
        };
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
}