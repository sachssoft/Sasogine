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

namespace Sachssoft.Sasogine.Components.Tools
{
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

        private SelectionToolInteractions? _interactions;

        private bool _isPressed;
        private bool _isMoving;
        private Vector2 _moveStartPosition;

        private SelectionToolMode _mode;

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

            _mode = SelectionToolMode.None;
        }

        /// <summary>
        /// Gets the source containing the objects that can be selected.
        /// </summary>
        public IEnumerable TargetsSource { get; }

        /// <summary>
        /// Gets the current transformation mode of the selection tool.
        /// </summary>
        public SelectionToolMode Mode => _mode;

        /// <summary>
        /// Gets or sets whether snapping is enabled for the tool.
        /// </summary>
        public bool EnableSnap { get; set; } = true;

        /// <summary>
        /// Gets or sets the size of the snapping grid.
        /// </summary>
        public Size GridSize { get; set; } = new Size(10f);

        /// <summary>
        /// Gets or sets the color used to draw selection outlines.
        /// </summary>
        public Color SelectionColor { get; set; } = Color.DodgerBlue;

        /// <summary>
        /// Gets or sets the color used to draw selection handles.
        /// </summary>
        public Color HandleColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the thickness of selection outlines.
        /// </summary>
        public float LineThickness { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the size of selection handles.
        /// </summary>
        public float HandleSize { get; set; } = 8f;

        /// <summary>
        /// Sets the interactions used by the selection tool.
        /// </summary>
        /// <param name="interactions">
        /// The interactions used by the selection tool.
        /// </param>
        public virtual void SetInteractions(
            SelectionToolInteractions interactions)
        {
            ArgumentNullException.ThrowIfNull(interactions);

            _interactions = interactions;
        }

        /// <summary>
        /// Sets the current cursor position.
        /// </summary>
        /// <param name="position">
        /// The cursor position.
        /// </param>
        /// <param name="isInViewport">
        /// Indicates whether the cursor is currently inside the viewport.
        /// </param>
        public virtual void SetCursorPosition(
            Vector2 position,
            bool isInViewport = true)
        {
            _cursorPosition = position;
            _isInViewport = isInViewport;
        }

        /// <summary>
        /// Updates the selection tool.
        /// </summary>
        /// <param name="context">
        /// The scene update context.
        /// </param>
        public virtual void Update(
            SceneUpdateContext context)
        {
            if (!_isInViewport ||
                _interactions == null)
            {
                return;
            }

            if (_interactions.Cancel.HasFlag(
                InteractionFlags.WasJustReleased))
            {
                CancelInteraction();
                return;
            }

            if (_interactions.Action.HasFlag(
                InteractionFlags.WasJustPressed))
            {
                HandleActionPressed();
            }

            if (_interactions.Action.HasFlag(
                InteractionFlags.IsPressed))
            {
                HandleActionPressedState();
            }

            if (_interactions.Action.HasFlag(
                InteractionFlags.WasJustReleased))
            {
                HandleActionReleased();
            }
        }

        /// <summary>
        /// Handles the initial press of the selection action.
        /// </summary>
        protected virtual void HandleActionPressed()
        {
            var hit = HitTest(_cursorPosition);

            bool modify = _interactions!.Modify.HasFlag(
                InteractionFlags.IsPressed);

            if (hit.Targets.Count == 0)
            {
                DeselectAll();

                _mode = SelectionToolMode.None;
                _isPressed = true;
                _isMoving = false;

                return;
            }

            var target = hit.Targets[0];

            if (modify)
            {
                if (IsSelected(target))
                {
                    Deselect(target);
                }
                else
                {
                    AddSelection(target);
                }

                _mode = SelectionToolMode.None;
                _isPressed = true;
                _isMoving = false;

                return;
            }

            if (!IsSelected(target))
            {
                Select(target);

                _mode = SelectionToolMode.None;
                _isMoving = false;
            }
            else
            {
                _mode = SelectionToolMode.None;

                _moveStartPosition =
                    SnapPosition(_cursorPosition);

                _isMoving = true;
            }

            _isPressed = true;
        }

        /// <summary>
        /// Handles the pressed state of the selection action.
        /// </summary>
        protected virtual void HandleActionPressedState()
        {
            if (!_isPressed ||
                !_isMoving ||
                _mode != SelectionToolMode.None)
            {
                return;
            }

            var currentPosition =
                SnapPosition(_cursorPosition);

            var delta =
                currentPosition -
                _moveStartPosition;

            if (delta == Vector2.Zero)
                return;

            foreach (var item in GetTargets())
            {
                if (!IsSelected(item))
                    continue;

                MoveTarget(item, delta);
            }

            // Wichtig:
            // Der Startpunkt bleibt ebenfalls auf dem Snap-Raster.
            _moveStartPosition = currentPosition;
        }

        /// <summary>
        /// Handles the release of the selection action.
        /// </summary>
        protected virtual void HandleActionReleased()
        {
            _isPressed = false;
            _isMoving = false;
            _mode = SelectionToolMode.None;
        }

        /// <summary>
        /// Cancels the current selection interaction.
        /// </summary>
        protected virtual void CancelInteraction()
        {
            _mode = SelectionToolMode.None;
            _isPressed = false;
            _isMoving = false;

            DeselectAll();
        }

        /// <summary>
        /// Draws the selection tool.
        /// </summary>
        /// <param name="context">
        /// The scene draw context.
        /// </param>
        public virtual void Draw(
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

            _lineShader.Color = SelectionColor;
            _lineShader.Opacity = 1f;
            _lineShader.Camera = context.ViewCamera;
            _lineShader.Apply();

            _pointShader.Color = HandleColor;
            _pointShader.Opacity = 1f;
            _pointShader.Camera = context.ViewCamera;
            _pointShader.Apply();

            _fillShader.Color = SelectionColor;
            _fillShader.Opacity = 1f;
            _fillShader.Camera = context.ViewCamera;
            _fillShader.Apply();

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

            _fillBatch.End();
            _pointBatch.End();
            _lineBatch.End();
        }

        /// <summary>
        /// Enumerates all selection targets from the target source.
        /// Direct <see cref="ISelectionTarget"/> objects have priority.
        /// If an object does not implement <see cref="ISelectionTarget"/>,
        /// its <see cref="IEngineObject.Definition"/> can provide an
        /// <see cref="ISelectionTargetDefinition"/>.
        /// </summary>
        protected virtual IEnumerable GetTargets()
        {
            foreach (var item in TargetsSource)
            {
                if (item is ISelectionTarget target)
                {
                    yield return target;
                }
                else if (item is IEngineObject engineObject &&
                         engineObject.Definition is ISelectionTargetDefinition definition)
                {
                    yield return definition;
                }
            }
        }

        /// <summary>
        /// Draws the selection representation for all selected targets.
        /// </summary>
        protected virtual void DrawSelections()
        {
            foreach (var item in GetTargets())
            {
                if (!IsSelected(item))
                    continue;

                DrawSelection(item);
            }
        }

        /// <summary>
        /// Draws the selection representation for the specified target.
        /// </summary>
        /// <param name="obj">
        /// The selection target or selection target definition.
        /// </param>
        protected virtual void DrawSelection(
            object obj)
        {
            Vector2 position = Vector2.Zero;
            Size size = Size.Zero;
            float rotation = 0f;
            bool allowResize = false;
            bool allowRotate = false;

            if (obj is ISelectionTarget selectionTarget)
            {
                if (selectionTarget is ISelectionMovable movable)
                {
                    position = movable.Position;
                }

                if (selectionTarget is ISelectionResizable resizable)
                {
                    size = resizable.Size;
                    allowResize = resizable.AllowResize;
                }

                if (selectionTarget is ISelectionRotatable rotatable)
                {
                    rotation = rotatable.Rotation;
                    allowRotate = rotatable.AllowRotate;
                }
            }
            else if (obj is ISelectionTargetDefinition definition)
            {
                if (definition is ISelectionMovableDefinition movableDefinition)
                {
                    position = movableDefinition.Position;
                }

                if (definition is ISelectionResizableDefinition resizableDefinition)
                {
                    size = resizableDefinition.Size;
                    allowResize = true;
                }

                if (definition is ISelectionRotatableDefinition rotatableDefinition)
                {
                    rotation = rotatableDefinition.Rotation;
                    allowRotate = true;
                }
            }
            else
            {
                return;
            }

            var lineOffset = LineThickness / 2f;

            var cos = MathF.Cos(rotation);
            var sin = MathF.Sin(rotation);

            Vector2 TransformPoint(
                float x,
                float y)
            {
                return position +
                    new Vector2(
                        x * cos - y * sin,
                        x * sin + y * cos);
            }

            var topLeft = TransformPoint(
                -lineOffset,
                -lineOffset);

            var topRight = TransformPoint(
                size.Width + lineOffset,
                -lineOffset);

            var bottomRight = TransformPoint(
                size.Width + lineOffset,
                size.Height + lineOffset);

            var bottomLeft = TransformPoint(
                -lineOffset,
                size.Height + lineOffset);

            _lineBatch.AddLine(
                [
                    topLeft,
                    topRight,
                    bottomRight,
                    bottomLeft,
                    topLeft
                ],
                LineThickness);

            if (allowResize)
            {
                DrawHandle(topLeft);
                DrawHandle(topRight);
                DrawHandle(bottomRight);
                DrawHandle(bottomLeft);
            }

            if (allowRotate)
            {
                DrawRotationHandle(
                    topLeft,
                    topRight);
            }
        }

        /// <summary>
        /// Draws the rotation handle for a selection.
        /// </summary>
        protected virtual void DrawRotationHandle(
            Vector2 topLeft,
            Vector2 topRight)
        {
            var topCenter = Vector2.Lerp(
                topLeft,
                topRight,
                0.5f);

            var edge = topRight - topLeft;

            if (edge == Vector2.Zero)
                return;

            edge.Normalize();

            var normal = new Vector2(
                edge.Y,
                -edge.X);

            var rotationHandle = topCenter +
                normal *
                HandleSize *
                2f;

            _lineBatch.AddLine(
                [
                    topCenter,
                    rotationHandle
                ],
                LineThickness);

            DrawHandle(rotationHandle);
        }

        /// <summary>
        /// Draws a selection handle.
        /// </summary>
        protected virtual void DrawHandle(
            Vector2 position)
        {
            var halfSize = HandleSize / 2f;

            _pointBatch.AddFillRectangle(
                new Bounds(
                    position.X - halfSize,
                    position.Y - halfSize,
                    HandleSize,
                    HandleSize));
        }

        /// <summary>
        /// Selects the specified target and deselects all other targets.
        /// </summary>
        public virtual void Select(
            object target)
        {
            ArgumentNullException.ThrowIfNull(target);

            if (!ContainsTarget(target))
            {
                throw new ArgumentException(
                    "The specified target is not contained in the target source.",
                    nameof(target));
            }

            foreach (var item in GetTargets())
            {
                SetSelected(
                    item,
                    ReferenceEquals(item, target));
            }
        }

        /// <summary>
        /// Adds the specified target to the current selection.
        /// </summary>
        public virtual void AddSelection(
            object target)
        {
            ArgumentNullException.ThrowIfNull(target);

            if (!ContainsTarget(target))
            {
                throw new ArgumentException(
                    "The specified target is not contained in the target source.",
                    nameof(target));
            }

            SetSelected(target, true);
        }

        /// <summary>
        /// Removes the specified target from the current selection.
        /// </summary>
        public virtual void Deselect(
            object target)
        {
            ArgumentNullException.ThrowIfNull(target);

            SetSelected(target, false);
        }

        /// <summary>
        /// Deselects all targets.
        /// </summary>
        public virtual void DeselectAll()
        {
            foreach (var item in GetTargets())
            {
                SetSelected(item, false);
            }

            _mode = SelectionToolMode.None;
            _isMoving = false;
        }

        /// <summary>
        /// Determines whether the specified target is selected.
        /// </summary>
        protected virtual bool IsSelected(
            object target)
        {
            if (target is ISelectionTarget selectionTarget)
                return selectionTarget.IsSelected;

            if (target is ISelectionTargetDefinition definition)
                return definition.IsSelected;

            return false;
        }

        /// <summary>
        /// Sets the selection state of the specified target.
        /// </summary>
        protected virtual void SetSelected(
            object target,
            bool selected)
        {
            if (target is ISelectionTarget selectionTarget)
            {
                selectionTarget.IsSelected = selected;
                return;
            }

            if (target is ISelectionTargetDefinition definition)
            {
                definition.IsSelected = selected;
            }
        }

        /// <summary>
        /// Moves the specified selection target by the given delta.
        /// </summary>
        protected virtual void MoveTarget(
            object target,
            Vector2 delta)
        {
            if (target is ISelectionTarget selectionTarget)
            {
                if (selectionTarget is ISelectionMovable movable &&
                    movable.AllowMove)
                {
                    movable.Position += delta;
                }

                return;
            }

            if (target is ISelectionTargetDefinition definition &&
                definition is ISelectionMovableDefinition movableDefinition)
            {
                movableDefinition.Position += delta;
            }
        }

        /// <summary>
        /// Determines whether the specified target exists in the target source.
        /// </summary>
        protected virtual bool ContainsTarget(
            object target)
        {
            foreach (var item in GetTargets())
            {
                if (ReferenceEquals(item, target))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Snaps the specified position to the selection tool grid.
        /// </summary>
        protected virtual Vector2 SnapPosition(
            Vector2 position)
        {
            if (!EnableSnap)
                return position;

            return new Vector2(
                MathF.Round(
                    position.X /
                    GridSize.Width) *
                    GridSize.Width,
                MathF.Round(
                    position.Y /
                    GridSize.Height) *
                    GridSize.Height);
        }

        /// <summary>
        /// Performs a hit test against all registered selection targets.
        /// </summary>
        public virtual SelectionTargetHitTestResult HitTest(
            Vector2 touchedPosition)
        {
            var targets = new List<object>();

            foreach (var item in GetTargets())
            {
                if (IsInTarget(
                    touchedPosition,
                    item))
                {
                    targets.Add(item);
                }
            }

            return new SelectionTargetHitTestResult(targets);
        }

        /// <summary>
        /// Determines whether the specified position is inside a selection target.
        /// </summary>
        protected virtual bool IsInTarget(
            Vector2 position,
            object target)
        {
            Vector2 targetPosition = Vector2.Zero;
            Size targetSize = Size.Zero;
            float rotation = 0f;

            if (target is ISelectionTarget selectionTarget)
            {
                if (selectionTarget is ISelectionMovable movable)
                    targetPosition = movable.Position;

                if (selectionTarget is ISelectionResizable resizable)
                    targetSize = resizable.Size;

                if (selectionTarget is ISelectionRotatable rotatable)
                    rotation = rotatable.Rotation;
            }
            else if (target is ISelectionTargetDefinition definition)
            {
                if (definition is ISelectionMovableDefinition movableDefinition)
                    targetPosition = movableDefinition.Position;

                if (definition is ISelectionResizableDefinition resizableDefinition)
                    targetSize = resizableDefinition.Size;

                if (definition is ISelectionRotatableDefinition rotatableDefinition)
                    rotation = rotatableDefinition.Rotation;
            }
            else
            {
                return false;
            }

            if (targetSize.Width <= 0f ||
                targetSize.Height <= 0f)
            {
                return false;
            }

            var relative = position - targetPosition;

            var cos = MathF.Cos(rotation);
            var sin = MathF.Sin(rotation);

            var localPosition = new Vector2(
                relative.X * cos + relative.Y * sin,
                -relative.X * sin + relative.Y * cos);

            return localPosition.X >= 0f &&
                   localPosition.X <= targetSize.Width &&
                   localPosition.Y >= 0f &&
                   localPosition.Y <= targetSize.Height;
        }

        /// <summary>
        /// Sets the current transformation mode.
        /// </summary>
        protected virtual void SetMode(
            SelectionToolMode mode)
        {
            _mode = mode;
        }
    }
}