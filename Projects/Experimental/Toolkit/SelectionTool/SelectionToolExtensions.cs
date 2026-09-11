using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Selection
{
    /// <summary>
    /// Provides extension methods for creating transformation matrices
    /// from selection targets and definitions.
    /// </summary>
    public static class SelectionToolExtensions
    {
        /// <summary>
        /// Creates a transformation matrix from the selection properties
        /// defined by the specified selection target.
        /// </summary>
        /// <param name="target">
        /// The selection target used to create the transformation matrix.
        /// </param>
        /// <returns>
        /// A transformation matrix containing the target's size, scale,
        /// rotation, rotation pivot, and position.
        /// </returns>
        public static Matrix ToMatrix(
            this ISelectionTarget target)
        {
            ArgumentNullException.ThrowIfNull(target);

            return CreateMatrix(
                target is ISelectionResizable2 resizable
                    ? resizable.Size
                    : null,
                target is ISelectionScalable2 scalable
                    ? scalable.Scale
                    : null,
                target is ISelectionRotatable2 rotatable
                    ? (rotatable.Rotation, rotatable.RotationPivot)
                    : null,
                target is ISelectionMovable2 movable
                    ? new Point2(movable.Position.X, movable.Position.Y)
                    : null);
        }

        /// <summary>
        /// Creates a transformation matrix from the selection properties
        /// defined by the specified engine object.
        /// </summary>
        /// <param name="obj">
        /// The engine object used to create the transformation matrix.
        /// </param>
        /// <returns>
        /// A transformation matrix containing the object's size, scale,
        /// rotation, rotation pivot, and position.
        /// </returns>
        public static Matrix ToMatrix(
            this IEngineObject obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            return CreateMatrix(
                obj is ISelectionResizable2 resizable
                    ? resizable.Size
                    : null,
                obj is ISelectionScalable2 scalable
                    ? scalable.Scale
                    : null,
                obj is ISelectionRotatable2 rotatable
                    ? (rotatable.Rotation, rotatable.RotationPivot)
                    : null,
                obj is ISelectionMovable2 movable
                    ? new Point2(movable.Position.X, movable.Position.Y)
                    : null);
        }

        /// <summary>
        /// Creates a transformation matrix from the selection properties
        /// defined by the specified definition.
        /// </summary>
        /// <param name="definition">
        /// The definition used to create the transformation matrix.
        /// </param>
        /// <returns>
        /// A transformation matrix containing the definition's size, scale,
        /// rotation, rotation pivot, and position.
        /// </returns>
        public static Matrix ToMatrix(
            this IDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            return CreateDefinitionMatrix(
                definition is ISelectionResizable2Definition resizable
                    ? resizable.Size
                    : null,
                definition is ISelectionScalable2Definition scalable
                    ? scalable.Scale
                    : null,
                definition is ISelectionRotatable2Definition rotatable
                    ? (rotatable.Rotation, rotatable.RotationPivot)
                    : null,
                definition is ISelectionMovable2Definition movable
                    ? new Point2(movable.Position.X, movable.Position.Y)
                    : null);
        }

        /// <summary>
        /// Creates a transformation matrix from the selection properties
        /// defined by the specified selection target definition.
        /// </summary>
        /// <param name="definition">
        /// The selection target definition used to create the transformation matrix.
        /// </param>
        /// <returns>
        /// A transformation matrix containing the definition's size, scale,
        /// rotation, rotation pivot, and position.
        /// </returns>
        public static Matrix ToMatrix(
            this ISelectionTargetDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            return CreateDefinitionMatrix(
                definition is ISelectionResizable2Definition resizable
                    ? resizable.Size
                    : null,
                definition is ISelectionScalable2Definition scalable
                    ? scalable.Scale
                    : null,
                definition is ISelectionRotatable2Definition rotatable
                    ? (rotatable.Rotation, rotatable.RotationPivot)
                    : null,
                definition is ISelectionMovable2Definition movable
                    ? new Point2(movable.Position.X, movable.Position.Y)
                    : null);
        }

        /// <summary>
        /// Fits the position and size of the specified selection target definition
        /// to the supplied world-space geometry while preserving its scale,
        /// rotation, and normalized rotation pivot.
        /// </summary>
        /// <param name="definition">
        /// The selection target definition whose position and size are updated.
        /// </param>
        /// <param name="polygons">
        /// The world-space vertices used to calculate the fitted selection bounds.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when at least one vertex was available and the
        /// definition could be fitted; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool FitBoundsPreserveRotation(
            this ISelectionTarget2Definition definition,
            IEnumerable<IReadOnlyList<Vector2>> polygons)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(polygons);

            if (definition is not ISelectionMovable2Definition movable ||
                definition is not ISelectionResizable2Definition resizable)
            {
                return false;
            }

            Vector2 scale = definition is ISelectionScalable2Definition scalable
                ? scalable.Scale
                : Vector2.One;

            if (MathF.Abs(scale.X) <= float.Epsilon ||
                MathF.Abs(scale.Y) <= float.Epsilon)
            {
                return false;
            }

            float rotation = 0f;
            Point2 rotationPivot = Point2.Zero;

            if (definition is ISelectionRotatable2Definition rotatable)
            {
                rotation = rotatable.Rotation;
                rotationPivot = rotatable.RotationPivot;
            }

            Matrix inverseRotation = Matrix.CreateRotationZ(-rotation);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            bool hasVertex = false;

            foreach (var polygon in polygons)
            {
                if (polygon == null)
                    continue;

                for (int i = 0; i < polygon.Count; i++)
                {
                    Vector2 point = Vector2.Transform(
                        polygon[i],
                        inverseRotation);

                    minX = MathF.Min(minX, point.X);
                    minY = MathF.Min(minY, point.Y);
                    maxX = MathF.Max(maxX, point.X);
                    maxY = MathF.Max(maxY, point.Y);
                    hasVertex = true;
                }
            }

            if (!hasVertex)
                return false;

            float width = (maxX - minX) / MathF.Abs(scale.X);
            float height = (maxY - minY) / MathF.Abs(scale.Y);

            var size = new Size2(width, height);

            Vector2 scaledPivot = new Vector2(
                width * scale.X * rotationPivot.X,
                height * scale.Y * rotationPivot.Y);

            Vector2 localOrigin = new Vector2(
                scale.X >= 0f ? minX : maxX,
                scale.Y >= 0f ? minY : maxY);

            Vector2 position =
                Vector2.Transform(
                    localOrigin + scaledPivot,
                    Matrix.CreateRotationZ(rotation)) -
                scaledPivot;

            resizable.Size = size;
            movable.Position = new Point2(position.X, position.Y);
            return true;
        }

        /// <summary>
        /// Creates a transformation matrix from the specified selection properties.
        /// </summary>
        /// <param name="size">
        /// The size of the selection target.
        /// </param>
        /// <param name="scale">
        /// The scale applied to the selection target.
        /// </param>
        /// <param name="rotation">
        /// The rotation and normalized rotation pivot of the selection target.
        /// </param>
        /// <param name="position">
        /// The position of the selection target.
        /// </param>
        /// <returns>
        /// The resulting transformation matrix.
        /// </returns>
        private static Matrix CreateMatrix(
            Size2? size,
            Vector2? scale,
            (float Rotation, Point2 Pivot)? rotation,
            Point2? position)
        {
            var matrix = Matrix.Identity;

            var actualSize = size.HasValue
                ? size.Value.ToVector2()
                : Vector2.One;

            if (size.HasValue)
            {
                matrix *= Matrix.CreateScale(
                    new Vector3(
                        size.Value.Width,
                        size.Value.Height,
                        1f));
            }

            if (scale.HasValue)
            {
                matrix *= Matrix.CreateScale(
                    new Vector3(
                        scale.Value,
                        1f));

                actualSize *= scale.Value;
            }

            if (rotation.HasValue)
            {
                var pivot =
                    actualSize *
                    rotation.Value.Pivot.ToVector2();

                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        -pivot,
                        0f));

                matrix *= Matrix.CreateRotationZ(
                    rotation.Value.Rotation);

                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        pivot,
                        0f));
            }

            if (position.HasValue)
            {
                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        position.Value.ToVector2(),
                        0f));
            }

            return matrix;
        }

        /// <summary>
        /// Creates a transformation matrix from the specified definition properties.
        /// </summary>
        /// <param name="size">
        /// The size defined by the selection target definition.
        /// </param>
        /// <param name="scale">
        /// The scale defined by the selection target definition.
        /// </param>
        /// <param name="rotation">
        /// The rotation and rotation pivot defined by the selection target definition.
        /// </param>
        /// <param name="position">
        /// The position defined by the selection target definition.
        /// </param>
        /// <returns>
        /// The resulting transformation matrix.
        /// </returns>
        private static Matrix CreateDefinitionMatrix(
            Size2? size,
            Vector2? scale,
            (float Rotation, Point2 Pivot)? rotation,
            Point2? position)
        {
            var matrix = Matrix.Identity;

            var actualSize = size.HasValue
                ? size.Value.ToVector2()
                : Vector2.One;

            if (size.HasValue)
            {
                matrix *= Matrix.CreateScale(
                    new Vector3(
                        size.Value.Width,
                        size.Value.Height,
                        1f));
            }

            if (scale.HasValue)
            {
                matrix *= Matrix.CreateScale(
                    new Vector3(
                        scale.Value,
                        1f));

                actualSize *= scale.Value;
            }

            if (rotation.HasValue)
            {
                var pivot =
                    actualSize *
                    rotation.Value.Pivot.ToVector2();

                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        -pivot,
                        0f));

                matrix *= Matrix.CreateRotationZ(
                    rotation.Value.Rotation);

                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        pivot,
                        0f));
            }

            if (position.HasValue)
            {
                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        position.Value.ToVector2(),
                        0f));
            }

            return matrix;
        }
    }
}