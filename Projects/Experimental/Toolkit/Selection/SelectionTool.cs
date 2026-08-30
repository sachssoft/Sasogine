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
        internal readonly ShapeBatch _lineBatch;
        internal readonly ShapeBatch _pointBatch;
        private readonly ShapeBatch _fillBatch;

        private readonly BasicShader _lineShader;
        private readonly BasicShader _pointShader;
        private readonly BasicShader _fillShader;

        internal Vector2 _cursorPosition;
        internal bool _isInViewport;
        internal SelectionToolInteractions? _interactions;

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
        /// Gets or sets the selection tool layer used for interaction.
        /// </summary>
        public SelectionToolLayer? Layer { get; set; }

        /// <summary>
        /// Gets the currently selected node.
        /// </summary>
        public SelectionToolNode? SelectedNode { get; internal set; }

        /// <summary>
        /// Gets the source containing the objects that can be selected.
        /// </summary>
        public IEnumerable TargetsSource { get; }

        public bool EnableSnap { get; set; } = true;

        public Size2 GridSize { get; set; } = new Size2(10f);

        public Color SelectionColor { get; set; } = Color.DodgerBlue;

        public Color HandleColor { get; set; } = Color.White;

        public float LineThickness { get; set; } = 2f;

        public float HandleSize { get; set; } = 8f;

        public virtual void SetInteractions(
            SelectionToolInteractions interactions)
        {
            ArgumentNullException.ThrowIfNull(interactions);
            _interactions = interactions;
        }

        public virtual void SetCursorPosition(
            Vector2 position,
            bool isInViewport = true)
        {
            _cursorPosition = position;
            _isInViewport = isInViewport;
        }

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

            //Layer?.Update(
            //    new SelectionToolLayerUpdateContext(this));

            if (_interactions.Action.HasFlag(
                InteractionFlags.WasJustReleased))
            {
                HandleActionReleased();
            }
        }

        protected virtual void HandleActionPressed()
        {
            var hit = HitTest(_cursorPosition);

            bool modify = _interactions!.Modify.HasFlag(
                InteractionFlags.IsPressed);

            if (hit.Targets.Count == 0)
            {
                DeselectAll();
                return;
            }

            var target = hit.Targets[0];

            if (modify)
            {
                if (IsSelected(target))
                    Deselect(target);
                else
                    AddSelection(target);

                return;
            }

            if (!IsSelected(target))
            {
                Select(target);
                return;
            }

            //SelectedNode = Layer?.HitTest(_cursorPosition);
        }

        protected virtual void HandleActionReleased()
        {
            SelectedNode = null;
        }

        protected virtual void CancelInteraction()
        {
            //Layer?.Cancel();

            SelectedNode = null;
            DeselectAll();
        }

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

            //Layer?.Draw(
            //    new SelectionToolLayerDrawContext(this));

            _fillBatch.End();
            _lineBatch.End();
            _pointBatch.End();
        }

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

        internal IEnumerable GetSelectedTargets()
        {
            foreach (var item in GetTargets())
            {
                if (IsSelected(item))
                    yield return item;
            }
        }

        protected virtual void DrawSelections()
        {
            foreach (var item in GetTargets())
            {
                if (!IsSelected(item))
                    continue;

                DrawSelection(item);
            }
        }

        protected virtual void DrawSelection(
            object obj)
        {
            Vector2 position = Vector2.Zero;
            Size2 size = Size2.Zero;
            float rotation = 0f;

            if (obj is ISelectionTarget selectionTarget)
            {
                if (selectionTarget is ISelectionMovable2 movable)
                    position = movable.Position;

                if (selectionTarget is ISelectionResizable2 resizable)
                    size = resizable.Size;

                if (selectionTarget is ISelectionRotatable2 rotatable)
                    rotation = rotatable.Rotation;
            }
            else if (obj is ISelectionTargetDefinition definition)
            {
                if (definition is ISelectionMovable2Definition movableDefinition)
                    position = movableDefinition.Position;

                if (definition is ISelectionResizable2Definition resizableDefinition)
                    size = resizableDefinition.Size;

                if (definition is ISelectionRotatable2Definition rotatableDefinition)
                    rotation = rotatableDefinition.Rotation;
            }
            else
            {
                return;
            }

            var lineOffset = LineThickness / 2f;
            var cos = MathF.Cos(rotation);
            var sin = MathF.Sin(rotation);

            Vector2 TransformPoint(float x, float y)
            {
                return position + new Vector2(
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
        }

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

            SelectedNode = null;
        }

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

        public virtual void Deselect(
            object target)
        {
            ArgumentNullException.ThrowIfNull(target);
            SetSelected(target, false);
        }

        public virtual void DeselectAll()
        {
            foreach (var item in GetTargets())
                SetSelected(item, false);

            SelectedNode = null;
        }

        internal bool IsSelected(
            object target)
        {
            if (target is ISelectionTarget selectionTarget)
                return selectionTarget.IsSelected;

            if (target is ISelectionTargetDefinition definition)
                return definition.IsSelected;

            return false;
        }

        internal void SetSelected(
            object target,
            bool selected)
        {
            if (target is ISelectionTarget selectionTarget)
            {
                selectionTarget.IsSelected = selected;
                return;
            }

            if (target is ISelectionTargetDefinition definition)
                definition.IsSelected = selected;
        }

        internal void MoveTarget(
            object target,
            Vector2 delta)
        {
            if (target is ISelectionTarget selectionTarget)
            {
                if (selectionTarget is ISelectionMovable2 movable &&
                    movable.AllowMove)
                {
                    movable.Position += delta;
                }

                return;
            }

            if (target is ISelectionTargetDefinition definition &&
                definition is ISelectionMovable2Definition movableDefinition)
            {
                movableDefinition.Position += delta;
            }
        }

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

        internal Vector2 SnapPosition(
            Vector2 position)
        {
            if (!EnableSnap)
                return position;

            return new Vector2(
                MathF.Round(position.X / GridSize.Width) * GridSize.Width,
                MathF.Round(position.Y / GridSize.Height) * GridSize.Height);
        }

        public virtual SelectionTargetHitTestResult HitTest(
            Vector2 touchedPosition)
        {
            var targets = new List<object>();

            foreach (var item in GetTargets())
            {
                if (IsInTarget(touchedPosition, item))
                    targets.Add(item);
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

            if (target is ISelectionTarget selectionTarget)
            {
                if (selectionTarget is ISelectionMovable2 movable)
                    targetPosition = movable.Position;

                if (selectionTarget is ISelectionResizable2 resizable)
                    targetSize = resizable.Size;

                if (selectionTarget is ISelectionRotatable2 rotatable)
                    rotation = rotatable.Rotation;
            }
            else if (target is ISelectionTargetDefinition definition)
            {
                if (definition is ISelectionMovable2Definition movableDefinition)
                    targetPosition = movableDefinition.Position;

                if (definition is ISelectionResizable2Definition resizableDefinition)
                    targetSize = resizableDefinition.Size;

                if (definition is ISelectionRotatable2Definition rotatableDefinition)
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
    }
}