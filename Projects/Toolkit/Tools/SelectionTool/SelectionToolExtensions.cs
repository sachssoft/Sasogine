using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Components.Tools.Selection
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
            (float Rotation, Vector2 Pivot)? rotation,
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
                    rotation.Value.Pivot;

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