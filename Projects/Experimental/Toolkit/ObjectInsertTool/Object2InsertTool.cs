using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Rendering.Cameras;
using Sachssoft.Sasogine.Components.Tools;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Graphics.Rendering.Batches;
using Sachssoft.Sasogine.Input;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools;

/// <summary>
/// Provides a tool for inserting 2D object definitions by clicking or dragging in a viewport.
/// </summary>
public sealed class Object2InsertTool : ToolBase
{
    private readonly ShapeBatch _lineBatch;
    private readonly BasicShader _lineShader;

    private Vector2 _cursorPosition;
    private Vector2 _insertStart;
    private Vector2 _insertEnd;
    private bool _isInViewport;
    private bool _isInserting;
    private IDefinition? _insertDefinition;
    private ToolInteractions? _interactions;
    private bool _hasDragged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Object2InsertTool"/> class.
    /// </summary>
    /// <param name="definitions">
    /// The collection that receives inserted definitions.
    /// </param>
    /// <param name="insertHandler">
    /// The handler used to create and manage inserted definitions.
    /// </param>
    /// <param name="graphicsDevice">
    /// The graphics device used to create rendering resources.
    /// </param>
    public Object2InsertTool(
        IList<IDefinition> definitions,
        IObject2InsertHandler insertHandler,
        GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(insertHandler);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (definitions.IsReadOnly)
        {
            throw new ArgumentException(
                "The definition collection must be mutable.",
                nameof(definitions));
        }

        Definitions = definitions;
        InsertHandler = insertHandler;

        _lineBatch = new ShapeBatch(graphicsDevice);
        _lineShader = new BasicShader
        {
            GraphicsDevice = graphicsDevice
        };
    }

    /// <summary>
    /// Gets the collection that receives inserted definitions.
    /// </summary>
    public IList<IDefinition> Definitions { get; }

    /// <summary>
    /// Gets the handler used to manage 2D definition insertion operations.
    /// </summary>
    public IObject2InsertHandler InsertHandler { get; }

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
            new Point2(bounds.X, bounds.Y),
            new Point2(bounds.X + bounds.Width, bounds.Y),
            new Point2(bounds.X + bounds.Width, bounds.Y + bounds.Height),
            new Point2(bounds.X, bounds.Y + bounds.Height),
            new Point2(bounds.X, bounds.Y)
        ],
            LineThickness);

        _lineBatch.End();
    }

    /// <inheritdoc/>
    protected override void ApplyContext(ToolContext context)
    {
        _interactions = context.Interactions;
        _cursorPosition = context.CursorState.GetWorldPosition(context.Camera);
        _isInViewport = context.CursorState.IsInViewport;
    }

    private void BeginInsertion()
    {
        _insertStart = SnapPosition(_cursorPosition);
        _insertEnd = _insertStart;
        _hasDragged = false;

        _insertDefinition = InsertHandler.Create(
            CreateInsertContext());

        if (_insertDefinition == null)
        {
            throw new InvalidOperationException(
                "The object insert handler returned null.");
        }

        _isInserting = true;
    }

    private void DragInsertion()
    {
        if (_insertDefinition == null)
            return;

        _insertEnd = SnapPosition(_cursorPosition);

        if (!_hasDragged)
        {
            if (_insertEnd == _insertStart)
                return;

            _hasDragged = true;
        }

        InsertHandler.Drag(
            _insertDefinition,
            CreateInsertContext());
    }

    private void CompleteInsertion()
    {
        if (_insertDefinition == null)
            return;

        _insertEnd = SnapPosition(_cursorPosition);

        var definition = _insertDefinition;
        var insertContext = CreateInsertContext();

        try
        {
            InsertHandler.Complete(
                definition,
                insertContext);

            Definitions.Add(definition);
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

        var definition = _insertDefinition;

        try
        {
            if (definition != null)
            {
                InsertHandler.Cancel(
                    definition,
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
        _insertDefinition = null;
        _isInserting = false;
        _hasDragged = false;
    }

    private Object2InsertContext CreateInsertContext()
    {
        var bounds = GetInsertionBounds();

        return new Object2InsertContext
        {
            Position = new Point2(bounds.Location.X, bounds.Location.Y),
            Size = new Size2(bounds.Width, bounds.Height),
            DragStart = _insertStart,
            DragEnd = _insertEnd,
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
            new Point2(position.X, position.Y),
            new Size2(maximum - position));
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