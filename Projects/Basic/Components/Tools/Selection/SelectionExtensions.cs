using System;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Provides extension methods for working with selection targets and selection capabilities.
    /// </summary>
    public static class SelectionExtensions
    {
        // ---------------------------------------------------------------------
        // Selection Target
        // ---------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified selection target is selected.
        /// </summary>
        public static bool IsSelected(this ISelectionTarget target)
        {
            return target.IsSelected;
        }

        /// <summary>
        /// Determines whether the specified object is selected.
        /// </summary>
        public static bool IsSelected(this IEngineObject obj)
        {
            return obj is ISelectionTarget target && target.IsSelected;
        }

        /// <summary>
        /// Determines whether the specified definition is selected.
        /// </summary>
        public static bool IsSelected(this IDefinition definition)
        {
            return definition is ISelectionTargetDefinition target && target.IsSelected;
        }

        /// <summary>
        /// Selects the specified selection target.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the selection target is locked.
        /// </exception>
        public static void Select(this ISelectionTarget target)
        {
            if (target.IsLocked)
                throw new InvalidOperationException(
                    "The selection target is locked.");

            target.IsSelected = true;
        }

        /// <summary>
        /// Selects the specified object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the object does not support selection or is locked.
        /// </exception>
        public static void Select(this IEngineObject obj)
        {
            if (obj is not ISelectionTarget target)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support selection.");

            target.Select();
        }

        /// <summary>
        /// Selects the specified definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the definition does not support selection or is locked.
        /// </exception>
        public static void Select(this IDefinition definition)
        {
            if (definition is not ISelectionTargetDefinition target)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support selection.");

            if (target.IsLocked)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' is locked.");

            target.IsSelected = true;
        }

        /// <summary>
        /// Attempts to select the specified selection target.
        /// </summary>
        public static bool TrySelect(this ISelectionTarget target)
        {
            if (target.IsLocked)
                return false;

            target.IsSelected = true;
            return true;
        }

        /// <summary>
        /// Attempts to select the specified object.
        /// </summary>
        public static bool TrySelect(this IEngineObject obj)
        {
            return obj is ISelectionTarget target && target.TrySelect();
        }

        /// <summary>
        /// Attempts to select the specified definition.
        /// </summary>
        public static bool TrySelect(this IDefinition definition)
        {
            if (definition is not ISelectionTargetDefinition target)
                return false;

            return target.IsLocked
                ? false
                : target.TrySetSelected(true);
        }

        /// <summary>
        /// Deselects the specified selection target.
        /// </summary>
        public static void Deselect(this ISelectionTarget target)
        {
            target.IsSelected = false;
        }

        /// <summary>
        /// Deselects the specified object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the object does not support selection.
        /// </exception>
        public static void Deselect(this IEngineObject obj)
        {
            if (obj is not ISelectionTarget target)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support selection.");

            target.Deselect();
        }

        /// <summary>
        /// Deselects the specified definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the definition does not support selection.
        /// </exception>
        public static void Deselect(this IDefinition definition)
        {
            if (definition is not ISelectionTargetDefinition target)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support selection.");

            target.IsSelected = false;
        }

        /// <summary>
        /// Attempts to deselect the specified selection target.
        /// </summary>
        public static bool TryDeselect(this ISelectionTarget target)
        {
            target.IsSelected = false;
            return true;
        }

        /// <summary>
        /// Attempts to deselect the specified object.
        /// </summary>
        public static bool TryDeselect(this IEngineObject obj)
        {
            return obj is ISelectionTarget target && target.TryDeselect();
        }

        /// <summary>
        /// Attempts to deselect the specified definition.
        /// </summary>
        public static bool TryDeselect(this IDefinition definition)
        {
            if (definition is not ISelectionTargetDefinition target)
                return false;

            target.IsSelected = false;
            return true;
        }

        /// <summary>
        /// Determines whether the specified selection target is locked.
        /// </summary>
        public static bool IsLocked(this ISelectionTarget target)
        {
            return target.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified object is locked.
        /// </summary>
        public static bool IsLocked(this IEngineObject obj)
        {
            return obj is ISelectionTarget target && target.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified definition is locked.
        /// </summary>
        public static bool IsLocked(this IDefinition definition)
        {
            return definition is ISelectionTargetDefinition target && target.IsLocked;
        }

        // ---------------------------------------------------------------------
        // Lock
        // ---------------------------------------------------------------------

        /// <summary>
        /// Locks the specified selection target.
        /// </summary>
        public static void Lock(this ISelectionTarget target)
        {
            target.IsLocked = true;
        }

        /// <summary>
        /// Locks the specified object.
        /// </summary>
        public static void Lock(this IEngineObject obj)
        {
            if (obj is not ISelectionTarget target)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support selection.");

            target.IsLocked = true;
        }

        /// <summary>
        /// Locks the specified definition.
        /// </summary>
        public static void Lock(this IDefinition definition)
        {
            if (definition is not ISelectionTargetDefinition target)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support selection.");

            target.IsLocked = true;
        }

        /// <summary>
        /// Attempts to lock the specified selection target.
        /// </summary>
        public static bool TryLock(this ISelectionTarget target)
        {
            target.IsLocked = true;
            return true;
        }

        /// <summary>
        /// Attempts to lock the specified object.
        /// </summary>
        public static bool TryLock(this IEngineObject obj)
        {
            if (obj is not ISelectionTarget target)
                return false;

            target.IsLocked = true;
            return true;
        }

        /// <summary>
        /// Attempts to lock the specified definition.
        /// </summary>
        public static bool TryLock(this IDefinition definition)
        {
            if (definition is not ISelectionTargetDefinition target)
                return false;

            target.IsLocked = true;
            return true;
        }

        /// <summary>
        /// Unlocks the specified selection target.
        /// </summary>
        public static void Unlock(this ISelectionTarget target)
        {
            target.IsLocked = false;
        }

        /// <summary>
        /// Unlocks the specified object.
        /// </summary>
        public static void Unlock(this IEngineObject obj)
        {
            if (obj is not ISelectionTarget target)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support selection.");

            target.IsLocked = false;
        }

        /// <summary>
        /// Unlocks the specified definition.
        /// </summary>
        public static void Unlock(this IDefinition definition)
        {
            if (definition is not ISelectionTargetDefinition target)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support selection.");

            target.IsLocked = false;
        }

        /// <summary>
        /// Attempts to unlock the specified selection target.
        /// </summary>
        public static bool TryUnlock(this ISelectionTarget target)
        {
            target.IsLocked = false;
            return true;
        }

        /// <summary>
        /// Attempts to unlock the specified object.
        /// </summary>
        public static bool TryUnlock(this IEngineObject obj)
        {
            if (obj is not ISelectionTarget target)
                return false;

            target.IsLocked = false;
            return true;
        }

        /// <summary>
        /// Attempts to unlock the specified definition.
        /// </summary>
        public static bool TryUnlock(this IDefinition definition)
        {
            if (definition is not ISelectionTargetDefinition target)
                return false;

            target.IsLocked = false;
            return true;
        }

        // ---------------------------------------------------------------------
        // Move
        // ---------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified selection target can be moved.
        /// </summary>
        public static bool IsMovable(this ISelectionTarget target)
        {
            return target is ISelectionMovable2 movable
                && movable.AllowMove
                && !target.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified object can be moved by the Selection Tool.
        /// </summary>
        public static bool IsMovable(this IEngineObject obj)
        {
            return obj is ISelectionMovable2 movable
                && movable.AllowMove
                && !movable.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified definition can be moved by the Selection Tool.
        /// </summary>
        public static bool IsMovable(this IDefinition definition)
        {
            return definition is ISelectionMovable2Definition movable
                && !movable.IsLocked;
        }

        /// <summary>
        /// Moves the specified selection target to the given position.
        /// </summary>
        public static void Move(this ISelectionTarget target, Point2 position)
        {
            if (target is not ISelectionMovable2 movable)
                throw new InvalidOperationException(
                    "The selection target does not support movement.");

            if (target.IsLocked)
                throw new InvalidOperationException(
                    "The selection target is locked.");

            if (!movable.AllowMove)
                throw new InvalidOperationException(
                    "The selection target does not allow movement.");

            movable.Position = position;
        }

        /// <summary>
        /// Moves the specified object to the given position.
        /// </summary>
        public static void Move(this IEngineObject obj, Point2 position)
        {
            if (obj is not ISelectionMovable2 movable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support movement.");

            if (movable.IsLocked)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' is locked.");

            if (!movable.AllowMove)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not allow movement.");

            movable.Position = position;
        }

        /// <summary>
        /// Moves the specified definition to the given position.
        /// </summary>
        public static void Move(this IDefinition definition, Point2 position)
        {
            if (definition is not ISelectionMovable2Definition movable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support movement.");

            if (movable.IsLocked)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' is locked.");

            movable.Position = position;
        }

        /// <summary>
        /// Attempts to move the specified selection target to the given position.
        /// </summary>
        public static bool TryMove(this ISelectionTarget target, Point2 position)
        {
            if (target is not ISelectionMovable2 movable
                || target.IsLocked
                || !movable.AllowMove)
                return false;

            movable.Position = position;
            return true;
        }

        /// <summary>
        /// Attempts to move the specified object to the given position.
        /// </summary>
        public static bool TryMove(this IEngineObject obj, Point2 position)
        {
            return obj is ISelectionMovable2 movable
                && !movable.IsLocked
                && movable.AllowMove
                && SetPosition(movable, position);
        }

        /// <summary>
        /// Attempts to move the specified definition to the given position.
        /// </summary>
        public static bool TryMove(this IDefinition definition, Point2 position)
        {
            if (definition is not ISelectionMovable2Definition movable
                || movable.IsLocked)
                return false;

            movable.Position = position;
            return true;
        }

        // ---------------------------------------------------------------------
        // Rotate
        // ---------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified selection target can be rotated.
        /// </summary>
        public static bool IsRotatable(this ISelectionTarget target)
        {
            return target is ISelectionRotatable2 rotatable
                && rotatable.AllowRotate
                && !target.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified object can be rotated by the Selection Tool.
        /// </summary>
        public static bool IsRotatable(this IEngineObject obj)
        {
            return obj is ISelectionRotatable2 rotatable
                && rotatable.AllowRotate
                && !rotatable.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified definition can be rotated by the Selection Tool.
        /// </summary>
        public static bool IsRotatable(this IDefinition definition)
        {
            return definition is ISelectionRotatable2Definition rotatable
                && !rotatable.IsLocked;
        }

        /// <summary>
        /// Rotates the specified selection target to the given rotation.
        /// </summary>
        public static void Rotate(this ISelectionTarget target, float rotation)
        {
            if (target is not ISelectionRotatable2 rotatable)
                throw new InvalidOperationException(
                    "The selection target does not support rotation.");

            if (target.IsLocked)
                throw new InvalidOperationException(
                    "The selection target is locked.");

            if (!rotatable.AllowRotate)
                throw new InvalidOperationException(
                    "The selection target does not allow rotation.");

            rotatable.Rotation = rotation;
        }

        /// <summary>
        /// Rotates the specified selection target to the given rotation around the specified pivot point.
        /// </summary>
        public static void Rotate(
            this ISelectionTarget target,
            float rotation,
            Point2 rotationPivot)
        {
            if (target is not ISelectionRotatable2 rotatable)
                throw new InvalidOperationException(
                    "The selection target does not support rotation.");

            if (target.IsLocked)
                throw new InvalidOperationException(
                    "The selection target is locked.");

            if (!rotatable.AllowRotate)
                throw new InvalidOperationException(
                    "The selection target does not allow rotation.");

            rotatable.RotationPivot = rotationPivot;
            rotatable.Rotation = rotation;
        }

        /// <summary>
        /// Rotates the specified object to the given rotation.
        /// </summary>
        public static void Rotate(this IEngineObject obj, float rotation)
        {
            if (obj is not ISelectionRotatable2 rotatable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support rotation.");

            if (rotatable.IsLocked)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' is locked.");

            if (!rotatable.AllowRotate)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not allow rotation.");

            rotatable.Rotation = rotation;
        }

        /// <summary>
        /// Rotates the specified object to the given rotation around the specified pivot point.
        /// </summary>
        public static void Rotate(
            this IEngineObject obj,
            float rotation,
            Point2 rotationPivot)
        {
            if (obj is not ISelectionRotatable2 rotatable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support rotation.");

            if (rotatable.IsLocked)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' is locked.");

            if (!rotatable.AllowRotate)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not allow rotation.");

            rotatable.RotationPivot = rotationPivot;
            rotatable.Rotation = rotation;
        }

        /// <summary>
        /// Rotates the specified definition to the given rotation.
        /// </summary>
        public static void Rotate(this IDefinition definition, float rotation)
        {
            if (definition is not ISelectionRotatable2Definition rotatable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support rotation.");

            if (rotatable.IsLocked)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' is locked.");

            rotatable.Rotation = rotation;
        }

        /// <summary>
        /// Rotates the specified definition to the given rotation around the specified pivot point.
        /// </summary>
        public static void Rotate(
            this IDefinition definition,
            float rotation,
            Point2 rotationPivot)
        {
            if (definition is not ISelectionRotatable2Definition rotatable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support rotation.");

            if (rotatable.IsLocked)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' is locked.");

            rotatable.RotationPivot = rotationPivot;
            rotatable.Rotation = rotation;
        }

        /// <summary>
        /// Attempts to rotate the specified selection target to the given rotation.
        /// </summary>
        public static bool TryRotate(this ISelectionTarget target, float rotation)
        {
            if (target is not ISelectionRotatable2 rotatable
                || target.IsLocked
                || !rotatable.AllowRotate)
                return false;

            rotatable.Rotation = rotation;
            return true;
        }

        /// <summary>
        /// Attempts to rotate the specified selection target to the given rotation around the specified pivot point.
        /// </summary>
        public static bool TryRotate(
            this ISelectionTarget target,
            float rotation,
            Point2 rotationPivot)
        {
            if (target is not ISelectionRotatable2 rotatable
                || target.IsLocked
                || !rotatable.AllowRotate)
                return false;

            rotatable.RotationPivot = rotationPivot;
            rotatable.Rotation = rotation;

            return true;
        }

        /// <summary>
        /// Attempts to rotate the specified object to the given rotation.
        /// </summary>
        public static bool TryRotate(this IEngineObject obj, float rotation)
        {
            if (obj is not ISelectionRotatable2 rotatable
                || rotatable.IsLocked
                || !rotatable.AllowRotate)
                return false;

            rotatable.Rotation = rotation;
            return true;
        }

        /// <summary>
        /// Attempts to rotate the specified object to the given rotation around the specified pivot point.
        /// </summary>
        public static bool TryRotate(
            this IEngineObject obj,
            float rotation,
            Point2 rotationPivot)
        {
            if (obj is not ISelectionRotatable2 rotatable
                || rotatable.IsLocked
                || !rotatable.AllowRotate)
                return false;

            rotatable.RotationPivot = rotationPivot;
            rotatable.Rotation = rotation;

            return true;
        }

        /// <summary>
        /// Attempts to rotate the specified definition to the given rotation.
        /// </summary>
        public static bool TryRotate(this IDefinition definition, float rotation)
        {
            if (definition is not ISelectionRotatable2Definition rotatable
                || rotatable.IsLocked)
                return false;

            rotatable.Rotation = rotation;
            return true;
        }

        /// <summary>
        /// Attempts to rotate the specified definition to the given rotation around the specified pivot point.
        /// </summary>
        public static bool TryRotate(
            this IDefinition definition,
            float rotation,
            Point2 rotationPivot)
        {
            if (definition is not ISelectionRotatable2Definition rotatable
                || rotatable.IsLocked)
                return false;

            rotatable.RotationPivot = rotationPivot;
            rotatable.Rotation = rotation;

            return true;
        }

        // ---------------------------------------------------------------------
        // Scale
        // ---------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified selection target can be scaled.
        /// </summary>
        public static bool IsScalable(this ISelectionTarget target)
        {
            return target is ISelectionScalable2 scalable
                && scalable.AllowScale
                && !target.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified object can be scaled by the Selection Tool.
        /// </summary>
        public static bool IsScalable(this IEngineObject obj)
        {
            return obj is ISelectionScalable2 scalable
                && scalable.AllowScale
                && !scalable.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified definition can be scaled by the Selection Tool.
        /// </summary>
        public static bool IsScalable(this IDefinition definition)
        {
            return definition is ISelectionScalable2Definition scalable
                && !scalable.IsLocked;
        }

        /// <summary>
        /// Scales the specified selection target to the given scale.
        /// </summary>
        public static void Scale(this ISelectionTarget target, Vector2 scale)
        {
            if (target is not ISelectionScalable2 scalable)
                throw new InvalidOperationException(
                    "The selection target does not support scaling.");

            if (target.IsLocked)
                throw new InvalidOperationException(
                    "The selection target is locked.");

            if (!scalable.AllowScale)
                throw new InvalidOperationException(
                    "The selection target does not allow scaling.");

            scalable.Scale = scale;
        }

        /// <summary>
        /// Scales the specified object to the given scale.
        /// </summary>
        public static void Scale(this IEngineObject obj, Vector2 scale)
        {
            if (obj is not ISelectionScalable2 scalable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support scaling.");

            if (scalable.IsLocked)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' is locked.");

            if (!scalable.AllowScale)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not allow scaling.");

            scalable.Scale = scale;
        }

        /// <summary>
        /// Scales the specified definition to the given scale.
        /// </summary>
        public static void Scale(this IDefinition definition, Vector2 scale)
        {
            if (definition is not ISelectionScalable2Definition scalable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support scaling.");

            if (scalable.IsLocked)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' is locked.");

            scalable.Scale = scale;
        }

        /// <summary>
        /// Attempts to scale the specified selection target to the given scale.
        /// </summary>
        public static bool TryScale(this ISelectionTarget target, Vector2 scale)
        {
            if (target is not ISelectionScalable2 scalable
                || target.IsLocked
                || !scalable.AllowScale)
                return false;

            scalable.Scale = scale;
            return true;
        }

        /// <summary>
        /// Attempts to scale the specified object to the given scale.
        /// </summary>
        public static bool TryScale(this IEngineObject obj, Vector2 scale)
        {
            if (obj is not ISelectionScalable2 scalable
                || scalable.IsLocked
                || !scalable.AllowScale)
                return false;

            scalable.Scale = scale;
            return true;
        }

        /// <summary>
        /// Attempts to scale the specified definition to the given scale.
        /// </summary>
        public static bool TryScale(this IDefinition definition, Vector2 scale)
        {
            if (definition is not ISelectionScalable2Definition scalable
                || scalable.IsLocked)
                return false;

            scalable.Scale = scale;
            return true;
        }

        // ---------------------------------------------------------------------
        // Resize
        // ---------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified selection target can be resized.
        /// </summary>
        public static bool IsResizable(this ISelectionTarget target)
        {
            return target is ISelectionResizable2 resizable
                && resizable.AllowResize
                && !target.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified object can be resized by the Selection Tool.
        /// </summary>
        public static bool IsResizable(this IEngineObject obj)
        {
            return obj is ISelectionResizable2 resizable
                && resizable.AllowResize
                && !resizable.IsLocked;
        }

        /// <summary>
        /// Determines whether the specified definition can be resized by the Selection Tool.
        /// </summary>
        public static bool IsResizable(this IDefinition definition)
        {
            return definition is ISelectionResizable2Definition resizable
                && !resizable.IsLocked;
        }

        /// <summary>
        /// Resizes the specified selection target to the given size.
        /// </summary>
        public static void Resize(this ISelectionTarget target, Size2 size)
        {
            if (target is not ISelectionResizable2 resizable)
                throw new InvalidOperationException(
                    "The selection target does not support resizing.");

            if (target.IsLocked)
                throw new InvalidOperationException(
                    "The selection target is locked.");

            if (!resizable.AllowResize)
                throw new InvalidOperationException(
                    "The selection target does not allow resizing.");

            resizable.Size = size;
        }

        /// <summary>
        /// Resizes the specified object to the given size.
        /// </summary>
        public static void Resize(this IEngineObject obj, Size2 size)
        {
            if (obj is not ISelectionResizable2 resizable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support resizing.");

            if (resizable.IsLocked)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' is locked.");

            if (!resizable.AllowResize)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not allow resizing.");

            resizable.Size = size;
        }

        /// <summary>
        /// Resizes the specified definition to the given size.
        /// </summary>
        public static void Resize(this IDefinition definition, Size2 size)
        {
            if (definition is not ISelectionResizable2Definition resizable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support resizing.");

            if (resizable.IsLocked)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' is locked.");

            resizable.Size = size;
        }

        /// <summary>
        /// Attempts to resize the specified selection target to the given size.
        /// </summary>
        public static bool TryResize(this ISelectionTarget target, Size2 size)
        {
            if (target is not ISelectionResizable2 resizable
                || target.IsLocked
                || !resizable.AllowResize)
                return false;

            resizable.Size = size;
            return true;
        }

        /// <summary>
        /// Attempts to resize the specified object to the given size.
        /// </summary>
        public static bool TryResize(this IEngineObject obj, Size2 size)
        {
            if (obj is not ISelectionResizable2 resizable
                || resizable.IsLocked
                || !resizable.AllowResize)
                return false;

            resizable.Size = size;
            return true;
        }

        /// <summary>
        /// Attempts to resize the specified definition to the given size.
        /// </summary>
        public static bool TryResize(this IDefinition definition, Size2 size)
        {
            if (definition is not ISelectionResizable2Definition resizable
                || resizable.IsLocked)
                return false;

            resizable.Size = size;
            return true;
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private static bool SetPosition(
            ISelectionMovable2 movable,
            Point2 position)
        {
            movable.Position = position;
            return true;
        }

        private static bool TrySetSelected(
            this ISelectionTargetDefinition target,
            bool value)
        {
            target.IsSelected = value;
            return true;
        }
    }
}