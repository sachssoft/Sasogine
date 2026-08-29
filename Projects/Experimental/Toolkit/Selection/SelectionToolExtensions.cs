using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
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
                target is ISelectionResizable resizable ? resizable.Size : null,
                target is ISelectionScalable scalable ? scalable.Scale : null,
                target is ISelectionRotatable rotatable
                    ? (rotatable.Rotation, rotatable.RotationPivot)
                    : null,
                target is ISelectionMovable movable ? movable.Position : null);
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
                obj is ISelectionResizable resizable ? resizable.Size : null,
                obj is ISelectionScalable scalable ? scalable.Scale : null,
                obj is ISelectionRotatable rotatable
                    ? (rotatable.Rotation, rotatable.RotationPivot)
                    : null,
                obj is ISelectionMovable movable ? movable.Position : null);
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
                definition is ISelectionResizableDefinition resizable
                    ? resizable.Size
                    : null,
                definition is ISelectionScalableDefinition scalable
                    ? scalable.Scale
                    : null,
                definition is ISelectionRotatableDefinition rotatable
                    ? (rotatable.Rotation, rotatable.RotationPivot)
                    : null,
                definition is ISelectionMovableDefinition movable
                    ? movable.Position
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
                definition is ISelectionResizableDefinition resizable
                    ? resizable.Size
                    : null,
                definition is ISelectionScalableDefinition scalable
                    ? scalable.Scale
                    : null,
                definition is ISelectionRotatableDefinition rotatable
                    ? (rotatable.Rotation, rotatable.RotationPivot)
                    : null,
                definition is ISelectionMovableDefinition movable
                    ? movable.Position
                    : null);
        }

        private static Matrix CreateMatrix(
            Size? size,
            Vector2? scale,
            (float Rotation, Vector2 Pivot)? rotation,
            Vector2? position)
        {
            var matrix = Matrix.Identity;

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
            }

            if (rotation.HasValue)
            {
                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        -rotation.Value.Pivot,
                        0f));

                matrix *= Matrix.CreateRotationZ(
                    rotation.Value.Rotation);

                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        rotation.Value.Pivot,
                        0f));
            }

            if (position.HasValue)
            {
                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        position.Value,
                        0f));
            }

            return matrix;
        }

        private static Matrix CreateDefinitionMatrix(
            Size? size,
            Vector2? scale,
            (float Rotation, Vector2 Pivot)? rotation,
            Vector2? position)
        {
            var matrix = Matrix.Identity;

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
            }

            if (rotation.HasValue)
            {
                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        -rotation.Value.Pivot,
                        0f));

                matrix *= Matrix.CreateRotationZ(
                    rotation.Value.Rotation);

                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        rotation.Value.Pivot,
                        0f));
            }

            if (position.HasValue)
            {
                matrix *= Matrix.CreateTranslation(
                    new Vector3(
                        position.Value,
                        0f));
            }

            return matrix;
        }
    }
}