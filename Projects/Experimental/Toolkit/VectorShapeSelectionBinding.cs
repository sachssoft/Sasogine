using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using Sachssoft.Sasogine.Experimental.Components.Tools.Selection;
using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Connects a <see cref="VectorShape"/> with a two-dimensional
    /// selection target.
    /// </summary>
    /// <remarks>
    /// The binding synchronizes selection transformations with the vector
    /// geometry and refits the selection bounds after vector editing.
    /// </remarks>
    public sealed class VectorShapeSelectionBinding
    {
        private readonly VectorShape _shape;
        private readonly ISelectionTarget2Definition _target;
        private readonly ISelectionMovable2Definition _movable;

        private Matrix _lastTransform = Matrix.Identity;
        private bool _isInitialized;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="VectorShapeSelectionBinding"/> class.
        /// </summary>
        /// <param name="shape">
        /// The vector shape whose geometry is synchronized.
        /// </param>
        /// <param name="target">
        /// The selection target definition associated with the shape.
        /// </param>
        public VectorShapeSelectionBinding(
            VectorShape shape,
            ISelectionTarget2Definition target)
        {
            ArgumentNullException.ThrowIfNull(shape);
            ArgumentNullException.ThrowIfNull(target);

            if (target is not ISelectionMovable2Definition movable)
            {
                throw new ArgumentException(
                    "The selection target must support two-dimensional movement.",
                    nameof(target));
            }

            if (target is not ISelectionResizable2Definition)
            {
                throw new ArgumentException(
                    "The selection target must support two-dimensional resizing.",
                    nameof(target));
            }

            _shape = shape;
            _target = target;
            _movable = movable;

            _shape.Changing += ShapeChanging;
            _shape.Changed += ShapeChanged;
        }

        /// <summary>
        /// Gets the associated vector shape.
        /// </summary>
        public VectorShape Shape => _shape;

        /// <summary>
        /// Gets the associated selection target definition.
        /// </summary>
        public ISelectionTarget2Definition Target => _target;

        /// <summary>
        /// Occurs while the vector geometry is being edited.
        /// </summary>
        public event EventHandler? Changing;

        /// <summary>
        /// Occurs after the vector geometry or its selection transformation
        /// has been completed.
        /// </summary>
        public event EventHandler? Changed;

        /// <summary>
        /// Initializes the vector geometry and establishes the initial
        /// transformation state.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;

            Trim();

            _lastTransform =
                SelectionToolExtensions.ToMatrix(_target);

            _shape.UpdateState();

            _isInitialized = true;

            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the transformation that must be applied while a selection
        /// transformation is in progress.
        /// </summary>
        /// <returns>
        /// The transformation relative to the last transformation that was
        /// baked into the vector nodes.
        /// </returns>
        public Matrix GetLiveTransform()
        {
            Matrix transform =
                SelectionToolExtensions.ToMatrix(_target);

            if (transform == _lastTransform)
                return Matrix.Identity;

            if (MathF.Abs(_lastTransform.Determinant()) <= 1e-6f)
                return Matrix.Identity;

            return Matrix.Invert(_lastTransform) * transform;
        }

        /// <summary>
        /// Notifies the binding that a selection transformation has started.
        /// </summary>
        public void OnTransformStarted()
        {
        }

        /// <summary>
        /// Notifies the binding that a selection transformation is changing.
        /// </summary>
        public void OnTransformChanged()
        {
        }

        /// <summary>
        /// Notifies the binding that a selection transformation has completed.
        /// </summary>
        public void OnTransformCompleted()
        {
            TransformNodes();
            _shape.UpdateState();

            Changed?.Invoke(
                this,
                EventArgs.Empty);
        }

        /// <summary>
        /// Refits the selection position and size to the current vector
        /// geometry while preserving rotation, scale, and rotation pivot.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when the bounds were successfully fitted;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool FitSelectionBounds()
        {
            var polygons = _shape.GetVectorVertices(
                ValueMode.Absolute,
                trim: false);

            if (!_target.FitBoundsPreserveRotation(polygons))
                return false;

            _lastTransform =
                SelectionToolExtensions.ToMatrix(_target);

            _shape.UpdateState();

            return true;
        }

        private void ShapeChanging(
            object? sender,
            EventArgs e)
        {
            Changing?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void ShapeChanged(
            object? sender,
            EventArgs e)
        {
            FitSelectionBounds();

            Changed?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void Trim()
        {
            if (!_shape.TryGetBounds(out Bounds2 bounds))
                return;

            Vector2 offset = new(
                bounds.X,
                bounds.Y);

            _movable.Position = new Point2(
                _movable.Position.X + offset.X,
                _movable.Position.Y + offset.Y);

            Vector2 position =
                _movable.Position.ToVector2();

            foreach (var path in _shape.Paths)
            {
                SetNodePosition(
                    path.Start,
                    new Point2(
                        path.Start.Position.X - offset.X + position.X,
                        path.Start.Position.Y - offset.Y + position.Y));

                foreach (var segment in path.Segments)
                {
                    SetNodePosition(
                        segment.Node,
                        new Point2(
                            segment.Node.Position.X - offset.X + position.X,
                            segment.Node.Position.Y - offset.Y + position.Y));

                    foreach (var controlNode in segment.GetControlNodes())
                    {
                        SetNodePosition(
                            controlNode,
                            new Point2(
                                controlNode.Position.X - offset.X + position.X,
                                controlNode.Position.Y - offset.Y + position.Y));
                    }
                }
            }
        }

        private void TransformNodes()
        {
            Matrix transform =
                SelectionToolExtensions.ToMatrix(_target);

            if (transform == _lastTransform)
                return;

            if (MathF.Abs(_lastTransform.Determinant()) <= 1e-6f)
            {
                _lastTransform = transform;
                return;
            }

            Matrix delta =
                Matrix.Invert(_lastTransform) *
                transform;

            foreach (var path in _shape.Paths)
            {
                TransformNode(
                    path.Start,
                    delta);

                foreach (var segment in path.Segments)
                {
                    TransformNode(
                        segment.Node,
                        delta);

                    foreach (var controlNode in segment.GetControlNodes())
                    {
                        TransformNode(
                            controlNode,
                            delta);
                    }
                }
            }

            _lastTransform = transform;
        }

        private static void TransformNode(
            VectorNode node,
            Matrix transform)
        {
            Vector2 position = Vector2.Transform(
                node.Position.ToVector2(),
                transform);

            SetNodePosition(
                node,
                new Point2(
                    position.X,
                    position.Y));
        }

        private static void SetNodePosition(
            VectorNode node,
            Point2 position)
        {
            node.Definition.Position = position;
            node.Reload();
        }
    }
}